
using NamedReferences.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NamedReferences.Editor
{
    [InitializeOnLoad]
    public static class NamedReferenceUpdater
    {
        private static readonly List<GameObject> _gameObjectsTmp = new();

        private static readonly List<Component> _componentsTmp = new();

        private static readonly TypeCache.FieldInfoCollection _fields;

        static NamedReferenceUpdater()
        {
            _fields = TypeCache.GetFieldsWithAttribute<NamedReferenceAttribute>();
            ObjectChangeEvents.changesPublished += ObjectChangeEventsOnchangesPublished;
            ProcessAllObjects();
            EditorSceneManager.sceneOpened += EditorSceneManagerOnsceneOpened;
        }

        private static void EditorSceneManagerOnsceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }
            
            _gameObjectsTmp.Clear();
            scene.GetRootGameObjects(_gameObjectsTmp);
            foreach (var obj in _gameObjectsTmp)
            {
                UpdateGameObject(obj);
            }
        }

        private static void ProcessAllObjects()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }
            
            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                _gameObjectsTmp.Clear();
                scene.GetRootGameObjects(_gameObjectsTmp);
                foreach (var obj in _gameObjectsTmp)
                {
                    UpdateGameObject(obj);
                }
            }
        }

        private static void ObjectChangeEventsOnchangesPublished(ref ObjectChangeEventStream stream)
        {
            for (var i = 0; i < stream.length; i++)
            {
                switch (stream.GetEventType(i))
                {
                    case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                    {
                        stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var changeComponent);
                        var obj = EditorUtility.InstanceIDToObject(changeComponent.instanceId);
                        switch (obj)
                        {
                            case GameObject gameObject:
                            {
                                var parent = gameObject.transform.parent;
                                while (parent)
                                {
                                    UpdateGameObject(parent.gameObject);
                                    parent = parent.parent;
                                }
                                break;
                            }

                            case Transform transform: UpdateGameObject(transform.gameObject);
                                break;
                        }
                    }
                        break;

                    case ObjectChangeKind.ChangeGameObjectParent:
                    {
                        stream.GetChangeGameObjectParentEvent(i, out var changeParent);
                        var obj = EditorUtility.InstanceIDToObject(changeParent.instanceId);
                        if (obj is GameObject gameObject)
                        {
                            var parent = gameObject.transform.parent;
                            UpdateGameObject(gameObject);
                            while (parent)
                            {
                                UpdateGameObject(parent.gameObject);
                                parent = parent.parent;
                            }
                        }
                    }
                        break;
                }
            }
        }

        private static void UpdateGameObject(GameObject parent)
        {
            foreach (var component in parent.GetComponents<Component>())
            {
                foreach (var field in _fields)
                {
                    if (field.DeclaringType != null && field.DeclaringType.IsInstanceOfType(component))
                    {
                        var namedReferenceAttribute = field.GetCustomAttribute<NamedReferenceAttribute>();
                        var existingValue = field.GetValue(component);
                        if (field.FieldType == typeof(GameObject))
                        {
                            var newChild = namedReferenceAttribute.IsDirect ? component.transform.TryFindAnyChildDirect(namedReferenceAttribute.Name) : component.transform.TryFindAnyChild(namedReferenceAttribute.Name);
                            if (newChild == existingValue as GameObject)
                            {
                                continue;
                            }
                            field.SetValue(component, newChild);
                            EditorUtility.SetDirty(component);
                        }
                        else
                        {
                            Component newComponent = null;
                            if (namedReferenceAttribute.IsDirect)
                            {
                                component.transform.TryFindAnyChildDirect(namedReferenceAttribute.Name, field.FieldType, out newComponent);
                            }
                            else
                            {
                                component.transform.TryFindAnyChild(namedReferenceAttribute.Name, field.FieldType, out newComponent);
                            }
                            if (existingValue as Component == newComponent)
                            {
                                continue;
                            }
                            field.SetValue(component, newComponent);
                            EditorUtility.SetDirty(component);
                        }
                    }
                }
            }
        }
    }
}