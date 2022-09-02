using System.Collections;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class EndGameHandler : MonoBehaviour
    {
        Button mainMenuBtn;
        TextMeshProUGUI textMesh;
        CanvasGroup canvasGroup;

        void Awake()
        {
            mainMenuBtn = GetComponentInChildren<Button>();
            textMesh = GetComponent<TextMeshProUGUI>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        void OnEnable()
        {
            mainMenuBtn.onClick.AddListener(() => StartCoroutine(ReactToMainMenuClick()));
        }

        void Start()
        {
            StartCoroutine(FadeInUI());
        }

        IEnumerator FadeInUI()
        {
            textMesh.alpha = 0;
            canvasGroup.alpha = 0;
            
            StartCoroutine(BlackScreenHandler.Instance.Darken(0.1f));
            while (textMesh.alpha < 1)
            {
                textMesh.alpha += Time.deltaTime * 0.75f;
                yield return null;
            }
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime * 0.75f;
                yield return null;
            }
        }

        IEnumerator ReactToMainMenuClick()
        {
            StartCoroutine(AudioManager.Instance.StopSmoothly());
            yield return BlackScreenHandler.Instance.Darken();
            SceneManager.LoadScene(0);
        }

        void OnDisable()
        {
            mainMenuBtn.onClick.RemoveAllListeners();
        }
    }
}
