using UnityEditor;
using UnityEngine;

namespace UnityEditorForks
{
	/// <summary>
	/// Unity's MenuCommand is missing a key feature: 'which GameObject did the user right-click to execute this menu?',
	/// which you typically want when using Unity's [MenuItem] attribute.
	///
	/// The code in this class is based on Unity's own API docs where they read Selection.* when command.context is null.
	/// </summary>
	public static class WorkaroundUnityMenuCommandContext
	{
		/// <summary>
		/// </summary>
		/// <param name="command"></param>
		/// <returns>The Unity-API 'command.context' if not-null, otherwise the selected GameObject from Hierarchy panel</returns>
		public static GameObject ContextGameObject( this MenuCommand command )
		{
			GameObject _candidate;
			if( command != null && command.context != null )
				_candidate = (command.context as GameObject);
			else if( Selection.activeGameObject != null )
				_candidate = Selection.activeGameObject;
			else
				_candidate = null;

			return _candidate;
		}
	}
}