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
    [SerializeField] DialogueItem snakeAttackedDialogue;
    [SerializeField] AudioClip foxAttackClip;
    [SerializeField] AudioClip snakeAttackClip;
    [SerializeField] AudioClip snakeDieClip;

    MoveControl playerMoveControl;
    Animator playerAnim;
    Animator snakeAnim;
    NavMeshAgent snakeNavMesh;
    CharacterController playerControl;
    CameraControl freeCameraControl;
    AttackSnakeCamera attackSnakeCamera;
    AudioSource playerAudio;
    AudioSource snakeAudio;

    Vector3 direction;

    bool hasEventStarted;
    float suspicionLevel;
    float awarenessLevel;
    float approachSliderValue;

    PointerEventData onApproachInputPressed;
    public Action OnEventEnds; 
    
    public static AttackSnakeEvent Instance;
    void Awake() => Instance = this;

    void Start() => direction = (snakeSpot.transform.position - playerSpot.transform.position).normalized;

    void OnEnable() => approachSlider.onValueChanged.AddListener(value => approachSliderValue = value);

    void Update()
    {
        if (!hasEventStarted) return;

        if (awarenessLevel >= 100)
            StartCoroutine(SnakeAttack());
        else if (IsCloseEnoughToAttack())
            StartCoroutine(PlayerAttack());
        else
        {
            ReactToApproachSlider();
            HandleSuspicion();
        }
    }

    public void StartEvent()
    {
        GetRequiredComponents();
        hasEventStarted = true;
        attackSnakeCanvas.gameObject.SetActive(true);
        playerMoveControl.enabled = false;
        freeCameraControl.enabled = false;
        attackSnakeCamera.enabled = true;
        snake.GetComponent<AnimalWanderer>().enabled = false;
        snake.GetComponent<IdleEvent>().enabled = false;
        //StartCoroutine(ManageCamera());
        ResetControls();
        // TODO: reduce speed animation walk player???
    }

    void GetRequiredComponents()
    {
        playerMoveControl = player.GetComponent<MoveControl>();
        playerAnim = player.GetComponent<Animator>();
        snakeAnim = snake.GetComponent<Animator>();
        snakeNavMesh = snake.GetComponent<NavMeshAgent>();
        playerControl = player.GetComponent<CharacterController>();
        freeCameraControl = freeCamera.GetComponent<CameraControl>();
        attackSnakeCamera = freeCamera.GetComponent<AttackSnakeCamera>();
        playerAudio =  player.GetComponent<AudioSource>();
        snakeAudio =  snake.GetComponent<AudioSource>();
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
        UpdateSuspicionUI();
        UpdateAwarenessUI();        
    }

    IEnumerator ManageCamera()
    {
        yield return new WaitForSeconds(1);
        freeCameraControl.enabled = false;
        yield return snakeNavMesh.WaitToArrive();
        yield return freeCamera.transform.LookAtSmoothly(snake.transform, 1f);
    }

    void ReactToApproachSlider()
    {
        if (approachSliderValue > 0)
        {
            playerControl.Move(direction * Time.deltaTime / 6);
            playerAnim.SetFloat(AnimParam.MoveSpeed, .01f);
        }
        else
            playerAnim.SetFloat(AnimParam.MoveSpeed, 0);
    }

    void HandleSuspicion()
    {
        if (approachSlider.value == 0 && suspicionLevel > 0) 
            suspicionLevel -= Time.deltaTime * 2;
        else
        {
            if (suspicionLevel <= 100) 
                suspicionLevel += Time.deltaTime * approachSlider.value * 50;

            if (suspicionLevel >= 25)
            {
                awarenessLevel += Time.deltaTime * 5f;
                UpdateAwarenessUI();
            }
        }
        UpdateSuspicionUI();
    }

    bool IsCloseEnoughToAttack()
    {
        return player.transform.position.IsCloseEnough(snake.transform.position);
    }

    IEnumerator SnakeAttack()
    {
        playerAnim.SetFloat(AnimParam.MoveSpeed, 0);
        snakeNavMesh.TrySetWorldDestination(player.transform.position);
        yield return snakeNavMesh.WaitToArrive(2);
        // closer camera
        //StartCoroutine(freeCamera.transform.MoveUntilArrive(player.transform, .5f, 3f));
        
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
    }

    void UpdateSuspicionUI()
    {
        var scaledSuspicion = suspicionLevel / 100;
        suspicionBar.localScale = new Vector3(1f, scaledSuspicion, 1f);
    }

    void EndEvent()
    {
        hasEventStarted = false;
        attackSnakeCanvas.gameObject.SetActive(false);
        playerMoveControl.enabled = true;
        freeCameraControl.enabled = true;
        attackSnakeCamera.enabled = false;
        OnEventEnds?.Invoke();
    }

    void OnDisable()
    {
        approachSlider.onValueChanged.RemoveAllListeners();
    }
}