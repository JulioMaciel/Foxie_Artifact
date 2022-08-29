using System.Collections;
using GameEvents;
using Managers;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] AudioSource environmentalAudio;
        [SerializeField] AudioClip menuMusic;
        [SerializeField] AudioClip sadMusic;
        [SerializeField] WakeUpEvent startGameEvent;
        [SerializeField] Camera menuCamera;
        [SerializeField] GameObject targetCameraFocus;
        [SerializeField] IntroHandler intro;
        [SerializeField] Button startGameBtn;
        [SerializeField] Toggle skipIntroCheckBox;
        [SerializeField] Button gitBtn;
        [SerializeField] Button linkedInBtn;
        [SerializeField] AudioClip startBtnClickedSound;
        [SerializeField] AudioClip skipCheckClickedSound;
        [SerializeField] RectTransform gameTitleRect;
        [SerializeField] RectTransform[] titleStars;
        [SerializeField] TextMeshProUGUI titleText;
        
        AudioClip natureEffects;
        AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            natureEffects = environmentalAudio.clip;
            environmentalAudio.PlayClip(menuMusic);
            environmentalAudio.time = 5;

            StartCoroutine(AnimateTitle());
            StartCoroutine(MoveCamera());
            SetUpButtons();
        }

        IEnumerator AnimateTitle()
        {
            KeepRotatingTitleStarts();
            while (gameTitleRect.rect.width < 800)
            {
                var current = gameTitleRect.sizeDelta;  
                gameTitleRect.sizeDelta = new Vector2(current.x + Time.deltaTime * 150, current.y);
                yield return null;
            }
            while (titleText.alpha < 1)
            {
                titleText.alpha += Time.deltaTime;
                yield return null;
            }
        }

        void KeepRotatingTitleStarts()
        {
            foreach (var star in titleStars)
            {
                StartCoroutine(RotateForever(star));
            }
        }

        IEnumerator RotateForever(Transform rect)
        {
            while (enabled)
            {
                rect.Rotate(Vector3.forward, rect.rotation.z + Time.deltaTime * 10);
                yield return null;
            }
        }

        IEnumerator MoveCamera()
        {
            var initPos = menuCamera.transform.localPosition;
            var goRight = true;
            while (enabled)
            {
                var pos = menuCamera.transform.localPosition;
                if (Vector3.Distance(initPos, pos) >= 50) goRight = false;
                var bitMoved = new Vector3(pos.x - Time.deltaTime * (goRight? 1:-1), pos.y, pos.z);
                menuCamera.transform.localPosition = bitMoved;
                menuCamera.transform.LookAt(targetCameraFocus.transform);
                yield return null;
            }
        }

        void SetUpButtons()
        {
            startGameBtn.onClick.AddListener(() =>
            {
                audioSource.PlayClip(startBtnClickedSound);
                StartCoroutine(skipIntroCheckBox.isOn ? StartGame() : StartIntro());
            });
            skipIntroCheckBox.onValueChanged.AddListener(_ =>
            {
                audioSource.PlayClip(skipCheckClickedSound);
            });
            gitBtn.onClick.AddListener(() =>
            {
                audioSource.PlayClip(startBtnClickedSound);
                Application.OpenURL("https://github.com/JulioMaciel/Foxie_Artifact");
            });
            linkedInBtn.onClick.AddListener(() =>
            {
                audioSource.PlayClip(startBtnClickedSound);
                Application.OpenURL("https://www.linkedin.com/in/julio-maciel-1a0971105/");
            });

        }

        IEnumerator StartIntro()
        {
            StartCoroutine(FadeOutMainMenu());
            yield return new WaitForSeconds(1);
            yield return BlackScreenHandler.Instance.Darken();
            StartCoroutine(environmentalAudio.ChangeClipSmoothly(sadMusic, 1));
            yield return new WaitForSeconds(3);
            intro.gameObject.SetActive(true);
            yield return BlackScreenHandler.Instance.Lighten(.4f);
            enabled = false;
        }

        IEnumerator StartGame()
        {
            StartCoroutine(MoveCameraTowardsPlayer());
            StartCoroutine(FadeOutMainMenu());
            yield return BlackScreenHandler.Instance.Darken();
            yield return new WaitForSeconds(1);
            StartCoroutine(environmentalAudio.ChangeClipSmoothly(natureEffects, 1));
            
            StartCoroutine(BlackScreenHandler.Instance.Lighten(1f));
            startGameEvent.enabled = true;
            menuCamera.gameObject.SetActive(false);
            enabled = false;
        }

        IEnumerator FadeOutMainMenu()
        {
            var cg = GetComponent<CanvasGroup>();
            while (cg.alpha >= 0)
            {
                cg.alpha -= Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator MoveCameraTowardsPlayer()
        {
            var player = Entity.Instance.player;
            while (Vector3.Distance(menuCamera.transform.position, player.transform.position) > .5) 
            {
                menuCamera.transform.position = Vector3.MoveTowards(menuCamera.transform.position, 
                    player.transform.position, .1f);
                yield return null;
            }
        }

        void OnDisable()
        {
            startGameBtn.onClick.RemoveAllListeners();
            skipIntroCheckBox.onValueChanged.RemoveAllListeners();
            gitBtn.onClick.RemoveAllListeners();
            linkedInBtn.onClick.RemoveAllListeners();
        }
    }
}
