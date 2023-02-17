using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// DrawDefaultInspector was a core feature of the Editor APIs for the 15+ years. The UIToolkit team decided to ignore
/// it and break it and provide no alternative in their new system until 3 years after releasing the 'production ready'
/// version. Unity 2022 and later have this method but it was never backported despite having zero dependencies and
/// despite being a required part of Unity's own APIs.
/// </summary>
public class WorkaroundUIToolkitMissingDefaultInspector
{
	/// <summary>
	/// This implementation provided by UIToolkit team via the forums,
	/// see the pre-release version here: https://forum.unity.com/threads/property-drawers.595369/#post-5426619
	/// </summary>
	/// <param name="container"></param>
	/// <param name="serializedObject"></param>
	/// <param name="hideScript"></param>
	public static void FillDefaultInspector( VisualElement container, SerializedObject serializedObject, bool hideScript)
	{
		SerializedProperty property = serializedObject.GetIterator();
		if (property.NextVisible(true)) // Expand first child.
		{
			do
			{
				if (property.propertyPath == "m_Script" && hideScript)
				{
					continue;
				}
				var field = new PropertyField(property);
				field.name = "PropertyField:" + property.propertyPath;
 
 
				if (property.propertyPath == "m_Script" && serializedObject.targetObject != null)
				{
					field.SetEnabled(false);
				}
 
				container.Add(field);
			}
			while (property.NextVisible(false));
		}
	}
}