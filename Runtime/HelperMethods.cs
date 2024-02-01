using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NamedReferences.Attributes
{
    public static class HelperMethods
    {
        private static Component FindAnyChildInner(Transform transform, string name, Type componentType, bool throwError, int depth, bool owns)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == name && child.TryGetComponent(componentType, out var component))
                {
                    return component;
                }

                if (owns && child.name.StartsWith('#'))
                {
                    continue;
                }

                var subChild = FindAnyChildInner(child, name, componentType, throwError, depth + 1, owns);
                if (subChild != null)
                {
                    return subChild;
                }
            }

            if (throwError && depth == 0)
            {
                Debug.LogError($"Could not find child with the name: ({name})", transform);
            }

            return default;
        }
        
        private static T FindAnyChildInner<T>(Transform transform, string name, bool throwError, int depth, bool owns)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == name && child.TryGetComponent(out T component))
                {
                    return component;
                }

                if (owns && child.name.StartsWith('#'))
                {
                    continue;
                }

                var subChild = FindAnyChildInner<T>(child, name, throwError, depth + 1, owns);
                if (subChild != null)
                {
                    return subChild;
                }
            }

            if (throwError && depth == 0)
            {
                Debug.LogError($"Could not find child with the name: ({name})", transform);
            }

            return default;
        }
        
        /// <summary>
        /// Same as <see cref="FindAnyChildDirect"/> by typed.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindAnyChildDirect<T>(this Transform transform, string name)
        {
            var component = FindAnyChildInner<T>(transform, name, false, 0, true);

            if (component == null)
            {
                Debug.LogError($"Could not find child with the name and component <{typeof(T).Name}>: ({name})", transform);
            }

            return component;
        }
        
        public static bool TryFindAnyChildDirect<T>(this Transform transform, string name,out T component)
        {
            component = FindAnyChildInner<T>(transform, name, false, 0, true);
            return component != null;
        }
        
        public static bool TryFindAnyChildDirect(this Transform transform, string name, Type componentType,out Component component)
        {
            component = FindAnyChildInner(transform, name, componentType, false, 0, true);
            return component;
        }
        
        /// <summary>
        /// Same as <see cref="FindAnyChild(UnityEngine.Transform,string,bool)"/> but does not log an error if child is not found. </summary>
        public static bool TryFindAnyChild(this Transform transform, string name, Type componentType, out Component component)
        {
            component = FindAnyChildInner(transform, name, componentType, false, 0, false);
            return component;
        }

        /// <summary>
        /// Same as <see cref="FindAnyChild(UnityEngine.Transform,string,bool)"/> but typed.
        /// </summary>
        public static bool TryFindAnyChild<T>(this Transform transform, string name, out T component) where T : Object
        {
            component = FindAnyChildInner<T>(transform, name, false, 0, false);
            return component;
        }

        /// <summary>
        /// Same as <see cref="TryFindAnyChild(UnityEngine.Transform,string,System.Type,out UnityEngine.Component)"/> but typed
        /// </summary>
        public static T TryFindAnyChild<T>(this Transform transform, string name)
        {
            var component = FindAnyChildInner<T>(transform, name, false, 0, false);
            return component;
        }
        
        /// <summary>
        /// Find any child with a given tag.
        /// </summary>
        public static void FindAnyChildrenWithTag(this Transform transform, string tag, List<Transform> objects)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.CompareTag(tag))
                {
                    objects.Add(child);
                }

                FindAnyChildrenWithTag(child, tag, objects);
            }
        }

        /// <summary>
        /// Find all children with a given tag.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static List<Transform> FindAnyChildrenWithTag(this Transform transform, string tag)
        {
            var children = new List<Transform>();
            FindAnyChildrenWithTag(transform, tag, children);
            return children;
        }

        /// <summary>
        /// Same as <see cref="FindAnyChild(UnityEngine.Transform,string,bool)"/> but typed.
        /// </summary>
        public static T FindAnyChild<T>(this Transform transform, string name)
        {
            var component = FindAnyChildInner<T>(transform, name, false, 0, false);

            if (component == null)
            {
                Debug.LogError($"Could not find child with the name and component <{typeof(T).Name}>: ({name})", transform);
            }

            return component;
        }
        
        private static GameObject FindAnyChildInternal(this Transform transform, string name, bool owns = true)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == name)
                {
                    return child.gameObject;
                }

                if (owns && child.name.StartsWith('#'))
                {
                    continue;
                }

                var subChild = FindAnyChildInternal(child, name, owns);
                if (subChild)
                {
                    return subChild;
                }
            }

            return null;
        }

        private static Component FindAnyChildInternal(this Transform transform, Type componentType, string name, bool owns = true)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == name && child.TryGetComponent(componentType, out var component))
                {
                    return component;
                }

                if (owns && child.name.StartsWith('#'))
                {
                    continue;
                }

                var subChild = FindAnyChildInternal(child, componentType, name, owns);
                if (subChild)
                {
                    return subChild;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Find a child with a given name only if directly a child of the transform. If there is another transform starting with the # in the name, that will block it.
        /// </summary>
        /// <example>
        /// #Root
        /// - #Child_1
        /// - #AnotherParent
        /// - - # Child_2
        /// In the above example, #Child_1 is a direct child of #Root but #Child_2 is not.
        /// </example>
        /// <returns>Logs an error if component is not found.</returns>
        public static GameObject FindAnyChildDirect(this Transform transform, string name)
        {
            var child = FindAnyChildInternal(transform, name);
            if (!child)
            {
                Debug.LogError($"Could not find child with the name: ({name})", transform);
            }

            return child;
        }
        
        /// <summary>
        /// Same as <see cref="FindAnyChildDirect"/> but doesn't log an error if child isn't found.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="name"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static GameObject TryFindAnyChildDirect(this Transform transform, string name)
        {
            return FindAnyChildInternal(transform, name);
        }

        /// <summary>
        /// Same as <see cref="FindAnyChild(UnityEngine.Transform,string,bool)"/> but typed.
        /// </summary>
        public static GameObject TryFindAnyChild(this Transform transform, string name)
        {
            var child = FindAnyChildInternal(transform, name, false);
            return child;
        }

        /// <summary>
        /// Finds the first valid child with a given name.
        /// </summary>
        public static GameObject FindAnyChild(this Transform transform, string name, bool throwError = true)
        {
            var child = FindAnyChildInternal(transform, name, false);
            if (!child && throwError)
            {
                Debug.LogError($"Could not find child with the name: ({name})", transform);
            }

            return child;
        }

        /// <summary>
        /// Finds the first valid child with a given name and component of type T on it.
        /// </summary>
        public static Component FindAnyChild(this Transform transform, Type componentType, string name, bool throwError = true)
        {
            var child = FindAnyChildInternal(transform, componentType, name, false);
            if (!child && throwError)
            {
                Debug.LogError($"Could not find child with the name: ({name})", transform);
            }

            return child;
        }
    }
}