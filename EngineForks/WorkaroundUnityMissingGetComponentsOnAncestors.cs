using UnityEngine;

namespace PublishersFork
{
	/// <summary>
	/// Unity provides GetComponent(s)InChildren, and GetComponent(s)InParent, but the latter
	/// only returns items on the direct parent, and doesn't give you fine control over which
	/// parents are accessed.
	///
	/// The methods here are commonly used for finding the outermost / containing Component
	/// (e.g. in UnityUI where you need to find the outermost UI component and check that it's
	/// on a Canvas)
	/// </summary>
	public static class WorkaroundUnityMissingGetComponentsOnAncestors
	{
		/// <summary>
		/// Finds the first instance of a specific MonoBehaviour, by going up the tree of Parents
		/// </summary>
		/// <param name="t"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetFirstAncestorComponent<T>(this Transform t) where T : MonoBehaviour
		{
			T parentT = null;
			Transform parentTransform = t.parent;
			while (parentTransform != null && parentT == null)
			{
				parentT = parentTransform.GetComponent<T>();
				parentTransform = parentTransform.parent;
			}

			return parentT;
		}

		/// <summary>
		/// Finds the root-most / highest-in-tree instance of a specific MonoBehaviour
		/// </summary>
		/// <param name="t"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetHighestAncestorComponent<T>(this Transform t) where T : MonoBehaviour
		{
			T parentT = null;
			Transform parentTransform = t.parent;
			while (parentTransform != null)
			{
				if (parentTransform.TryGetComponent<T>(out parentT))
#pragma warning disable 642
					;
#pragma warning restore 642
				// keep looking in case there's an outer one left to find...
				parentTransform = parentTransform.parent;
			}

			return parentT;
		}
	}
}