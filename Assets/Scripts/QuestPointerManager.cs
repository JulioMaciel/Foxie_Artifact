using System;
using System.Collections;
using ScriptObjects;
using TMPro;
using UnityEngine;

public class QuestPointerManager : MonoBehaviour
{
    [SerializeField] GameObject questPointer3D;
    [SerializeField] GameObject questPointer2D;
    [SerializeField] GameObject player;

    QuestPointerHandler questPointerHandler;
    TextMeshProUGUI questPointerText;
    Transform targetTransform;
    TargetItem currentTarget;
    
    public static QuestPointerManager Instance;
    
    public event Action<Target> OnApproachTarget;
    
    void Awake()
    {
        Instance = this;
        questPointerHandler = questPointer3D.GetComponentInChildren<QuestPointerHandler>();
        questPointerText = questPointer2D.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetNewTarget(TargetItem item, Transform target)
    {
        questPointer3D.SetActive(true);
        questPointer2D.SetActive(true);
        
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
        
        questPointer3D.SetActive(false);
        questPointer2D.SetActive(false);
    } 
}
