using UnityEngine;

namespace MLAPI.NetworkPooling
{
    public class PoolableObject : MonoBehaviour
    {
        /// <summary>
        /// Count of objects created at startup
        /// </summary>
        public int PrewarmCount => prewarmCount;
        [SerializeField] int prewarmCount;

    }
}