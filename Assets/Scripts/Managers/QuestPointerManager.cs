using System;
using System.Collections;
using ScriptableObjects;
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
        EventToTrigger eventToTrigger;
    
        public static QuestPointerManager Instance;
    
        public event Action<EventToTrigger> OnApproachTarget;
    
        void Awake()
        {
            Instance = this;
            player = Entity.Instance.player;
            questPointerHandler = questPointer3D.GetComponentInChildren<QuestPointerHandler>();
            questPointerText = questPointer2D.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetNewTarget(GameObject target, EventToTrigger eventToTriggerWhenApproach, string underPointerInstruction)
        {
            questPointer3D.gameObject.SetActive(true);
            questPointer2D.gameObject.SetActive(true);
        
            //currentTarget = item;
            targetTransform = target.transform;
            eventToTrigger = eventToTriggerWhenApproach;
            questPointerHandler.SetTarget(targetTransform);
            questPointerText.text = underPointerInstruction;

            StartCoroutine(EraseMessageLater());
            StartCoroutine(CheckIfApproachedTarget());
        }

        IEnumerator EraseMessageLater()
        {
            yield return new WaitForSeconds(5);
            questPointerText.text = string.Empty;
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
