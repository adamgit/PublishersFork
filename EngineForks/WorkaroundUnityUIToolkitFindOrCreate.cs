using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PublishersFork
{
	/// <summary>
	/// This is one of the most heavily used fixes/improvements to UIToolkit.
	/// 
	/// UIToolkit - by design - needs you to NEVER 'Add' a component if it already has been added, but ...
	/// Unity's provided methods don't directly allow that. So instead of calling Add( VisualElement ),
	/// we have a method ReuseOrAddNew( VisualElement, ... ).
	///
	/// This method lazily creates a new VisualElement instance if needed AND OTHERWISE it discovers and
	/// returns the EXISTING instance that matched the required parameters.
	/// </summary>
	public static class WorkaroundUnityUIToolkitFindOrCreate
	{
		/// <summary>
		/// Uses the provided function as a constructor to create the new VisualElement, so that the T parameter doesn't need
		/// to be specified (it should be inferred from the constructor). Automatically unhides the element if it existed
		/// already and was invisible.
		///
		/// Use it like this:
		///
		///    VisualElement myRoot = ...
		///    Label newLabel = myRoot.ReuseOrAppendNew( "kLabel1", () => new Label() { text = "Hello World" } );
		/// </summary>
		/// <param name="localRoot"></param>
		/// <param name="kName"></param>
		/// <param name="initialSetup"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T ReuseOrAppendNew<T>( this VisualElement localRoot, string kName, Func<T> initialSetup, bool debug = false ) where T : VisualElement
		{
			var block = localRoot.Q<T>( kName );
			if( block == null )
			{
				if( debug ) Debug.Log( localRoot.name+" has no child named: "+kName );

				block = initialSetup();
				block.name = kName;
				localRoot.Add( block );
			}
			else if( block.style.display == DisplayStyle.None )
				block.style.display = DisplayStyle.Flex;

			return block;
		}

		/// <summary>
		/// Uses the default constructor for the VisualElement subclass, recommend you use the alternate <see cref="ReuseOrAppendNew{T}(UnityEngine.UIElements.VisualElement,string,System.Func{T})"/>
		/// </summary>
		/// <param name="root"></param>
		/// <param name="childName"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T ReuseOrAppendNew<T>( this VisualElement root, string childName ) where T : VisualElement, new()
		{
			return ReuseOrAppendNew<T>( root, childName, () => new T() {name = childName} );
		}
		
		/// <summary>
		/// As for <see cref="ReuseOrAppendNew{T}(UnityEngine.UIElements.VisualElement,string,Func{T})"/> except that
		/// it allows you to run code once - AND ONCE ONLY - after the object Constructor happens (this is necessary
		/// for a lot of UIToolkit's VisualElement classes that - due to UIToolkit's design - are incapable of being
		/// created in a single line of code).
		/// </summary>
		/// <param name="localRoot"></param>
		/// <param name="kName"></param>
		/// <param name="initialSetup"></param>
		/// <param name="postConstructionInitializer"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T ReuseOrAppendNew<T>( this VisualElement localRoot, string kName, Func<T> initialSetup, Action<T> postConstructionInitializer ) where T : VisualElement
		{
			return ReuseOrAppendNew<T>( localRoot, kName, initialSetup, postConstructionInitializer, out var discard );
		}

		public static T ReuseOrAppendNew<T>( this VisualElement localRoot, string kName, Func<T> initialSetup, Action<T> postConstructionInitializer, out bool didCreate ) where T : VisualElement
		{
			var block = localRoot.Q<T>( kName );
			if( block == null )
			{
				block = initialSetup();
				block.name = kName;
				postConstructionInitializer( block );
				localRoot.Add( block );
				didCreate = true;
			}
			else
			{
				if( block.style.display == DisplayStyle.None )
					block.style.display = DisplayStyle.Flex;
				
				didCreate = false;
			}

			return block;
		}
	}
}