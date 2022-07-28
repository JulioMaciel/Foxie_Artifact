using System.Collections;
using StaticData;
using Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace Controller
{
    [RequireComponent(typeof(AudioSource))]
    
    public class FootstepControl : MonoBehaviour
    {
        [SerializeField] GameObject footstepObject;
        [SerializeField] AudioClip[] grassClips;
        [SerializeField] AudioClip[] sandClips;
        
        AudioSource audioSource;
        ObjectPool<GameObject> poolDust;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            poolDust = new ObjectPool<GameObject>(() =>
                Instantiate(footstepObject),
                dust => dust.SetActive(true),
                dust => dust.SetActive(false),
                Destroy, false
            );
        }

        // Called by animation clip
        void Step()
        {
            if (IsOverTerrain(Tags.SandTerrain))
            {
                audioSource.PlayRandomClip(sandClips);
                var dust = poolDust.Get();
                dust.transform.position = gameObject.transform.localPosition;
                StartCoroutine(ReleaseDustPoolAfterFinishing(dust));
            }
            else if (IsOverTerrain(Tags.GrassTerrain)) 
                audioSource.PlayRandomClip(grassClips);
        }

        IEnumerator ReleaseDustPoolAfterFinishing(GameObject dust)
        {
            var ps = dust.GetComponent<ParticleSystem>();
            while (ps.isPlaying) yield return null;
            poolDust.Release(dust);
        }

        bool IsOverTerrain(string tag)
        {
            var hasHit = Physics.Raycast(transform.position, Vector3.down, out var hitInfo, 0.1f, Layers.Terrain);
            return hasHit && hitInfo.transform.CompareTag(tag);
        }
    }
}