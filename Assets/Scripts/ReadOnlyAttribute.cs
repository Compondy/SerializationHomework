using System;
using UnityEngine;
using UnityEngine.UIElements;

[AttributeUsage(AttributeTargets.Field)]
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyPropertyDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        UnityEditor.EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }

    public override VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property)
    {
        var element = base.CreatePropertyGUI(property) ?? new UnityEditor.UIElements.PropertyField(property);
        element.SetEnabled(false);
        return element;
    }
}
#endif
