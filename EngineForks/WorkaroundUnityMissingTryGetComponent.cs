using UnityEngine;

namespace PublishersFork
{
	/// <summary>
	/// Because Unity added "TryGetComponent" but failed to add the other versions of this method, even
	/// though they are obviously required (logged as a bug with Unity many years ago, but they replied that they don't
	/// want to fix it).
	///
	/// This is functionally identical to <see cref="GameObject.TryGetComponent{T}"/> merged with <see cref="GameObject.GetComponentInChildren{T}()"/>
	/// except it lacks the performance optimization for TryGetComponent that Unity implemented in EDITOR (at runtime
	/// TryGetComponent is supposedly the same implementation as GetComponent). This is acceptable since the method is primarily used
	/// for code-readability and maintenance, not for the (very minor) performance improvements.
	/// </summary>
	public static class WorkaroundUnityMissingTryGetComponent
	{
		public static bool TryGetComponentInChildren<T>( this GameObject go, out T result ) where T : Component
		{
			var firstChild = go.GetComponentInChildren<T>();
			if( firstChild != null )
			{
				result = firstChild;
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}
	}
}