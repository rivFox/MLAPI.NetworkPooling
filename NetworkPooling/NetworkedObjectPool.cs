using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            return instance;
        }

        internal void ReclaimInstance(NetworkedObject instance)
        {
            instance.gameObject.SetActive(false);
            pool.Push(instance);
        }
    }
}


