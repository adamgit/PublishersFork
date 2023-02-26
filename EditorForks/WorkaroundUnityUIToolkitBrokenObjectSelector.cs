using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditorForks
{
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
			var hiddenType = typeof(UnityEditor.Editor).Assembly.GetType( "UnityEditor.ObjectSelector" );
			var ps = hiddenType.GetProperties( BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty );
			//Debug.Log( "all props on "+(typeof(ObjectSelector))+": "+string.Join( ",\n", ps.Select( info => info.ToString() ) ) );
			PropertyInfo piGet = hiddenType.GetProperty( "get", BindingFlags.Public | BindingFlags.Static );
			var os = piGet.GetValue( null );

			MethodInfo miShow = null;
			
			var unityVersion = UnityEngine.Application.unityVersion.Split( '.' );

			/**
			 * If definitely Unity-2019 or earlier, use the old version; for all others use the new version (so that this
			 * is forwards-compatible with when Unity Marketing inevitably breaks all Unity version comparisons AGAIN while
			 * the Unity Engineering team AGAIN fails to provide a working 'UnityVersion' API (come on, guys! It's not hard!
			 * and only YOU can maintain it correctly, since you're the ones putting incomparable values into it!)
			 */
			if( int.TryParse( unityVersion[0], out int unityMajorVersion ) && unityMajorVersion < 2020 )
			{
				/** Type 1: Unity - up to 2019 */
				miShow = hiddenType.GetMethod( "Show", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]
				{
					typeof(T),
					typeof(System.Type),
					typeof(SerializedProperty),
					typeof(bool),
					typeof(List<int>),
					typeof(Action<UnityEngine.Object>),
					typeof(Action<UnityEngine.Object>)
				}, new ParameterModifier[0] );
			}
			else
			{
				/*** Type 2: Unity - 2020 onwards */
				miShow = hiddenType.GetMethod( "Show", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]
				{
					typeof(T),
					typeof(System.Type),
					typeof(UnityEngine.Object),
					typeof(bool),
					typeof(List<int>),
					typeof(Action<UnityEngine.Object>),
					typeof(Action<UnityEngine.Object>)
				}, new ParameterModifier[0] );
			}

			/**
			 * Something went wrong ... did Unity staff add a third class? Argh!
			 */
			if( miShow == null )
			{
				string methodName = "Show";
				Debug.LogError( "Found \"" + methodName + "\"methods: \n" + string.Join( ",\n", hiddenType.GetMethods( BindingFlags.NonPublic | BindingFlags.Instance )
					.Where( info => info.Name == methodName ).Select( info => "  " + info.Name + " {" + string.Join( ",\n      ", info.GetParameters().Select( parameterInfo => parameterInfo.Name + ":" + parameterInfo.ParameterType ) ) + "\n}\n" ) ) );

/*			Debug.LogError("Found methods: "+string.Join(",\n", hiddenType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Select(info => info.Name+"{"+
			                                                                                                                                   */
			}

			Action<UnityEngine.Object> onSelectorClosed;
			Action<UnityEngine.Object> onSelectedUpdated;
			switch( sources )
			{
				case ObjectPickerSources.ASSETS:
				case ObjectPickerSources.ASSETS_AND_SCENE:
					onSelectedUpdated = o => { OnSelectionChanged( o as T ); };
					onSelectorClosed = o => OnSelectorClosed.Invoke( o as T );
					break;
				case ObjectPickerSources.MONOBEHAVIOURS:
					onSelectedUpdated = o => OnSelectionChanged( (o as GameObject).GetComponent<T>() );
					onSelectorClosed = o => OnSelectorClosed.Invoke( (o as GameObject).GetComponent<T>() );
					break;
				default:
					throw new Exception( "Impossible value of sources parameter" );
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
}