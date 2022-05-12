using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// The UIToolkit team broke the core API "ShowObjectPicker", implemented their own version, and then made it 'internal'
/// to prevent anyone else from  using the code. So we have to do this painful workaround to access the API call they're
/// supposed to make public but 'forgot' to.
///
/// Usage:
/// e.g. to pick a Material:
///    ((Material)null).ShowObjectPicker<Material>( o => { Debug.Log( "selector closed"+o.name ); }, o => { Debug.Log( "selector updated"+o.name ); } );
///
/// (or you can directly access the static method using the full class name here) 
/// </summary>
public static class WorkaroundUnityUIToolkitBrokenObjectSelector
{
	public enum ObjectPickerSources
	{
		ASSETS,
		ASSETS_AND_SCENE,
		MONOBEHAVIOURS,
	}
	
	public static void ShowObjectPicker<T>( this T initialValue, Action<T> OnSelectorClosed, Action<T> OnSelectionChanged, ObjectPickerSources sources = ObjectPickerSources.ASSETS ) where T : UnityEngine.Object
	{
		ShowObjectPicker<T>( OnSelectorClosed, OnSelectionChanged, initialValue, sources );
	}

	public static void ShowObjectPicker<T>( Action<T> OnSelectorClosed, Action<T> OnSelectionChanged, T initialValueOrNull = null, ObjectPickerSources sources = ObjectPickerSources.ASSETS ) where T : UnityEngine.Object
	{
		var hiddenType = typeof(Editor).Assembly.GetType( "UnityEditor.ObjectSelector" );
		var ps = hiddenType.GetProperties( BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
		//Debug.Log( "all props on "+(typeof(ObjectSelector))+": "+string.Join( ",\n", ps.Select( info => info.ToString() ) ) );
		PropertyInfo piGet = hiddenType.GetProperty( "get", BindingFlags.Public | BindingFlags.Static );
		var os = piGet.GetValue( null );
		
		MethodInfo miShow = null;
		//if( miShow == null )
		#if UNITY_2020_1_OR_NEWER
		/*** Type 2: Unity ? - 2020 */
		miShow = hiddenType.GetMethod( "Show", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]
		{
			typeof(T),
			typeof(	System.Type),
			typeof(UnityEngine.Object),
			typeof(bool),
			typeof(List<int>),
			typeof(Action<UnityEngine.Object>),
			typeof(Action<UnityEngine.Object>)
		}, new ParameterModifier[0] );

#else
		/*** Type 1: Unity 2019 - ? */
miShow = hiddenType.GetMethod( "Show", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]
		{
			typeof(T),
			typeof(	System.Type),
			typeof(SerializedProperty),
			typeof(bool),
			typeof(List<int>),
			typeof(Action<UnityEngine.Object>),
			typeof(Action<UnityEngine.Object>)
		}, new ParameterModifier[0] );
#endif
		
		/**
		 * Something went wrong ... did Unity staff change the API again? (already done it once...)
		 */
		if(miShow == null)
		{
			string methodName = "Show";
			Debug.LogError("Found \""+methodName+"\"methods: \n"+string.Join(",\n", hiddenType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
				               .Where(info => info.Name == methodName).Select(info => "  "+info.Name+" {"+string.Join(",\n      ",info.GetParameters().Select(parameterInfo => parameterInfo.Name+":"+parameterInfo.ParameterType))+"\n}\n")));

/*			Debug.LogError("Found methods: "+string.Join(",\n", hiddenType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Select(info => info.Name+"{"+
			                                                                                                                                    string.Join(", ",info.GetParameters().Select(
				                                                                                                                                    parameterInfo => parameterInfo.Name+":"+parameterInfo.ParameterType))+"}")));
				                                                                                                                                    
				                                                                                                                                    */
		}

		Action<UnityEngine.Object> onSelectorClosed;
		Action<UnityEngine.Object> onSelectedUpdated;
		switch(sources)
		{
			case ObjectPickerSources.ASSETS:
			case ObjectPickerSources.ASSETS_AND_SCENE:
				onSelectedUpdated = o => { OnSelectionChanged( o as T ); };
				onSelectorClosed = o => OnSelectorClosed.Invoke( o as T );
				break;
			case ObjectPickerSources.MONOBEHAVIOURS:
				onSelectedUpdated = o => OnSelectionChanged( (o as GameObject).GetComponent<T>() );
				onSelectorClosed = o => OnSelectorClosed.Invoke( (o as GameObject).GetComponent<T>());
				break;
			default:
				throw new Exception("Impossible value of sources parameter");
		}
		
		miShow.Invoke( os, new object[]
			{
				initialValueOrNull,
				typeof(T),
				null,
				(sources == ObjectPickerSources.ASSETS_AND_SCENE) || (sources == ObjectPickerSources.MONOBEHAVIOURS),
				null,
				onSelectorClosed, 
				onSelectedUpdated,
			}
		);
	}
}