using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace PublishersFork
{
    /// <summary>
    /// One fix, multiple bugs, in Unity's Foldout.
    ///
    /// 1. Foldout is hard coded to change its own position if it detects its parent is anything EXCEPT another Foldout.
    /// This seemingly exists because they wrote Foldout as a hack, instead of writing it properly - they made it
    /// use negative margins (poorly), but that causes unpleasant visual design if a foldout has no spare space to the left.
    /// This was a reported bug in early releases of Unity 2018 or so. Instead of fixing the original bug, with several
    /// options that would have been CSS standard approaches - or even: just write a better code solution! - they added a bug to hide it.
    ///
    /// 2. Foldout can't be styled, because someone at Unity decided to make the core variable - REQUIRED BY DESIGN to be public
    /// - into a private variable. People have complained about this to Unity publicly for years, and UIToolkit team is
    /// aware but chose to do nothing about it. There is no obvious reason for making this private - it only makes everyone's
    /// code worse. There is no benefit. Everyone is forced to write hacks to 'guess' where the style elements are and
    /// write very fragile code that will fail if Unity makes any minor change in the future. Or ... Unity could just make
    /// the variable public!
    ///
    /// To fix problem 1, use the .Toggle() method here and re-instate the original bug, removing the secondary bug. Then
    /// make sure you always embedd Foldout instances inside something that has at least 15 pixels of left spare space. e.g.:
    ///
    ///    VisualElement parent = ...
    ///    parent.marginLeft = 15f;
    ///    
    ///    Foldout myFoldout = ...
    ///    parent.Add( myFoldout );
    ///    myFoldout.Toggle().style.marginLeft = -15f; // what Unity originally set it to
    ///
    /// To fix problem 2, use .Toggle() method and then style it - however as of 2022 there are ADDITIONAL BUGS in Toggle
    /// class, such that it returns the wrong object for .labelElement, and your styling will be lost; another workaround
    /// class fixes that, providing a Toggle.Label() method that returns the 'real' Label.
    /// </summary>
    public static class WorkaroundUnityUIToolkitFoldoutToggleIsPrivate
    {
        /// <summary>
        /// Without this you can still try to style Foldouts, but it's error prone and painful, and very easy to get wrong.
        /// </summary>
        /// <param name="foldout"></param>
        /// <returns>The internal Toggle that Foldout uses to implement most of its functionality</returns>
        public static Toggle Toggle( this Foldout foldout )
        {
            return typeof(Foldout).GetField( "m_Toggle", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( foldout ) as Toggle;
        }
    }
}