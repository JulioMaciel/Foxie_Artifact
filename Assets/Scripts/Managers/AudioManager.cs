using System.Collections;
using Tools;
using UnityEngine;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] AudioClip menuClip;
        [SerializeField] AudioClip introClip;
        [SerializeField] AudioClip gameClip;

        AudioSource backgroundAudio;
        float initVolume;
        
        public static AudioManager Instance;
        void Awake() 
        {
            Instance = this;
            backgroundAudio = GetComponent<AudioSource>();
        }

        void Start()
        {
            initVolume = backgroundAudio.volume;
        }
        
        public void PlayMenuClip()
        {
            backgroundAudio.PlayClip(menuClip);
            StartCoroutine(ResetVolume());
        }
        
        public IEnumerator PlayIntroClip()
        {
            yield return WaitCurrentClipStop();
            backgroundAudio.PlayClip(introClip);
            StartCoroutine(ResetVolume());
        }

        public IEnumerator PlayGameClip()
        {
            yield return WaitCurrentClipStop();
            backgroundAudio.PlayClip(gameClip);
            backgroundAudio.time = 5;
            StartCoroutine(ResetVolume());
        }

        IEnumerator WaitCurrentClipStop()
        {
            while (backgroundAudio.isPlaying)
                yield return null;
        }

        IEnumerator ResetVolume()
        {
            while (backgroundAudio.volume < initVolume)
            {
                backgroundAudio.volume += Time.deltaTime;
                yield return null;
            }
        }

        public IEnumerator StopSmoothly()
        {
            while (backgroundAudio.volume > 0)
            {
                backgroundAudio.volume -= Time.deltaTime * 0.75f;
                yield return null;
            }
            backgroundAudio.Stop();
        }
    }
}