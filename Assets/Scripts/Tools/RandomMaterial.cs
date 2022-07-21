using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public class RandomMaterial : MonoBehaviour
    {
        [SerializeField] List<Material> materials;

        void Start()
        {
            var smr = GetComponent<SkinnedMeshRenderer>();
            var rnd = Random.Range(0, materials.Count);
            smr.material = materials[rnd];
            Destroy(this);
        }
    }
}
