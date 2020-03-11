using UnityEngine;

namespace MLAPI.NetworkPooling
{
    public class PoolableObject : MonoBehaviour
    {
        public int PrewarmCount => prewarmCount;
        [SerializeField] int prewarmCount;

    }
}