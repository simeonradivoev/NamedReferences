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

        private static bool _showReferences;

        private static bool _showReferents;
        
        private const string ReferentToggleMenu = "Tools/Named References/Show Referents (Green)";
        
        private const string ReferenceToggleMenu = "Tools/Named References/Show References (Yellow)";
        
        private const string ShowReferencesEditorPref = "NamedReferences.ShowReferences";
        
        private const string ShowReferentsEditorPref = "NamedReferences.ShowReferents";

        static ReferencesInspector()
        {
            Selection.selectionChanged += SelectionChanged;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            SelectionChanged();
            
            _showReferences = EditorPrefs.GetBool(ShowReferencesEditorPref ,true);
            _showReferents = EditorPrefs.GetBool(ShowReferentsEditorPref, true);
            Menu.SetChecked(ReferenceToggleMenu,_showReferences);
            Menu.SetChecked(ReferentToggleMenu,_showReferents);
        }

        [MenuItem(ReferenceToggleMenu)]
        
        private static void ToggleShowReferences()
        {
            _showReferences = !_showReferences;
            EditorPrefs.SetBool(ShowReferentsEditorPref,_showReferences);
            Menu.SetChecked(ReferenceToggleMenu,_showReferences);
            SelectionChanged();
            
        }
        
        [MenuItem(ReferentToggleMenu)]
        private static void ToggleShowReferents()
        {
            _showReferents = !_showReferents;
            EditorPrefs.SetBool(ShowReferencesEditorPref,_showReferents);
            Menu.SetChecked(ReferentToggleMenu,_showReferents);
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
            if (_selectedTargetIds.Count <= 0 || _selectedTargetIds.Contains(instanceid) || (!_showReferences && !_showReferents))
            {
                return;
            }

            if (_cachedGameObjectReferenceIds.Contains(instanceid))
            {
                if (!_showReferences)
                {
                    return;
                }
                
                var texRect = new Rect(selectionRect.xMax - selectionRect.height, selectionRect.yMin, selectionRect.height, selectionRect.height);
                GUI.Label(texRect, EditorGUIUtility.IconContent("PreviewPackageInUse","Referenced by the selected object"));
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
                if (!_showReferents)
                {
                    return;
                }
                var texRect = new Rect(selectionRect.xMax - selectionRect.height, selectionRect.yMin, selectionRect.height, selectionRect.height);
                GUI.Label(texRect, EditorGUIUtility.IconContent("FixedJoint Icon","References the selected object"));
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

            if (!_showReferences && !_showReferents)
            {
                return;
            }

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