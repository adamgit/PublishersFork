using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// When using a Foldout, it uses Toggles for rendering.
///
/// But those Toggles return the wrong object for 'labelElement', seems that Unity forgot to read the class they were
/// extending? (BaseField provided labelElement - but Toggle returns the wrong object, and maintains a private
/// object instead)
/// </summary>
public static class WorkaroundUnityUIToolkitToggleBrokenLabelBug
{
    /// <summary>
    /// This returns the 'real' Label, instead of the 'fake, won't work' Label returned by the public API.
    /// </summary>
    /// <param name="toggle"></param>
    /// <returns>Label that contains the text of the Toggle, and is styleable</returns>
    public static Label Label(this Toggle toggle)
    {
        return typeof(Toggle).GetField("m_Label", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue(toggle) as Label;
    }
}