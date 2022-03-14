using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorForks
{
	/// <summary>
	/// All versions of Unity prior to 2020.1 require this class since Unity made some mistakes when adding
	/// NestedPrefabs APIs that lead to scene/project corruption if you edit prefabs from scripts.
	/// 
	/// Based on ideas in this thread:
	///
	///   https://forum.unity.com/threads/how-do-i-edit-prefabs-from-scripts.685711/
	///
	/// Note: our implementation here is bit more useful than Unity's (it tracks Exceptions too!), so it's still useful
	/// even after Unity 2020.1.1
	///
	/// Usage:
	///
	/// public void MyMethod()
	/// {
	///    using( var safeEditing = new WorkaroundUnityPrefabEditingSafe( assetPath ) )
	///    {
	///        .. // Do your prefab editing here
	///    }
	/// }
	/// </summary>
	public class WorkaroundUnityPrefabEditingSafe
	{
		public readonly string assetPath;
		public readonly GameObject prefabRoot;
		public readonly Exception thrownException;

		public bool isValid { get { return thrownException == null; } }

		public WorkaroundUnityPrefabEditingSafe(string assetPath)
		{
			this.assetPath = assetPath;
			try
			{
				prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
			}
			catch( Exception e )
			{
				thrownException = e;
			}
		}
     
		public void Dispose()
		{
			if( prefabRoot != null )
			{
				PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
				PrefabUtility.UnloadPrefabContents(prefabRoot);
			}
		}
	}
}