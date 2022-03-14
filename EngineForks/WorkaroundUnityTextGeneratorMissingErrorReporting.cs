using System.Reflection;
using UnityEngine;

namespace PublishersFork
{
	/// <summary>
	/// Unity's TextGenerator class - the only way to find the size of a piece of text, so that you can adjust
	/// sizes of the rest of your UI, images, etc - silently deletes any error messages, and does not allow you
	/// to see them, while incorrectly returning "0" for the width or height of strings. This method finds the
	/// error, if you call it immediately after a TextGenerator public API method has failed/returned 0. 
	/// </summary>
	public static class WorkaroundUnityTextGeneratorMissingErrorReporting
	{
		public static string FetchLastErrorString(this TextGenerator tg)
		{
			var infoProp = typeof(TextGenerator).GetField("m_LastValid", BindingFlags.Instance | BindingFlags.NonPublic);
			var tge = infoProp.GetValue(tg);
			return tge.ToString();
		}
	}
}