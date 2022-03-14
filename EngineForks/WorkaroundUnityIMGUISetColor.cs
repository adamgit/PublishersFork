using System;
using UnityEngine;

namespace PublishersFork
{
	/// <summary>
	/// With IMGUI you often need to re-color an item, but IMGUI has no direct way of doing that. The standard approach
	/// is to save the GUI.color value, change it, draw your component, then reset the GUI.color - but this is error
	/// prone and a lot of boilerplate code to achieve something that should be 0 lines of code.
	///
	/// This class is setup to allow you to write:
	///
	/// public void MyGUIMethod()
	/// {
	///    using( new WorkaroundUnityIMGUISetColor( Color.red ) )
	///    {
	///       GUI.Label( "Red-colored label" );
	///    }
	/// }
	///
	/// 'using' allows us to auto-reset the GUI.color.
	///
	/// TODO: convert it to a struct (to prevent GC.alloc) and test it still works correctly
	/// </summary>
	public class WorkaroundUnityIMGUISetColor : IDisposable
	{
		private bool m_Disposed;

		internal virtual void Dispose(bool disposing)
		{
			if (this.m_Disposed)
				return;
			if (disposing)
				this.CloseScope();
			this.m_Disposed = true;
		}

		~WorkaroundUnityIMGUISetColor()
		{
			if (!this.m_Disposed)
				Debug.LogError(this.GetType().Name + " was not disposed! You should use the 'using' keyword or manually call Dispose.");
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize((object) this);
		}

		private Color _originalColor;

		public WorkaroundUnityIMGUISetColor(Color newColor)
		{
			_originalColor = GUI.color;
			GUI.color = newColor;
		}

		protected void CloseScope()
		{
			GUI.color = _originalColor;
		}
	}
}