using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditorForks
{
	/// <summary>
	/// Unity staff know that 'AssetDatabase.FindAssets' is broken in multiple core cases, but refuse to
	/// fix it (and keep refusing to update their docs to admit that they know it's broken - so the docs are also wrong).
	/// This class aims to fix the common cases that Unity won't. Most of these could be fixed in fewer lines of code by
	/// Unity fixing their internal classes e.g. SearchFilter - Unity staff don't seem to understand what 'internal' keyword
	/// in C# actuall means.
	///  
	/// Without heavy use of complex Reflection calls, only Unity staff can access the true Find API in Unity 2019-2023
	/// and the replacement ("use magic strings with the AssetDatabase.Find methods") is extremely fragile and incorrectly documented
	/// (I logged bugs against this more than
	/// two years ago, that were accepted, but Unity still hasn't fixed them - still hasn't corrected their own docs).
	///
	/// This class wraps the bad API call into a sensible one, and adds some obvious missing API calls they should/would
	/// have included if they'd accepted input from real users before publishing it.
	///
	/// 2023 UPDATE: this also mostly fixes the multiple-years-old bug in Unity that if the Type you're searching for doesn't exist
	/// in a file with the same literal name (due to sheer laziness by Unity staff) then Unity returns incorrect results.
	/// Unity's bug breaks all searches for T where T is itself Generic (e.g. T = MyClass<MyGeneric>). Unity can't be
	/// bothered to throw an exception, or to document this known failing - they just return wrong results instead.
	/// </summary>
	public static class WorkaroundUnityMissingAssetDatabaseFindAPI
	{
	/// <summary>
    		/// Unity's new FindAssets API fails to support type-search, it supports only 'subtypes of ScriptableObject search',
    		/// because they didn't think at all about the names or use-cases of their new API when creating it. So the obvious
    		/// search-by-type will ALWAYS fail (by design - Unity's bad design) if you need to find 'assets that contain this
    		/// type'. This method fixes that mistake in Unity's codebase by letting you search for "assets that contain
    		/// components of type T".
    		/// </summary>
    		/// <param name="debugSettingsDiscovery"></param>
    		/// <typeparam name="T"></typeparam>
    		/// <returns></returns>
    		public static List<T> FindAssetPrefabsByComponentType<T>( bool debugSettingsDiscovery = false ) where T : UnityEngine.Object
    		{
    			var prefabAssets = AssetDatabase.FindAssets( "t:prefab" );
            
    			var foundComponentInstances = new List<T>();
    			for( int i = 0; i < prefabAssets.Length; i++ )
    			{
    				if( AssetDatabase.LoadAssetAtPath<GameObject>( AssetDatabase.GUIDToAssetPath( prefabAssets[i] ) ).TryGetComponent<T>( out var component ) )
    					foundComponentInstances.Add( component );
    			}
    			
    			if( debugSettingsDiscovery ) Debug.Log( "Found objects at: " + string.Join( ",", foundComponentInstances.Select( settings => AssetDatabase.GetAssetPath( settings ) ) ) );
    
    			return foundComponentInstances;
    		}
    		
		/// <summary>
		/// Unity has an equivalent API call but for the past 4+ years they've refused to let non-Unity staff access it; you
		/// can access it via reflection and override their incorrect use of 'internal', or (their official recommendation)
		/// you can fake it using various combinations of their undocumented features (e.g. "a:" etc) of FindAssets().
		///
		/// It is particularly frustrating that we're converting a Type to a string so that Unity's code can convert it back
		/// to a Type, and re-create the <T> generic we had in the first place but which a Unity team refuses to let anyone
		/// else call directly.
		/// </summary>
		/// <param name="debugSettingsDiscovery"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> FindAssetsByType<T>( bool debugSettingsDiscovery = false ) where T : UnityEngine.Object
		{
			/** Unity's API is wrongly documented - it does NOT support 'types' it only supports 'classnames'; we have
			 * to do the conversion manually when those things differ
			 */
			if( typeof(T).IsGenericType )
			{
				var settingsFileAssets = AssetDatabase.FindAssets( "t:ScriptableObject" );
				var foundObjects = new List<T>();
				for( int i = 0; i < settingsFileAssets.Length; i++ )
				{
					var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>( AssetDatabase.GUIDToAssetPath( settingsFileAssets[i] ) );
					if( so is T ) 
						foundObjects.Add( so as T );
				}

				if( debugSettingsDiscovery ) Debug.Log( "Found " + settingsFileAssets.Length + " assets of type (" + typeof(T) + "): " + string.Join( ",", foundObjects.Select( settings => AssetDatabase.GetAssetPath( settings ) ) ) );

				return foundObjects;
			}
			else
			{
				var settingsFileAssets = AssetDatabase.FindAssets( "t:" + typeof(T).Name );
				var foundObjects = new List<T>();
				for( int i = 0; i < settingsFileAssets.Length; i++ )
				{
					foundObjects.Add( AssetDatabase.LoadAssetAtPath<T>( AssetDatabase.GUIDToAssetPath( settingsFileAssets[i] ) ) );
				}

				if( debugSettingsDiscovery ) Debug.Log( "Found objects at: " + string.Join( ",", foundObjects.Select( settings => AssetDatabase.GetAssetPath( settings ) ) ) );

				return foundObjects;
			}
		}

		/// <summary>
		/// Identical to <see cref="FindAssetsByType{T}(bool)"/> except that it's non-generic (for when you're working with
		/// runtime-resolved Type objects, instead of compile-time resolved classnames)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="debugSettingsDiscovery"></param>
		/// <returns></returns>
		public static List<Object> FindAssetsByType( Type type, bool debugSettingsDiscovery = false )
		{
			/** Unity's API is wrongly documented - it does NOT support 'types' it only supports 'classnames'; we have
			 * to do the conversion manually when those things differ
			 */
			if( type.IsGenericType )
			{
				var settingsFileAssets = AssetDatabase.FindAssets( "t:ScriptableObject" );
				var foundObjects = new List<Object>();
				for( int i = 0; i < settingsFileAssets.Length; i++ )
				{
					var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>( AssetDatabase.GUIDToAssetPath( settingsFileAssets[i] ) );
					if( type.IsAssignableFrom( so.GetType() ) ) 
						foundObjects.Add( so );
				}

				if( debugSettingsDiscovery ) Debug.Log( "Found " + settingsFileAssets.Length + " assets of type (" + type + "): " + string.Join( ",", foundObjects.Select( settings => AssetDatabase.GetAssetPath( settings ) ) ) );

				return foundObjects;
			}
			else
			{
				var settingsFileAssets = AssetDatabase.FindAssets( "t:" + type.Name );
				var foundObjects = new List<Object>();
				for( int i = 0; i < settingsFileAssets.Length; i++ )
				{
					foundObjects.Add( AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( settingsFileAssets[i] ), type ) );
				}

				if( debugSettingsDiscovery ) Debug.Log( "Found objects at: " + string.Join( ",", foundObjects.Select( settings => AssetDatabase.GetAssetPath( settings ) ) ) );

				return foundObjects;
			}
		}


		/// <summary>
		/// Convenience version of <see cref="FindAssetsByType{T}(bool)"/> so you don't have to remember the name of this class.
		///
		/// Usage:
		///
		///    foreach( var subclass in typeof(MyClass).FindAssetsByType() )
		///      ...
		/// </summary>
		/// <param name="t"></param>
		/// <param name="debugSettingsDiscovery"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> FindAssetsByType<T>( this Type t, bool debugSettingsDiscovery = false ) where T : UnityEngine.Object
		{
			return FindAssetsByType<T>( debugSettingsDiscovery );
		}

		/// <summary>
		/// Shortcut to checking <see cref="FindAssetsByType{T}"/> and seeing if it has any results, without running the code
		/// for interpreting and loading them all
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static bool ExistsAnyAssetsOfType<T>() where T : UnityEngine.Object
		{
			/** Unity's API is wrongly documented - it does NOT support 'types' it only supports 'classnames'; we have
			 * to do the conversion manually when those things differ
			 */
			if( typeof(T).IsGenericType )
			{
				var settingsFileAssets = AssetDatabase.FindAssets( "t:ScriptableObject" );
				var foundObjects = new List<T>();
				for( int i = 0; i < settingsFileAssets.Length; i++ )
				{
					var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>( AssetDatabase.GUIDToAssetPath( settingsFileAssets[i] ) );
					if( so is T )
						return true;
				}

				return false;
			}
			else
			{
				var settingsFileAssets = AssetDatabase.FindAssets( "t:" + typeof(T).Name );
				return settingsFileAssets.Length > 0;
			}
		}

		/// <summary>
		/// Convenience version of <see cref="ExistsAnyAssetsOfType{T}"/> so you don't have to remember the name of this class.
		///
		/// Usage:
		///
		///    if( typeof(MyClass).ExistsAnyAssetsOftype() )
		///      ... 
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static bool ExistsAnyAssetsOfType( this Type t )
		{
			/** Unity's API is wrongly documented - it does NOT support 'types' it only supports 'classnames'; we have
			 * to do the conversion manually when those things differ
			 */
			if( t.IsGenericType )
			{
				var settingsFileAssets = AssetDatabase.FindAssets( "t:ScriptableObject" );
				var foundObjects = new List<Object>();
				for( int i = 0; i < settingsFileAssets.Length; i++ )
				{
					var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>( AssetDatabase.GUIDToAssetPath( settingsFileAssets[i] ) );
					if( t.IsAssignableFrom( so.GetType() ) )
						return true;
				}

				return false;
			}
			else
			{
				var settingsFileAssets = AssetDatabase.FindAssets( "t:" + t.Name );
				return settingsFileAssets.Length > 0;
			}
		}
	}
}