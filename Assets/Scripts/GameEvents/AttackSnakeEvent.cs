using System.Collections;
using AI;
using Cameras;
using Controller;
using Managers;
using ScriptableObjects;
using StaticData;
using Tools;
using UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace GameEvents
{
    public class AttackSnakeEvent : MonoBehaviour
    {
        [SerializeField] Transform playerSpot;
        [SerializeField] Transform snakeSpot;
        [SerializeField] Transform goldieCongratsPlayer;
        [SerializeField] AttackSnakeHandler attackSnakeHandler;
        [SerializeField] Slider approachSlider;
        [SerializeField] DialogueItem snakeAttackedDialogue;
        [SerializeField] DialogueItem snakedSuccessfullyAttacked;
        [SerializeField] AudioClip foxAttackClip;
        [SerializeField] AudioClip snakeAttackClip;
        [SerializeField] AudioClip snakeDieClip;

        MoveControl playerMoveControl;
        Animator playerAnim;
        Animator snakeAnim;
        Animator goldieAnimator;
        NavMeshAgent snakeNavMesh;
        NavMeshAgent goldieNavMesh;
        CharacterController playerControl;
        GameplayCamera gameplayGameplayCamera;
        AttackSnakeCamera snakeAttackCameraControl;
        AudioSource playerAudio;
        AudioSource snakeAudio;
        AnimalWanderer snakeAnimalWanderer;
        IdleEvent snakeIdleEvent;
        
        GameObject player;
        GameObject snake;
        GameObject goldie;
        Camera gameplayCamera;

        Vector3 direction;

        bool hasEventStarted;
        bool canPlayerApproach;
        float suspicionLevel;
        float awarenessLevel;
        float approachSliderValue;
        
        bool hasAwarenessReachedMaxLevel;
        bool hasPlayerApproachEnoughToAttack;
    
        public static AttackSnakeEvent Instance;
        void Awake() => Instance = this;

        void OnEnable()
        {
            approachSlider.onValueChanged.AddListener(value => approachSliderValue = value);
        
            SetObjects();
            GetComponents();
            hasEventStarted = true;
            attackSnakeHandler.ShowCanvas();
            playerMoveControl.enabled = false;
            gameplayGameplayCamera.enabled = false;
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
            HandleSuspicion();
        }

        void SetObjects()
        {
            gameplayCamera = Entity.Instance.gamePlayCamera;
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
            gameplayGameplayCamera = gameplayCamera.GetComponent<GameplayCamera>();
            snakeAttackCameraControl = gameplayCamera.GetComponent<AttackSnakeCamera>();
            playerAudio = player.GetComponent<AudioSource>();
            snakeAudio = snake.GetComponent<AudioSource>();
            snakeAnimalWanderer = snake.GetComponent<AnimalWanderer>();
            snakeIdleEvent = snake.GetComponent<IdleEvent>();
            
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
            hasAwarenessReachedMaxLevel = false;
            canPlayerApproach = true;
            approachSlider.interactable = true;
            attackSnakeHandler.UpdateSuspicionUI(suspicionLevel);
            attackSnakeHandler.UpdateAwarenessUI(awarenessLevel);        
        }

        void ReactToApproachSlider()
        {
            if (approachSliderValue > 0) 
                playerControl.Move(direction * approachSlider.value / 10);

            playerAnim.SetFloat(AnimParam.MoveSpeed, approachSlider.value);
            var walkMultiplier = Mathf.Clamp01(approachSlider.value * 3);
            playerAnim.SetFloat(AnimParam.Fox.WalkMultiplier, walkMultiplier);
        }

        void HandleSuspicion()
        {
            if (hasAwarenessReachedMaxLevel || hasPlayerApproachEnoughToAttack)
                return;
            
            if (approachSlider.value == 0 && suspicionLevel > 0) 
                suspicionLevel -= Time.deltaTime * 6;
            else
            {
                if (suspicionLevel <= 100) 
                    suspicionLevel += Time.deltaTime * approachSlider.value * 75;

                if (suspicionLevel >= 25)
                {
                    awarenessLevel += suspicionLevel / 150;
                    attackSnakeHandler.UpdateAwarenessUI(awarenessLevel);
                }
            }
            attackSnakeHandler.UpdateSuspicionUI(suspicionLevel);

            if (awarenessLevel >= 100)
            {
                hasAwarenessReachedMaxLevel = true;
                StartCoroutine(SnakeAttack());
            }
            else if (player.transform.position.IsCloseEnough(snake.transform.position, 1))
            {
                hasPlayerApproachEnoughToAttack = true;
                StartCoroutine(PlayerAttack());
            }
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

        void EndEvent()
        {
            hasEventStarted = false;
            attackSnakeHandler.HideCanvas();
            playerMoveControl.enabled = true;
            gameplayGameplayCamera.enabled = true;
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