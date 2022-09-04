using System.Collections;
using Managers;
using ScriptableObjects;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class IntroHandler : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI textUI;
        [SerializeField] Image skipBar;
        [SerializeField] Image bg;
        [SerializeField] IntroTextItem initialIntroText;
        [SerializeField] AudioClip letterTypedClip;

        IntroTextItem last;
        IntroTextItem current;
        Vector2 initialBgAnchorPos;
        Vector2 targetBgAnchorPos;
        AudioSource introSource;

        bool hasPlayerSkipped;

        void Awake()
        {
            introSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            StartCoroutine(AudioManager.Instance.PlayIntroClip());
            StartCoroutine(BlackScreenHandler.Instance.Lighten(1f));
            FixBgImageFrameIfWebGL();
            initialBgAnchorPos = bg.rectTransform.anchoredPosition;
            targetBgAnchorPos = new Vector2(initialBgAnchorPos.x - 300, initialBgAnchorPos.y);
            current = initialIntroText;
            ShowItem();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Escape)) IncreaseSkipBar();
            else if (skipBar.fillAmount > 0) ReduceSkipBar();

            if (Input.GetMouseButtonDown(0)) JumpToNextItem();
        }

        void FixBgImageFrameIfWebGL()
        {
            #if UNITY_WEBGL
                var s = bg.rectTransform.localScale;
                bg.rectTransform.localScale = new Vector3(s.x * 1.33f, s.y, s.z);
            #endif
        }

        void JumpToNextItem()
        {
            if (current.next != null)
            {
                current = current.next;
                ShowItem();
                last = current;
            }
            else StartCoroutine(StartGame());
        }

        void ShowItem()
        {
            var isFirstItem = last == null;
            if (isFirstItem)
            {
                bg.sprite = current.backgroundItem.image;
                StartCoroutine(MoveBg());
            }
            else if (current.backgroundItem.image != last.backgroundItem.image)
                StartCoroutine(ChangeBg());
            
            StartCoroutine(ShowTextLetterByLetter());
        }

        IEnumerator ChangeBg()
        {
            yield return DarkenBg();
            bg.rectTransform.anchoredPosition = initialBgAnchorPos;
            bg.sprite = current.backgroundItem.image;
            StartCoroutine(LightenBg());
            StartCoroutine(MoveBg());
        }
        
        IEnumerator MoveBg()
        {
            while (Vector2.Distance(bg.rectTransform.anchoredPosition, targetBgAnchorPos) > 1)
            {
                var frameDistance = Time.deltaTime * 10;
                var nextPos = new Vector2(bg.rectTransform.anchoredPosition.x - frameDistance, initialBgAnchorPos.y);
                bg.rectTransform.anchoredPosition = nextPos;
                yield return null;
            }
        }
        
        IEnumerator ShowTextLetterByLetter()
        {
            textUI.text = string.Empty;
            var currentWhenStarted = current;
            foreach (var c in current.text)
            {
                if (currentWhenStarted != current)
                    yield break;
                
                textUI.text += c;
                introSource.PlayOneShot(letterTypedClip);
                yield return new WaitForSeconds(0.05f);
            }
        }

        IEnumerator DarkenBg()
        {
            while (bg.color.a > 1)
            {
                var c = bg.color;
                bg.color = new Color(c.r, c.g, c.b, c.a - Time.deltaTime * 0.75f);
                yield return null;
            }
        }
        
        IEnumerator LightenBg()
        {
            while (bg.color.a < 1)
            {
                var c = bg.color;
                bg.color = new Color(c.r, c.g, c.b, c.a + Time.deltaTime * 0.75f);
                yield return null;
            }
        }
        
        void IncreaseSkipBar()
        {
            if (skipBar.fillAmount >= 1 && !hasPlayerSkipped)
            {
                hasPlayerSkipped = true;
                StartCoroutine(StartGame());
            }
            else skipBar.fillAmount += Time.deltaTime;
        }

        void ReduceSkipBar()
        {
            if (!hasPlayerSkipped) skipBar.fillAmount -= Time.deltaTime * 0.5f;
        }

        IEnumerator StartGame()
        {
            StartCoroutine(AudioManager.Instance.StopSmoothly());
            yield return BlackScreenHandler.Instance.Darken();
            
            SceneParameters.StarGameSkippingMainMenu = true;
            SceneManager.LoadScene(0);
        }
    }
}