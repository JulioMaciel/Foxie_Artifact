using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMode : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    
    void Start()
    {
        playerAnimator.SetTrigger(AnimParam.StandUp);
        AttackSnakeEvent.Instance.StartEvent();
        GetComponent<GameEvents>().enabled = false;
    }
}
