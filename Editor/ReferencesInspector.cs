using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamedReferences.Editor
{
    /// <summary>
    /// Highlights references to and from the selected GameObject in the scene
    /// </summary>
    [InitializeOnLoad]
    public static class ReferencesInspector
    {
        private static readonly List<Object> _cachedReferences = new();

        private static readonly HashSet<int> _selectedTargetIds = new();

        private static readonly HashSet<int> _cachedGameObjectReferantIds = new();

        private static readonly HashSet<int> _cachedGameObjectReferenceIds = new();

        private static readonly HashSet<int> _passedGameObjectReferants = new();

        private static readonly List<Component> _componentsTmp = new();

        static ReferencesInspector()
        {
            Selection.selectionChanged += SelectionChanged;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            SelectionChanged();
        }

        private static void OnHierarchyChanged()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }
            SelectionChanged();
        }

        private static void OnHierarchyGUI(int instanceid, Rect selectionRect)
        {
            if (_selectedTargetIds.Count <= 0 || _selectedTargetIds.Contains(instanceid))
            {
                return;
            }

            if (_cachedGameObjectReferenceIds.Contains(instanceid))
            {
                var texRect = new Rect(selectionRect.xMax - selectionRect.height, selectionRect.yMin, selectionRect.height, selectionRect.height);
                GUI.Label(texRect, EditorGUIUtility.IconContent("PreviewPackageInUse"));
                EditorGUI.DrawRect(selectionRect, new Color(1, 1, 0, 0.1f));
            }
            else if (_passedGameObjectReferants.Add(instanceid))
            {
                if (EditorUtility.InstanceIDToObject(instanceid) is GameObject gameObject)
                {
                    _componentsTmp.Clear();
                    gameObject.GetComponents(_componentsTmp);
                    foreach (var component in _componentsTmp)
                    {
                        if (!component)
                        {
                            continue;
                        }

                        using var serializedObject = new SerializedObject(component);
                        using var it = serializedObject.GetIterator();
                        while (it.NextVisible(true))
                        {
                            if (it.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                if (_selectedTargetIds.Contains(it.objectReferenceInstanceIDValue))
                                {
                                    _cachedReferences.Add(it.serializedObject.targetObject);
                                    _cachedGameObjectReferantIds.Add(component.gameObject.GetInstanceID());
                                }
                            }
                        }
                    }
                }
            }
            else if (_cachedGameObjectReferantIds.Contains(instanceid))
            {
                var texRect = new Rect(selectionRect.xMax - selectionRect.height, selectionRect.yMin, selectionRect.height, selectionRect.height);
                GUI.Label(texRect, EditorGUIUtility.IconContent("FixedJoint Icon"));
                EditorGUI.DrawRect(selectionRect, new Color(0, 1, 0, 0.1f));
            }
        }

        private static void SelectionChanged()
        {
            _cachedReferences.Clear();
            _cachedGameObjectReferantIds.Clear();
            _cachedGameObjectReferenceIds.Clear();
            _passedGameObjectReferants.Clear();
            _selectedTargetIds.Clear();

            if (Selection.activeGameObject)
            {
                var targets = new HashSet<Object> { Selection.activeGameObject };
                _selectedTargetIds.Add(Selection.activeInstanceID);
                _componentsTmp.Clear();
                Selection.activeGameObject.GetComponents(_componentsTmp);
                foreach (var component in _componentsTmp)
                {
                    if (!component)
                    {
                        continue;
                    }

                    targets.Add(component);
                    _selectedTargetIds.Add(component.GetInstanceID());

                    using var serializedObject = new SerializedObject(component);
                    using var it = serializedObject.GetIterator();
                    while (it.NextVisible(true))
                    {
                        if (it.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (it.objectReferenceValue is GameObject gameObject)
                            {
                                _cachedGameObjectReferenceIds.Add(gameObject.GetInstanceID());
                            }
                            else if (it.objectReferenceValue is Component refComponent)
                            {
                                _cachedGameObjectReferenceIds.Add(refComponent.gameObject.GetInstanceID());
                            }
                        }
                    }
                }
            }
        }
    }
}