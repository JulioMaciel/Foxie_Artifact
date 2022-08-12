using System.Collections;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI
{
    public class SpeechBalloonHandler : MonoBehaviour
    {
        [SerializeField] GameObject speechBalloonObj;
        [SerializeField] SpriteRenderer contentRenderer;

        BalloonItem item;
        float duration;
        bool isShowingBalloon;

        public void ShowBalloon(BalloonItem balloonItem, float itemDuration, bool loop = false)
        {
            speechBalloonObj.SetActive(true);
            item = balloonItem;
            isShowingBalloon = true;
            duration = itemDuration;

            StartCoroutine(loop ? ShowSequentially() : ShowOnce());
        }

        IEnumerator ShowOnce()
        {
            var rnd = Random.Range(0, item.balloonContent.Length);
            contentRenderer.sprite = item.balloonContent[rnd];
            yield return new WaitForSeconds(duration);
            HideBalloon();
        }

        IEnumerator ShowSequentially()
        {
            var rnd = Random.Range(0, item.balloonContent.Length);
            contentRenderer.sprite = item.balloonContent[rnd];
            yield return new WaitForSeconds(duration);
            
            if (isShowingBalloon) StartCoroutine(ShowSequentially());
        }

        void HideBalloon()
        {
            speechBalloonObj.SetActive(false);
            isShowingBalloon = false;
        }
    }
}