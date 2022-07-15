using System;
using System.Collections;
using ScriptObjects;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttackSnakeEvent : MonoBehaviour
{
    [SerializeField] GameObject freeCamera;
    [SerializeField] GameObject player;
    [SerializeField] GameObject snake;
    [SerializeField] Transform playerSpot;
    [SerializeField] Transform snakeSpot;
    [SerializeField] GameObject attackSnakeCanvas;
    [SerializeField] RectTransform suspicionBar;
    [SerializeField] Slider awarenessSlider;
    [SerializeField] Slider approachSlider;
    [SerializeField] EventTrigger knobEventTrigger; // this shit makes the component static
    [SerializeField] DialogueItem snakeAttackedDialogue;

    MoveControl playerMoveControl;
    Animator playerAnim;
    Animator snakeAnim;
    NavMeshAgent snakeNavMesh;
    CharacterController playerControl;
    CameraControl freeCameraControl;

    bool hasEventStarted;
    float suspicionLevel;
    float awarenessLevel;
    float secondsPressingSlider;

    PointerEventData onApproachInputPressed;
    public Action OnEventEnds; 
    
    public static AttackSnakeEvent Instance;
    void Awake() => Instance = this;

    void Update()
    {
        if (!hasEventStarted) return;

        if (secondsPressingSlider >= 1)
            IncreaseSuspicion();

        if (awarenessLevel >= 100)
            StartCoroutine(SnakeAttack());

        if (IsCloseEnoughToAttack())
            StartCoroutine(PlayerAttack());
    }

    public void StartEvent()
    {
        GetRequiredComponents();
        hasEventStarted = true;
        attackSnakeCanvas.SetActive(true);
        playerMoveControl.enabled = false;
        snake.GetComponent<AnimalWanderer>().enabled = false;
        snake.GetComponent<IdleEvent>().enabled = false;
        StartCoroutine(ManageCamera());
        SubscribeKnobEvents();
        ResetPositions();
        // TODO: reduce speed animation walk player
    }

    void GetRequiredComponents()
    {
        playerMoveControl = player.GetComponent<MoveControl>();
        playerAnim = player.GetComponent<Animator>();
        snakeAnim = snake.GetComponent<Animator>();
        snakeNavMesh = snake.GetComponent<NavMeshAgent>();
        playerControl = player.GetComponent<CharacterController>();
        freeCameraControl = freeCamera.GetComponent<CameraControl>();
    }

    void SubscribeKnobEvents()
    {
        var entry = new EventTrigger.Entry {eventID = EventTriggerType.PointerDown};
        entry.callback.AddListener(ReactApproachSliderDown);
        knobEventTrigger.triggers.Add(entry);
        entry = new EventTrigger.Entry {eventID = EventTriggerType.PointerUp};
        entry.callback.AddListener(ReactApproachSliderUp);
        knobEventTrigger.triggers.Add(entry);
    }

    void ResetPositions()
    {
        player.transform.position = playerSpot.position;
        player.transform.LookAt(snake.transform);
        snakeNavMesh.ResetPath();
        snake.transform.position = snakeSpot.position;
        snake.transform.TurnBackOn(player.transform);
    }

    void ResetControls()
    {
        suspicionLevel = 0;
        awarenessLevel = 0;
        secondsPressingSlider = 0;
        UpdateSuspicionUI(suspicionLevel);
        UpdateAwarenessUI(awarenessLevel);
        approachSlider.value = 0;        
    }

    IEnumerator ManageCamera()
    {
        yield return new WaitForSeconds(1);
        freeCameraControl.enabled = false;
        yield return snakeNavMesh.WaitToArrive();
        yield return freeCamera.transform.LookAtSmoothly(snake.transform, 1f);
    }

    public void ReactApproachSliderDown(BaseEventData data)
    {
        secondsPressingSlider += Time.deltaTime;
        
        playerControl.Move(Vector3.forward * Time.deltaTime);
        playerAnim.SetFloat(AnimParam.MoveSpeed, 0.2f);
    }

    public void ReactApproachSliderUp(BaseEventData data)
    {
        secondsPressingSlider = 0;
        playerAnim.SetFloat(AnimParam.MoveSpeed, 0);
        DecreaseSuspicion();
    }

    void IncreaseSuspicion()
    {
        if (suspicionLevel <= 100)
        {
            suspicionLevel += Time.deltaTime * approachSlider.value;
            UpdateSuspicionUI(suspicionLevel);
        }

        if (suspicionLevel >= 50)
        {
            awarenessLevel  += Time.deltaTime * 50;
            UpdateAwarenessUI(awarenessLevel);
        }
    }
    
    void DecreaseSuspicion()
    {
        if (suspicionLevel >= 0)
            suspicionLevel -= Time.deltaTime * 20;
    }

    bool IsCloseEnoughToAttack()
    {
        return player.transform.position.IsCloseEnough(snake.transform.position);
    }

    IEnumerator SnakeAttack()
    {
        snakeNavMesh.TrySetWorldDestination(player.transform.position);
        yield return snakeNavMesh.WaitToArrive();
        snakeAnim.SetTrigger(AnimParam.Attack);
        yield return new WaitForSeconds(2);
        DialogueManager.Instance.StartDialogue(snakeAttackedDialogue);
        yield return new WaitForSeconds(3);
        ResetPositions();
        ResetControls();
    }

    IEnumerator PlayerAttack()
    {
        playerAnim.SetTrigger(AnimParam.Attack);
        yield return new WaitForSeconds(1);
        snakeAnim.SetTrigger(AnimParam.Snake.Die);
        EndEvent();
    }

    void EndEvent()
    {
        hasEventStarted = false;
        attackSnakeCanvas.SetActive(false);
        playerMoveControl.enabled = true;
        freeCameraControl.enabled = false;
        OnEventEnds?.Invoke();
    }
    
     void UpdateAwarenessUI(float awareness)
     {
         var scaledAwareness = awareness / 100;
         awarenessSlider.value = scaledAwareness;
     }

     void UpdateSuspicionUI(float suspicion)
     {
         var scaledSuspicion = suspicion / 100;
         suspicionBar.localScale = new Vector3(1f, scaledSuspicion, 1f);
     }

    void OnDisable()
    {
        knobEventTrigger.triggers.Clear();
    }
}