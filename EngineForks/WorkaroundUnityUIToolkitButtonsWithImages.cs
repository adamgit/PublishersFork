using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PublishersFork
{
	/// <summary>
	/// REQUIRES: "WorkaroundUnityTexture2DPixelsPerPoint.cs" -- because Unity made the core API here only available to
	/// other Unity employees.
	///
	/// UIToolkit's Button class handles images badly: it won't let you put an image on a button without breaking the
	/// image's size/aspect-ratio. This class has methods for doing what you'd expect/demand by default: buttons-with-images
	/// use the image as the size for the button!
	/// </summary>
	public static class WorkaroundUnityUIToolkitButtonsWithImages
	{
		public static Button AddButtonUsingImage( this VisualElement parent, Texture2D icon, Action clickAction )
		{
			var b = new Button( clickAction )
			{
				style =
				{
					backgroundImage = icon,
					width = icon.width/icon.PixelsPerPoint(),
					height = icon.height/icon.PixelsPerPoint()
				}
			};
			parent.Add( b );

			return b;
		}
		
		/// <summary>
		/// Optional variant: lets you override the size of the icon, specifying it in device-independent values (i.e. same
		/// as the image's own pixels)
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="icon"></param>
		/// <param name="clickAction"></param>
		/// <param name="overrideSize"></param>
		/// <returns></returns>
		public static Button AddButtonUsingImage( this VisualElement parent, Texture2D icon, Action clickAction, Vector2? overrideSize )
		{
			var b = new Button( clickAction )
			{
				style =
				{
					backgroundImage = icon,
					width = (overrideSize?.x ?? icon.width)/icon.PixelsPerPoint(),
					height = (overrideSize?.y ?? icon.height)/icon.PixelsPerPoint()
				}
			};
			parent.Add( b );

			return b;
		}
	}
}