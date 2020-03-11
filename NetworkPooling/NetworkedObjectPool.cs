using MLAPI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MLAPI.NetworkPooling
{
    public sealed class NetworkedObjectPool
    {
        private Stack<NetworkedObject> pool = new Stack<NetworkedObject>();
        private NetworkedObject networkedObject;

        public NetworkedObjectPool(NetworkedObject networkedObject, int prewarmCount)
        {
            this.networkedObject = networkedObject;
            for (int i = 0; i < prewarmCount; i++)
            {
                InstantiateNewToPool();
            }
        }

        public void InstantiateNewToPool()
        {
            var obj = UnityEngine.Object.Instantiate(networkedObject);
            UnityEngine.Object.DontDestroyOnLoad(obj.gameObject);
            obj.gameObject.SetActive(false);
            pool.Push(obj);
#if UNITY_EDITOR
            if (hideInHierarchy)
                obj.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
        }

        public NetworkedObject GetInstance(bool setActive = true)
        {
            if (pool.Count == 0)
                InstantiateNewToPool();

            var instance = pool.Pop();
            if (setActive)
                instance.gameObject.SetActive(true);

            return instance;
        }

        public NetworkedObject GetInstance(Vector3 position, Quaternion rotation)
        {
            var instance = GetInstance(false);
            var transform = instance.transform;
            transform.position = position;
            transform.rotation = rotation;
            instance.gameObject.SetActive(true);
#if UNITY_EDITOR
            instance.gameObject.hideFlags = HideFlags.None;
            UnityEditor.EditorApplication.DirtyHierarchyWindowSorting();
#endif
            return instance;
        }

        public void ReclaimInstance(NetworkedObject instance)
        {
#if UNITY_EDITOR
            if (hideInHierarchy)
            {
                instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
                UnityEditor.EditorApplication.DirtyHierarchyWindowSorting();
            }
#endif
            instance.gameObject.SetActive(false);
            pool.Push(instance);
        }

#if UNITY_EDITOR
        bool hideInHierarchy = false;
        public void SetHideInHierarchy(bool value)
        {
            if (hideInHierarchy == value)
                return;

            hideInHierarchy = value;
            foreach (var item in pool.ToList())
            {
                if (!item)
                    continue;

                item.gameObject.hideFlags = hideInHierarchy ? HideFlags.HideInHierarchy : HideFlags.None;
            }
        }
#endif
    }
}


