using UnityEngine;

public class DebugMode : MonoBehaviour
{
    [SerializeField] MainGameEvent startAtEvent;
    
    void Start()
    {
        switch (startAtEvent)
        {
            case MainGameEvent.AttackSnake:
                GetComponent<WakeUpEvent>().enabled = false;
                GetComponent<AttackSnakeEvent>().enabled = true;
                break;
        }
    }

    public enum MainGameEvent
    {
        WakeUp,
        AttackSnake,
    }
}
