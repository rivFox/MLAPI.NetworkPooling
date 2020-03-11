using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using System.IO;


namespace MLAPI.NetworkPooling
{
    public class NetworkSpawner : MonoBehaviour
    {
        static readonly Dictionary<ulong, NetworkedObjectPool> instancePoolMap = new Dictionary<ulong, NetworkedObjectPool>();

        [SerializeField]
        bool allNetworkedPrefabsArePoolalbe = false;

        [SerializeField]
        int defaultPrewarmCount = 0;


        void Start() => InstantiatePool();

        //Should be call after NetworkingManager.Singleton.StartClient() / .StartServer() / .StartHost() because handlers are cleared in NetworkingManager.Init()
        void InstantiatePool()
        {
            var prefabList = NetworkingManager.Singleton.NetworkConfig.NetworkedPrefabs;
            for (int i = 0; i < prefabList.Count; i++)
            {
                var prefab = prefabList[i].Prefab;
                var poolableObject = prefab.GetComponent<PoolableObject>();
                if (!poolableObject && !allNetworkedPrefabsArePoolalbe)
                    continue;

                var networkedObject = prefab.GetComponent<NetworkedObject>();
                if (!networkedObject)
                {
                    Debug.LogError($"{prefab.name} should have NetwrokObject component.");
                    continue;
                }

                var prefabHash = networkedObject.PrefabHash;

                instancePoolMap.Add(prefabHash, new NetworkedObjectPool(networkedObject, poolableObject?.PrewarmCount ?? defaultPrewarmCount));
                SpawnManager.RegisterSpawnHandler(prefabHash, (Vector3 position, Quaternion rotation) => instancePoolMap[prefabHash].GetInstance(position, rotation));
                SpawnManager.RegisterCustomDestroyHandler(prefabHash, (NetworkedObject instance) => instancePoolMap[prefabHash].ReclaimInstance(instance));
            }
        }

        public static NetworkedObject GetInstance(NetworkedObject prefab)
        {
            return instancePoolMap.GetValueOrDefault(prefab.PrefabHash)?.GetInstance() ?? null;
        }

        /// <summary>
        /// Instantiate (from pool if possible) and spawn prefab object across the network. Can only be called from the Server
        /// </summary>
        public static NetworkedObject Spawn(NetworkedObject prefab, Vector3 position = default, Quaternion rotation = default, Stream spawnPayloads = null, bool destroyWithScene = false) 
        {
            //TODO: I don't know if `destroyWithScene = true` works correctly! Need test!
            var instance = instancePoolMap.GetValueOrDefault(prefab.PrefabHash)?.GetInstance(position, rotation) ?? null;
            if (!instance)
                instance = Instantiate(prefab, position, rotation);

            var transform = instance.transform;
            transform.position = position;
            transform.rotation = rotation;

            instance.Spawn(spawnPayloads, destroyWithScene);
            return instance;
        }

        /// <summary>
        /// If not poolable: Unspawns this GameObject and destroys it for other clients. This should be used if the object should be kept on the server
        /// If poolable: Unspawns this GameObject and reclaim to pool on server and clients. 
        /// Can only be called from the Server
        /// </summary>
        /// <returns>True if object is poolable, false if object is not poolable</returns>
        public static bool UnSpawn(NetworkedObject instance)
        {
            instance.UnSpawn();
            if (instancePoolMap.TryGetValue(instance.PrefabHash, out var pool))
            {
                pool.ReclaimInstance(instance);
                return true;
            }
            return false;
        }

        /// <summary>
        /// If not poolable: Unspawns this GameObject and destroys it for other clients and server. 
        /// If poolable: Unspawns this GameObject and reclaim to pool on server and clients.
        /// Can only be called from the Server
        /// </summary>
        public static void Destroy(NetworkedObject instance)
        {
            if (!UnSpawn(instance))
                Destroy(instance.gameObject);
        }
    }
}