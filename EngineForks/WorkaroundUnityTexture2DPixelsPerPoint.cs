using System;
using UnityEngine;

namespace PublishersFork
{
	/// <summary>
	/// There is a core method in Texture2D without which it's impossible to display textures correctly
	/// on retina screens (which as of 2022 is most screens) - but Unity made it private to Unity
	/// and added a specific hack to make it available to the UIToolkit team. There is a public equivalent that is
	/// Editor-only, but if you're using UIToolkit in player/runtime/games then you need this without the Editor APIs.
	///
	/// FYI Unity's internal hack is: "[VisibleToOtherModules(new string[] {"UnityEngine.UIElementsModule"})]" 
	/// </summary>
	public static class WorkaroundUnityTexture2DPixelsPerPoint
	{
		/// <summary>
		/// Note: this would be a property, except C# doesn't support extension properties yet.
		/// </summary>
		/// <param name="texture2D"></param>
		/// <returns></returns>
		public static float PixelsPerPoint(this Texture2D texture2D)
		{
			var property_pixelsPerPoint = typeof(Texture2D).GetProperty("pixelsPerPoint");
			var value = property_pixelsPerPoint.GetValue(texture2D);
			if (value is float)
				return (float) value;
			else
				throw new Exception("Unity changed their internal API; was expecting a float from .pixelsPerPoint, received something else: "+value);
		}
	}
}