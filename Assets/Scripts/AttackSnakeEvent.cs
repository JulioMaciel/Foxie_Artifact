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
    [SerializeField] RectTransform attackSnakeCanvas;
    [SerializeField] RectTransform suspicionBar;
    [SerializeField] Slider awarenessSlider;
    [SerializeField] Slider approachSlider;
    [SerializeField] Image fillAreaAwarenessSlider;
    [SerializeField] DialogueItem snakeAttackedDialogue;
    [SerializeField] AudioClip foxAttackClip;
    [SerializeField] AudioClip snakeAttackClip;
    [SerializeField] AudioClip snakeDieClip;
    
    [SerializeField] Color32 awarenessInitialColor = new(255, 238, 196, 255);
    [SerializeField] Color32 awarenessFinalColor = new(246, 74, 57, 255);

    MoveControl playerMoveControl;
    Animator playerAnim;
    Animator snakeAnim;
    NavMeshAgent snakeNavMesh;
    CharacterController playerControl;
    CameraControl freeCameraControl;
    AttackSnakeCamera attackSnakeCamera;
    AudioSource playerAudio;
    AudioSource snakeAudio;
    AnimalWanderer snakeAnimalWanderer;
    IdleEvent snakeIdleEvent;
    Image suspiciousImage;

    Vector3 direction;

    bool hasEventStarted;
    bool canPlayerApproach;
    float suspicionLevel;
    float awarenessLevel;
    float approachSliderValue;

    PointerEventData onApproachInputPressed;
    public Action OnEventEnds; 
    
    public static AttackSnakeEvent Instance;
    void Awake() => Instance = this;

    // void Start()
    // {
    //     direction = (snakeSpot.transform.position - playerSpot.transform.position).normalized;
    // }

    void OnEnable()
    {
        approachSlider.onValueChanged.AddListener(value => approachSliderValue = value);
        
        GetRequiredComponents();
        hasEventStarted = true;
        attackSnakeCanvas.gameObject.SetActive(true);
        playerMoveControl.enabled = false;
        freeCameraControl.enabled = false;
        attackSnakeCamera.enabled = true;
        snakeAnimalWanderer.enabled = false;
        snakeIdleEvent.enabled = false;
        ResetControls();
        direction = (snakeSpot.transform.position - playerSpot.transform.position).normalized;
    }

    void Update()
    {
        if (!hasEventStarted || !canPlayerApproach) return;

        ReactToApproachSlider();
        StartCoroutine(HandleSuspicion());
    }

    // public void StartEvent()
    // {
    //     GetRequiredComponents();
    //     hasEventStarted = true;
    //     attackSnakeCanvas.gameObject.SetActive(true);
    //     playerMoveControl.enabled = false;
    //     freeCameraControl.enabled = false;
    //     attackSnakeCamera.enabled = true;
    //     snakeAnimalWanderer.enabled = false;
    //     snakeIdleEvent.enabled = false;
    //     ResetControls();
    // }

    void GetRequiredComponents()
    {
        playerMoveControl = player.GetComponent<MoveControl>();
        playerAnim = player.GetComponent<Animator>();
        snakeAnim = snake.GetComponent<Animator>();
        snakeNavMesh = snake.GetComponent<NavMeshAgent>();
        playerControl = player.GetComponent<CharacterController>();
        freeCameraControl = freeCamera.GetComponent<CameraControl>();
        attackSnakeCamera = freeCamera.GetComponent<AttackSnakeCamera>();
        playerAudio = player.GetComponent<AudioSource>();
        snakeAudio = snake.GetComponent<AudioSource>();
        snakeAnimalWanderer = snake.GetComponent<AnimalWanderer>();
        snakeIdleEvent = snake.GetComponent<IdleEvent>();
        suspiciousImage = suspicionBar.GetComponent<Image>();
    }

    void ResetControls()
    {
        snakeNavMesh.ResetPath();
        snake.transform.position = snakeSpot.position;
        snake.transform.TurnBackOn(player.transform);
        snakeAnim.SetFloat(AnimParam.MoveSpeed, 0f);
        player.transform.position = playerSpot.position;
        player.transform.LookAt(snake.transform);
        suspicionLevel = 0;
        awarenessLevel = 0;
        approachSlider.value = 0;
        canPlayerApproach = true;
        approachSlider.interactable = true;
        UpdateSuspicionUI();
        UpdateAwarenessUI();        
    }

    void ReactToApproachSlider()
    {
        if (approachSliderValue > 0) 
            playerControl.Move(direction * approachSlider.value / 10);

        playerAnim.SetFloat(AnimParam.MoveSpeed, approachSlider.value);
        var walkMultiplier = Mathf.Clamp01(approachSlider.value * 3);
        playerAnim.SetFloat(AnimParam.Fox.WalkMultiplier, walkMultiplier);
    }

    IEnumerator HandleSuspicion()
    {
        if (approachSlider.value == 0 && suspicionLevel > 0) 
            suspicionLevel -= Time.deltaTime * 6;
        else
        {
            if (suspicionLevel <= 100) 
                suspicionLevel += Time.deltaTime * approachSlider.value * 75;

            if (suspicionLevel >= 25)
            {
                awarenessLevel += suspicionLevel / 150;
                UpdateAwarenessUI();
            }
        }
        UpdateSuspicionUI();

        if (awarenessLevel >= 100)
            yield return SnakeAttack();
        else if (player.transform.position.IsCloseEnough(snake.transform.position, 1))
            yield return PlayerAttack();
    }

    IEnumerator SnakeAttack()
    {
        canPlayerApproach = false;
        approachSlider.interactable = false;
        playerAnim.SetFloat(AnimParam.MoveSpeed, 0);
        snakeNavMesh.TrySetWorldDestination(player.transform.position);
        yield return snakeNavMesh.WaitToArrive(1.5f);
        snakeAnim.SetTrigger(AnimParam.Attack);
        snakeAudio.clip = snakeAttackClip;
        snakeAudio.Play();
        yield return new WaitForSeconds(2);
        DialogueManager.Instance.StartDialogue(snakeAttackedDialogue);
        yield return new WaitForSeconds(3);
        ResetControls();
    }

    IEnumerator PlayerAttack()
    {
        canPlayerApproach = false;
        approachSlider.interactable = false;
        playerAnim.SetTrigger(AnimParam.Attack);
        playerAudio.clip = foxAttackClip;
        playerAudio.Play();
        yield return new WaitForSeconds(1);
        snakeAnim.SetTrigger(AnimParam.Snake.Die);
        snakeAudio.clip = snakeDieClip;
        snakeAudio.Play();
        //TODO: add poof particle effect
        EndEvent();
    }
    
    void UpdateAwarenessUI()
    {
        var scaledAwareness = awarenessLevel / 100;
        awarenessSlider.value = scaledAwareness;
        
        var nextColor = Color32.Lerp(awarenessInitialColor,awarenessFinalColor, scaledAwareness);
        fillAreaAwarenessSlider.color = nextColor;
    }

    void UpdateSuspicionUI()
    {
        var scaledSuspicion = suspicionLevel / 100;
        suspicionBar.localScale = new Vector3(1f, scaledSuspicion, 1f);
        
        var currentAlpha = Mathf.Lerp(115, 215, scaledSuspicion);
        var tempColor = (Color32)suspiciousImage.color;
        tempColor.a = (byte)currentAlpha;
        suspiciousImage.color = tempColor;
    }

    void EndEvent()
    {
        hasEventStarted = false;
        attackSnakeCanvas.gameObject.SetActive(false);
        playerMoveControl.enabled = true;
        freeCameraControl.enabled = true;
        attackSnakeCamera.enabled = false;
        playerAnim.SetFloat(AnimParam.Fox.WalkMultiplier, 1f);
        OnEventEnds?.Invoke();
    }

    void OnDisable()
    {
        approachSlider.onValueChanged.RemoveAllListeners();
    }
}