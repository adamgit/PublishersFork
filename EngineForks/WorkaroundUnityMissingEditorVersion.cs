using System;
using System.Linq;
using UnityEngine;

namespace PublishersFork
{
	/// <summary>
	/// Unity failed to provide a method "what's the version of the currently-executing UnityEditor" that uses C#'s
	/// Version class, or an equivalent - Unity only gives a string, which is useless in most cases. You cannot use
	/// comparison methods, greater/less-than etc.
	/// </summary>
	public class WorkaroundUnityMissingEditorVersion
	{
		/// <summary>
		/// Returns a valid C# Version object, stripping the letter out of Unity's proprietary versioning system
		/// i.e. 2021.2.3f would return: (2021, 2, 3).
		///
		/// This allows you to compare Versions directly, e.g. "if( unityVersion > new Version(2020,1,2) )"
		/// </summary>
		public static Version unityVersion
		{
			get
			{
				var v = Application.unityVersion;
				var elements = v.Split('.');
				var unityBuildNumber = int.Parse(new string(elements[2].Where(c => char.IsDigit(c)).ToArray()));
				
				return new Version(int.Parse(elements[0]), int.Parse(elements[1]), unityBuildNumber);
			}
		}
	}
}