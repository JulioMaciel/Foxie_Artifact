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

namespace GameEvents
{
    public class AttackSnakeEvent : MonoBehaviour
    {
        [SerializeField] AttackSnakeControl attackSnakeControl;
        [SerializeField] Transform playerSpot;
        [SerializeField] Transform snakeSpot;
        [SerializeField] Transform goldieCongratsPlayer;
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
    
        public static AttackSnakeEvent Instance;
        void Awake() => Instance = this;

        void OnEnable()
        {
            SetObjects();
            GetComponents();
            playerMoveControl.enabled = false;
            gameplayGameplayCamera.enabled = false;
            snakeAttackCameraControl.enabled = true;
            snakeAnimalWanderer.enabled = false;
            snakeIdleEvent.enabled = false;
            goldieNavMesh.SetDestination(goldieCongratsPlayer.position);
            direction = (snakeSpot.transform.position - playerSpot.transform.position).normalized;
            attackSnakeControl.enabled = true;
            SetInitialPositions();
            attackSnakeControl.OnEventToTrigger += OnEventToTrigger;
        }

        void LateUpdate()
        {
            MovePlayerBySlider(attackSnakeControl.GetApproachSliderValue());
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

        void SetInitialPositions()
        {
            snakeNavMesh.ResetPath();
            snake.transform.position = snakeSpot.position;
            snake.transform.TurnBackOn(player.transform);
            snakeAnim.SetFloat(AnimParam.MoveSpeed, 0f);
            player.transform.position = playerSpot.position;
            player.transform.LookAt(snake.transform);
            attackSnakeControl.Reset();
        }

        void OnEventToTrigger(EventToTrigger eventToTrigger)
        {
            switch (eventToTrigger)
            {
                case EventToTrigger.HandleSnakeAttackFoxie: 
                    StartCoroutine(SnakeAttack()); break;
                case EventToTrigger.HandleFoxieAttackSnake: 
                    StartCoroutine(FoxieAttack()); break;
            }
        }

        void MovePlayerBySlider(float approachSliderValue)
        {
            if (approachSliderValue > 0)
                playerControl.Move(direction * approachSliderValue / 10);

            playerAnim.SetFloat(AnimParam.MoveSpeed, approachSliderValue);
            var walkMultiplier = Mathf.Clamp01(approachSliderValue * 3);
            playerAnim.SetFloat(AnimParam.Fox.WalkMultiplier, walkMultiplier);
        }

        IEnumerator SnakeAttack()
        {
            playerAnim.SetFloat(AnimParam.MoveSpeed, 0);
            snakeNavMesh.TrySetWorldDestination(player.transform.position);
            yield return snakeNavMesh.WaitToArrive(1.5f);
            snakeAnim.SetTrigger(AnimParam.Attack);
            snakeAudio.PlayClip(snakeAttackClip);
            yield return new WaitForSeconds(2);
            DialogueManager.Instance.StartDialogue(snakeAttackedDialogue);
            yield return new WaitForSeconds(3);
            SetInitialPositions();
        }

        IEnumerator FoxieAttack()
        {
            playerAnim.SetTrigger(AnimParam.Attack);
            playerAudio.PlayClip(foxAttackClip);
            yield return new WaitForSeconds(1);
            snakeAnim.SetTrigger(AnimParam.Snake.Die);
            snakeAudio.PlayClip(snakeDieClip);
            EndEvent();
        }

        void EndEvent()
        {
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
            attackSnakeControl.OnEventToTrigger -= OnEventToTrigger;
        }
    }
}