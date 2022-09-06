using System.Collections;
using Cameras;
using Controller;
using Managers;
using ScriptableObjects;
using StaticData;
using Tools;
using UI;
using UnityEngine;
using UnityEngine.AI;

namespace GameEvents
{
    public class WakeUpEvent : MonoBehaviour
    {
        [SerializeField] WakeUpVolumeHandler wakeUpVolumeHandler;
        [SerializeField] DialogueItem welcomeFarmerDialogue;
        [SerializeField] DialogueItem snakeHuntDialogue;
        [SerializeField] DialogueItem snakeFoundDialogue;
        [SerializeField] AudioClip goldieBarkClip;
        [SerializeField] Transform goldieWelcomeFarmerSpot;
        [SerializeField] Transform farmerWaveSpot;
        [SerializeField] Transform farmerWorkSpot;
        [SerializeField] Transform goldieSnakePigSpot;

        Animator goldieAnimator;
        Animator playerAnimator;
        Animator farmerAnimator;
        MoveControl playerMoveControl;
        NavMeshAgent goldieNavmesh;
        NavMeshAgent farmerNavMesh;
        AudioSource goldieAudio;
        FirstPersonCamera firstPersonCamera;

        Camera gameplayCamera;
        GameObject player;
        GameObject goldie;
        GameObject snake;
        GameObject farmer;

        void Awake()
        {
            SetObjects();
            GetComponents();
        }

        void SetObjects()
        {
            gameplayCamera = Entity.Instance.gamePlayCamera;
            player = Entity.Instance.player;
            goldie = Entity.Instance.goldie;
            farmer = Entity.Instance.farmer;
            snake = Entity.Instance.snake;
        }

        void GetComponents()
        {
            goldieAnimator = goldie.GetComponent<Animator>();
            playerAnimator = player.GetComponent<Animator>();
            farmerAnimator = farmer.GetComponent<Animator>();
            playerMoveControl = player.GetComponent<MoveControl>();
            goldieNavmesh = goldie.GetComponent<NavMeshAgent>();
            farmerNavMesh = farmer.GetComponent<NavMeshAgent>();
            goldieAudio = goldie.GetComponent<AudioSource>();
            firstPersonCamera = gameplayCamera.GetComponent<FirstPersonCamera>();
        }

        void Start()
        {
            StartCoroutine(BlackScreenHandler.Instance.Lighten());
            StartCoroutine(AudioManager.Instance.PlayGameClip());
            gameplayCamera.gameObject.SetActive(true);
            playerAnimator.SetTrigger(AnimParam.Fox.Sleep);
            DialogueManager.Instance.StartDialogue(welcomeFarmerDialogue);
            playerMoveControl.enabled = false;
        }

        void OnEnable()
        {
            DialogueManager.Instance.OnEventToTrigger += OnEventToTrigger;
            QuestPointerManager.Instance.OnApproachTarget += OnEventToTrigger;
            TutorialManager.Instance.OnEventToTrigger += OnEventToTrigger;
        }

        void OnEventToTrigger(EventToTrigger eventToTrigger)
        {
            switch (eventToTrigger)
            {
                case EventToTrigger.CleanSleepingPP: wakeUpVolumeHandler.StartWakingUpEffect(); break;
                case EventToTrigger.MoveWakingCamera: MoveCameraToThirdPerson(); break;
                case EventToTrigger.EnablePlayerControl: StartCoroutine(EnablePlayerControl()); break;
                case EventToTrigger.SetGoldieAsFirstTarget: SetGoldieAsFirstTarget(); break;
                case EventToTrigger.StartFarmerAnimation: StartCoroutine(FarmerLeaveHouse()); break;
                case EventToTrigger.SetSnakeTarget: SetSnakeTarget(); break;
                case EventToTrigger.ShowSnakeFoundDialogue: DialogueManager.Instance.StartDialogue(snakeFoundDialogue); break;
                case EventToTrigger.StartAttackSnakeEvent: StartAttackSnake(); break;
            }
        }

        void MoveCameraToThirdPerson()
        {
            firstPersonCamera.ChangeToThirdPerson();
            GoldieBarks();
        }

        void GoldieBarks()
        {
            goldieAnimator.SetTrigger(AnimParam.Goldie.Bark);
            goldieAudio.PlayClip(goldieBarkClip);   
        }

        IEnumerator EnablePlayerControl()
        {
            gameplayCamera.GetComponent<GameplayCamera>().enabled = true;
            playerAnimator.SetTrigger(AnimParam.Fox.StandUp);
            yield return playerAnimator.WaitAnimationStart(AnimClip.Idle);

            playerMoveControl.enabled = true;

            StartCoroutine(MoveGoldieToWelcomeSpot());
        }

        IEnumerator MoveGoldieToWelcomeSpot()
        {
            TutorialManager.Instance.StartWakeUpTutorial();
            StartCoroutine(goldieNavmesh.MoveAnimating(goldieWelcomeFarmerSpot.position));
            yield return goldieNavmesh.WaitToArrive();
            goldie.transform.LookAt(farmer.transform);
        }

        void SetGoldieAsFirstTarget()
        {
            GoldieBarks();
            QuestPointerManager.Instance.SetNewTarget(goldie, EventToTrigger.StartFarmerAnimation, "Follow Goldie");
        }

        IEnumerator FarmerLeaveHouse()
        {
            StartCoroutine(farmerNavMesh.MoveAnimating(farmerWaveSpot.position));
        
            yield return farmerNavMesh.WaitToArrive();
        
            farmer.transform.LookAt(goldie.transform);
            farmerAnimator.SetTrigger(AnimParam.Human.Wave);
            yield return new WaitForSeconds(3);
        
            DialogueManager.Instance.StartDialogue(snakeHuntDialogue);
            
            yield return farmerNavMesh.MoveAnimating(farmerWorkSpot.position, 1f);
            yield return new WaitForSeconds(.5f);
            farmerAnimator.SetBool(AnimParam.Human.IsWorking, true);
        }

        void SetSnakeTarget()
        {
            StartCoroutine(goldieNavmesh.MoveAnimating(goldieSnakePigSpot.position));
            QuestPointerManager.Instance.SetNewTarget(snake, EventToTrigger.ShowSnakeFoundDialogue, "Search for snakes around the chicks");
        }

        void StartAttackSnake()
        {
            GetComponent<AttackSnakeEvent>().enabled = true;
            Destroy(this);
        }

        void OnDisable()
        {
            DialogueManager.Instance.OnEventToTrigger -= OnEventToTrigger;
            QuestPointerManager.Instance.OnApproachTarget -= OnEventToTrigger;
            TutorialManager.Instance.OnEventToTrigger -= OnEventToTrigger;
        }
    }
}
