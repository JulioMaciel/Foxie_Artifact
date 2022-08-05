using System;
using System.Collections;
using GameEvents;
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
        TargetItem currentTarget;
    
        public static QuestPointerManager Instance;
    
        public event Action<Target> OnApproachTarget;
    
        void Awake()
        {
            Instance = this;
            player = Entity.Instance.player;
            questPointerHandler = questPointer3D.GetComponentInChildren<QuestPointerHandler>();
            questPointerText = questPointer2D.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetNewTarget(TargetItem item, Transform target)
        {
            questPointer3D.gameObject.SetActive(true);
            questPointer2D.gameObject.SetActive(true);
        
            currentTarget = item;
            targetTransform = target;
            questPointerHandler.SetTarget(targetTransform);
            questPointerText.text = item.message;

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
        
            OnApproachTarget?.Invoke(currentTarget.target);
        
            questPointer3D.gameObject.SetActive(false);
            questPointer2D.gameObject.SetActive(false);
        } 
    }
}
