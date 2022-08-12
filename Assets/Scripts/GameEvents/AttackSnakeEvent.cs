using System.Collections;
using AI;
using Cameras;
using Controller;
using Managers;
using ScriptableObjects;
using StaticData;
using Tools;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameEvents
{
    public class AttackSnakeEvent : MonoBehaviour
    {
        [SerializeField] Transform playerSpot;
        [SerializeField] Transform snakeSpot;
        [SerializeField] Transform goldieCongratsPlayer;
        [SerializeField] RectTransform attackSnakeCanvas;
        [SerializeField] RectTransform suspicionBar;
        [SerializeField] Slider awarenessSlider;
        [SerializeField] Slider approachSlider;
        [SerializeField] Image fillAreaAwarenessSlider;
        [SerializeField] DialogueItem snakeAttackedDialogue;
        [SerializeField] DialogueItem snakedSuccessfullyAttacked;
        [SerializeField] AudioClip foxAttackClip;
        [SerializeField] AudioClip snakeAttackClip;
        [SerializeField] AudioClip snakeDieClip;
    
        [SerializeField] Color32 awarenessInitialColor = new(255, 238, 196, 255);
        [SerializeField] Color32 awarenessFinalColor = new(246, 74, 57, 255);

        MoveControl playerMoveControl;
        Animator playerAnim;
        Animator snakeAnim;
        Animator goldieAnimator;
        NavMeshAgent snakeNavMesh;
        NavMeshAgent goldieNavMesh;
        CharacterController playerControl;
        FreeCameraControl freeFreeCameraControl;
        AttackSnakeCamera snakeAttackCameraControl;
        AudioSource playerAudio;
        AudioSource snakeAudio;
        AnimalWanderer snakeAnimalWanderer;
        IdleEvent snakeIdleEvent;
        Image suspiciousImage;
        GameObject player;
        GameObject snake;
        GameObject goldie;
        Camera mainCamera;

        Vector3 direction;

        bool hasEventStarted;
        bool canPlayerApproach;
        float suspicionLevel;
        float awarenessLevel;
        float approachSliderValue;

        PointerEventData onApproachInputPressed;
    
        public static AttackSnakeEvent Instance;
        void Awake() => Instance = this;

        void OnEnable()
        {
            approachSlider.onValueChanged.AddListener(value => approachSliderValue = value);
        
            SetObjects();
            GetComponents();
            hasEventStarted = true;
            attackSnakeCanvas.gameObject.SetActive(true);
            playerMoveControl.enabled = false;
            freeFreeCameraControl.enabled = false;
            snakeAttackCameraControl.enabled = true;
            snakeAnimalWanderer.enabled = false;
            snakeIdleEvent.enabled = false;
            goldieNavMesh.SetDestination(goldieCongratsPlayer.position);
            ResetControls();
            direction = (snakeSpot.transform.position - playerSpot.transform.position).normalized;
        }

        void Update()
        {
            if (!hasEventStarted || !canPlayerApproach) return;

            ReactToApproachSlider();
            StartCoroutine(HandleSuspicion());
        }

        void SetObjects()
        {
            mainCamera = Camera.main;
            player = Entity.Instance.player;
            snake = Entity.Instance.snake;
            goldie = Entity.Instance.goldie; 
        }

        void GetComponents()
        {
            playerMoveControl = player.GetComponent<MoveControl>();
            playerAnim = player.GetComponent<Animator>();
            snakeAnim = snake.GetComponent<Animator>();
            goldieAnimator = goldie.GetComponent<Animator>();
            snakeNavMesh = snake.GetComponent<NavMeshAgent>();
            goldieNavMesh = goldie.GetComponent<NavMeshAgent>();
            playerControl = player.GetComponent<CharacterController>();
            freeFreeCameraControl = mainCamera.GetComponent<FreeCameraControl>();
            snakeAttackCameraControl = mainCamera.GetComponent<AttackSnakeCamera>();
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
            snakeAudio.PlayClip(snakeAttackClip);
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
            playerAudio.PlayClip(foxAttackClip);
            yield return new WaitForSeconds(1);
            snakeAnim.SetTrigger(AnimParam.Snake.Die);
            snakeAudio.PlayClip(snakeDieClip);
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
            freeFreeCameraControl.enabled = true;
            snakeAttackCameraControl.enabled = false;
            playerAnim.SetFloat(AnimParam.Fox.WalkMultiplier, 1f);
            goldieAnimator.SetFloat(AnimParam.MoveSpeed, 0);
            goldie.transform.LookAt(player.transform);
            DialogueManager.Instance.StartDialogue(snakedSuccessfullyAttacked);
            GetComponent<BoringIndustryEvent>().enabled = true;
            Destroy(this);
        }

        void OnDisable()
        {
            approachSlider.onValueChanged.RemoveAllListeners();
        }
    }
}