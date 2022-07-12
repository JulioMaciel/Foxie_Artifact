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
    Transform target;
    Quest currentQuest;
    
    public static QuestPointerManager Instance;
    
    public event Action<Quest> OnApproachTarget;
    
    void Awake()
    {
        Instance = this;
        questPointerHandler = questPointer3D.GetComponentInChildren<QuestPointerHandler>();
        questPointerText = questPointer2D.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetNewTarget(Transform newTarget, string text, Quest quest)
    {
        questPointer3D.SetActive(true);
        questPointer2D.SetActive(true);
        
        questPointerHandler.SetTarget(newTarget);
        questPointerText.text = text;
        target = newTarget;
        currentQuest = quest;

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
        while (!player.transform.position.IsCloseEnough(target.position, 2f))
            yield return null;
        
        OnApproachTarget?.Invoke(currentQuest);
        
        questPointer3D.SetActive(false);
        questPointer2D.SetActive(false);
    } 
}
