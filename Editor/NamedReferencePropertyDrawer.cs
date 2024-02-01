using NamedReferences.Attributes;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NamedReferences.Editor
{
    [CustomPropertyDrawer(typeof(NamedReferenceAttribute))]
    public class NamedReferencePropertyDrawer : PropertyDrawer
    {
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