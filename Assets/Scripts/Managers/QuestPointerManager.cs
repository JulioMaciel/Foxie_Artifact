using System;
using System.Collections;
using StaticData;
using TMPro;
using Tools;
using UI;
using UnityEngine;

namespace Managers
{
    public class QuestPointerManager : MonoBehaviour
    {
        [SerializeField] Transform questPointer3D;
        [SerializeField] RectTransform questPointer2D;
        
        GameObject player;
        QuestPointerHandler questPointerHandler;
        TextMeshProUGUI questPointerText;
        Transform targetTransform;
        CanvasGroup textCanvasGroup;
        RectTransform textRectFrame;
        EventToTrigger eventToTrigger;
    
        public static QuestPointerManager Instance;
    
        public event Action<EventToTrigger> OnApproachTarget;
    
        void Awake()
        {
            Instance = this;
            player = Entity.Instance.player;
            questPointerHandler = questPointer3D.GetComponentInChildren<QuestPointerHandler>();
            questPointerText = questPointer2D.GetComponentInChildren<TextMeshProUGUI>();
            textCanvasGroup = questPointer2D.GetComponentInChildren<CanvasGroup>();
            textRectFrame = textCanvasGroup.GetComponent<RectTransform>();
        }

        public void SetNewTarget(GameObject target, EventToTrigger eventToTriggerWhenApproach, string underPointerInstruction)
        {
            questPointer3D.gameObject.SetActive(true);
            questPointer2D.gameObject.SetActive(true);
        
            targetTransform = target.transform;
            eventToTrigger = eventToTriggerWhenApproach;
            questPointerHandler.SetTarget(targetTransform);
            questPointerText.text = underPointerInstruction;
            textRectFrame.sizeDelta = new Vector2(underPointerInstruction.Length * 11f, textRectFrame.sizeDelta.y);
            
            StartCoroutine(ShowInstruction());
            StartCoroutine(HideInstruction());
            StartCoroutine(CheckIfApproachedTarget());
        }
        
        IEnumerator ShowInstruction()
        {
            while (textCanvasGroup.alpha < 1)
            {
                textCanvasGroup.alpha += Time.deltaTime * 5;
                yield return null;
            }
        }

        IEnumerator HideInstruction()
        {
            yield return new WaitForSeconds(5);
            while (textCanvasGroup.alpha > 0)
            {
                textCanvasGroup.alpha -= Time.deltaTime * 5;
                yield return null;
            }
        }

        IEnumerator CheckIfApproachedTarget()
        {
            while (!player.transform.position.IsCloseEnough(targetTransform.position, 2f))
                yield return null;
        
            OnApproachTarget?.Invoke(eventToTrigger);
        
            questPointer3D.gameObject.SetActive(false);
            questPointer2D.gameObject.SetActive(false);
        } 
    }
}
