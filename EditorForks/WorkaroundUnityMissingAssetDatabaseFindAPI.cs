using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// No-one except Unity staff can access the Find API in Unity 2019-2021, and the replacement ("use magic strings with
/// the AssetDatabase.Find methods") is extremely fragile and incorrectly documented (I logged bugs against this more than
/// a year ago, that were accepted, but Unity still hasn't fixed them - still hasn't corrected their own docs).
///
/// This class wraps the bad API call into a sensible one ... like Unity should have done. 
/// </summary>
public static class WorkaroundUnityMissingAssetDatabaseFindAPI
{
	/// <summary>
	/// Unity has an equivalent API call but for the past 3 years they've refused to let non-Unity staff access it; you
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
		var settingsFileAssets = AssetDatabase.FindAssets("t:" + typeof(T).Name);
		var foundObjects = new List<T>();
		for(int i = 0; i < settingsFileAssets.Length; i++)
		{
			foundObjects.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(settingsFileAssets[i])));
		}

		if( debugSettingsDiscovery ) Debug.Log("Found objects at: " + string.Join(",", foundObjects.Select(settings => AssetDatabase.GetAssetPath(settings))));
		
		return foundObjects;
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
		var settingsFileAssets = AssetDatabase.FindAssets("t:" + type.Name);
		var foundObjects = new List<Object>();
		for(int i = 0; i < settingsFileAssets.Length; i++)
		{
			foundObjects.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(settingsFileAssets[i]), type));
		}

		if( debugSettingsDiscovery ) Debug.Log("Found objects at: " + string.Join(",", foundObjects.Select(settings => AssetDatabase.GetAssetPath(settings))));
		
		return foundObjects;
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
		var settingsFileAssets = AssetDatabase.FindAssets("t:" + typeof(T).Name);
		return settingsFileAssets.Length > 0;
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
		var settingsFileAssets = AssetDatabase.FindAssets("t:" + t.Name);
		return settingsFileAssets.Length > 0;
	}
}