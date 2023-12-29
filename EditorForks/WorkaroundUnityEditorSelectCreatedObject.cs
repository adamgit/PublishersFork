using UnityEngine;
using UnityEditor;

namespace UnityEditorForks
{
	/// <summary>
	/// After you create an object in Editor, in 99.9% of cases you immediately want to let the user rename it - Unity's
	/// staff themselves make this the default behaviour for all Unity code, but so far have refused to let anyone else do it directly,
	/// so we have to ping the (public!) methods, and implement some (required!) artificial delays.
	///
	/// Usage:
	/// 	1. Create a new object in script - e.g. "var go = new GameObject()"
	/// 	2. Call "go.SelectAndRename();"
	///
	/// ...this will cause the new object to be automatically selected in the Editor *and* the UI built-in 'rename new object'
	/// interface to be triggered, allowing the user to interactively name it without wasting time and keystrokes.
	/// </summary>
	public static class WorkaroundUnityEditorSelectCreatedObject
	{
		public static void SelectAndRename( this GameObject go )
		{
			Selection.activeGameObject = go;

#if UNITY_2021_1_OR_NEWER
		EditorApplication.update += Delay1Frame_Rename;
#else // Unity broke this in Unity 2021-ish; since approx Unity-2021 you need to use the other approach
			// Workaround for 10+ years bug that Unity still doesn't let you select things in Hierarchy:
			((EditorWindow)Resources.FindObjectsOfTypeAll( typeof(UnityEditor.Editor).Assembly.GetType( "UnityEditor.SceneHierarchyWindow" ) )[0]).Focus();
#endif
		}
		
		/// Unity engineers couldn't make 'create object' execute correctly within a single frame, requiring MULTIPLE FRAMES before you can call the 'real' edit-name method
		private static void Delay1Frame_Rename()
		{
			EditorApplication.update -= Delay1Frame_Rename;
			EditorApplication.update += Rename;
		}

		static void Rename()
		{
			EditorApplication.update -= Rename;
			((EditorWindow)Resources.FindObjectsOfTypeAll( typeof(UnityEditor.Editor).Assembly.GetType( "UnityEditor.SceneHierarchyWindow" ) )[0]).Focus();
			EditorApplication.ExecuteMenuItem( "Window/General/Hierarchy" );
			EditorApplication.ExecuteMenuItem( "Edit/Rename" );
		}
	}
}