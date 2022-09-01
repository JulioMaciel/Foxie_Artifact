using System.Collections;
using System.Linq;
using GameEvents;
using Managers;
using ScriptableObjects;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class IntroHandler : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI textUI;
        [SerializeField] Image skipBar;
        [SerializeField] Image bg;
        [SerializeField] IntroTextItem[] introTextItems;
        [SerializeField] WakeUpEvent startGameEvent;
        [SerializeField] Camera menuCamera;
        [SerializeField] AudioClip letterTypedClip;

        IntroTextItem last;
        IntroTextItem current;
        Vector2 initBgPosition;
        AudioSource introSource;

        void Awake()
        {
            introSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            StartCoroutine(AudioManager.Instance.PlayIntroClip());
            StartCoroutine(BlackScreenHandler.Instance.Lighten(1f));
            initBgPosition = bg.rectTransform.offsetMax;
            current = introTextItems.First();
            ShowItem();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Escape)) IncreaseSkipBar();
            else if (skipBar.fillAmount > 0) ReduceSkipBar();

            if (Input.GetMouseButtonDown(0)) JumpToNextItem();
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
            bg.rectTransform.offsetMax = initBgPosition;
            bg.sprite = current.backgroundItem.image;
            StartCoroutine(LightenBg());
            StartCoroutine(MoveBg());
        }
        
        IEnumerator MoveBg()
        {
            while (bg.rectTransform.offsetMax.x > 0)
            {
                var max = bg.rectTransform.offsetMax;
                var min = bg.rectTransform.offsetMin;
                bg.rectTransform.offsetMax = new Vector2(max.x - Time.deltaTime * 10, max.y);
                bg.rectTransform.offsetMin = new Vector2(min.x - Time.deltaTime * 10, min.y);
                yield return null;
            }
        }
        
        IEnumerator ShowTextLetterByLetter()
        {
            textUI.text = string.Empty;
            foreach (var c in current.text)
            {
                textUI.text += c;
                introSource.PlayClip(letterTypedClip);
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
            skipBar.fillAmount += Time.deltaTime;

            if (skipBar.fillAmount >= 1)
            {
                // skipBar.fillAmount = 0;
                // JumpToNextItem();
                StartCoroutine(StartGame());
            }
        }

        void ReduceSkipBar()
        {
            skipBar.fillAmount -= Time.deltaTime * 0.5f;
        }

        IEnumerator StartGame()
        {
            StartCoroutine(AudioManager.Instance.StopSmoothly());
            yield return BlackScreenHandler.Instance.Darken();
            startGameEvent.enabled = true;
            menuCamera.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}