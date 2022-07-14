using System;
using System.Collections;
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
    Target currentTarget;
    
    public static QuestPointerManager Instance;
    
    public event Action<Target> OnApproachTarget;
    
    void Awake()
    {
        Instance = this;
        questPointerHandler = questPointer3D.GetComponentInChildren<QuestPointerHandler>();
        questPointerText = questPointer2D.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetNewTarget(Transform newTarget, string text, Target target)
    {
        questPointer3D.SetActive(true);
        questPointer2D.SetActive(true);
        
        questPointerHandler.SetTarget(newTarget);
        questPointerText.text = text;
        targetTransform = newTarget;
        currentTarget = target;

        StartCoroutine(EraseMessage());
        StartCoroutine(CheckIfApproachedTarget());
    }

    IEnumerator EraseMessage()
    {
        yield return new WaitForSeconds(5);
        questPointerText.text = string.Empty;
    }

    IEnumerator CheckIfApproachedTarget()
    {
        while (!player.transform.position.IsCloseEnough(targetTransform.position, 2f))
            yield return null;
        
        OnApproachTarget?.Invoke(currentTarget);
        
        questPointer3D.SetActive(false);
        questPointer2D.SetActive(false);
    } 
}
