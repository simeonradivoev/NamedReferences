using NamedReferences.Attributes;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NamedReferences.Editor
{
    [CustomPropertyDrawer(typeof(NamedReferenceAttribute))]
    public class NamedReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attribute = fieldInfo.GetCustomAttribute<NamedReferenceAttribute>();
            EditorGUI.ObjectField(position, property, EditorGUIUtility.TrTempContent(attribute.IsDirect ? $"{attribute.Name} (Direct)" : attribute.Name));
        }

        #region Overrides of PropertyDrawer

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var attribute = fieldInfo.GetCustomAttribute<NamedReferenceAttribute>();
            var objectField = new ObjectField(attribute.IsDirect ? $"{attribute.Name} (Direct)" : attribute.Name) { tooltip = property.displayName };
            objectField.SetEnabled(false);
            objectField.BindProperty(property);
            return objectField;
        }

        #endregion
    }
}