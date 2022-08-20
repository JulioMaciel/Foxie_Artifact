using System;
using System.Collections;
using StaticData;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] GameObject tutorialUI;
        [SerializeField] string wasdText;
        [SerializeField] string jumpText;
        [SerializeField] Color controlPressedTextColor;
        [SerializeField] float minTimeControlPressed = 2;
        [SerializeField] float initialUIMovementSpeed = 1.1f;

        RectTransform tutorialUIRect;
        TextMeshProUGUI textMeshPro;
        TutorialControl currentControl;
        Color controlStartingTextColor;
        float timeControlPressed;

        public event Action<EventToTrigger> OnEventToTrigger;
        
        public static TutorialManager Instance;
        void Awake() => Instance = this;

        public void StartTutorial()
        {
            tutorialUI.SetActive(true);
            textMeshPro = tutorialUI.GetComponentInChildren<TextMeshProUGUI>();
            tutorialUIRect = tutorialUI.GetComponent<RectTransform>();
            controlStartingTextColor = textMeshPro.color;
            currentControl = TutorialControl.MouseCamera;
            StartCoroutine(MoveWaitAndChange());
        }

        IEnumerator MoveWaitAndChange()
        {
            yield return MoveDownWaitMoveUp();
            currentControl = TutorialControl.WASD;
            textMeshPro.text = wasdText;
            yield return MoveDownWaitMoveUp();
            currentControl = TutorialControl.Jump;
            textMeshPro.text = jumpText;
            yield return MoveDownWaitMoveUp();
            OnEventToTrigger?.Invoke(EventToTrigger.SetGoldieAsFirstTarget);
        }

        IEnumerator MoveDownWaitMoveUp()
        {
            yield return MoveUI(true);
            yield return WaitControlPressedEnough();
            yield return MoveUI(false);
        }

        IEnumerator MoveUI(bool goDown)
        {
            if(goDown) textMeshPro.color = controlStartingTextColor;
            
            var targetY = tutorialUIRect.anchoredPosition.y + 100 * (goDown? -1 : 1);
            var timePast = 0f;
            while(Math.Abs(tutorialUIRect.anchoredPosition.y - targetY) > 0.1)
            {
                timePast += Time.deltaTime;
                var y = Mathf.Lerp(tutorialUIRect.anchoredPosition.y, targetY, timePast * initialUIMovementSpeed);
                tutorialUIRect.anchoredPosition = new Vector2(tutorialUIRect.anchoredPosition.x, y);
                yield return null;
            }
            
        }

        IEnumerator WaitControlPressedEnough()
        {
            timeControlPressed = 0f;
            while (timeControlPressed < minTimeControlPressed)
            {
                if (currentControl == TutorialControl.MouseCamera && Input.GetMouseButton(0) || 
                    currentControl == TutorialControl.WASD && Input.GetKey(KeyCode.W) ||
                    currentControl == TutorialControl.WASD && Input.GetKey(KeyCode.A) ||
                    currentControl == TutorialControl.WASD && Input.GetKey(KeyCode.S) ||
                    currentControl == TutorialControl.WASD && Input.GetKey(KeyCode.D) ||
                    currentControl == TutorialControl.Jump && Input.GetKey(KeyCode.Space))
                {
                    if (currentControl is TutorialControl.Jump) minTimeControlPressed *= 0.5f;
                    
                    timeControlPressed += Time.deltaTime;
                    var percentTotal = timeControlPressed / minTimeControlPressed;
                    textMeshPro.color = Color.Lerp(controlStartingTextColor,controlPressedTextColor, percentTotal);
                }

                yield return null;
            }
        }

        internal enum TutorialControl
        {
            MouseCamera,
            WASD,
            Jump
        }
    }
}