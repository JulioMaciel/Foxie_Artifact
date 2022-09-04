using System;
using Managers;
using StaticData;
using Tools;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Controller
{
    public class AttackSnakeControl : MonoBehaviour
    {
        [SerializeField] AttackSnakeHandler attackSnakeHandler;
        [SerializeField] Slider approachSlider;
        
        GameObject player;
        GameObject snake;
        
        float suspicionLevel;
        float awarenessLevel;
        bool hasAwarenessReachedMaxLevel;
        bool hasPlayerApproachEnoughToAttack;
        
        public event Action<EventToTrigger> OnEventToTrigger;

        void Awake()
        {
            player = Entity.Instance.player;
            snake = Entity.Instance.snake;
        }

        void OnEnable()
        {
            attackSnakeHandler.ShowCanvas();
        }

        void Update()
        {
            HandleSuspicion();
        }

        public float GetApproachSliderValue()
        {
            if (hasAwarenessReachedMaxLevel || hasPlayerApproachEnoughToAttack)
                return 0;
            
            return approachSlider.value;
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
                TriggerSnakeAttack();
            }
            
            else if (player.transform.position.IsCloseEnough(snake.transform.position, 1))
            {
                hasPlayerApproachEnoughToAttack = true;
                TriggerFoxieAttack();
            }
        }

        void TriggerSnakeAttack()
        {
            approachSlider.interactable = false;
            OnEventToTrigger?.Invoke(EventToTrigger.HandleSnakeAttackFoxie);
        }
        
        void TriggerFoxieAttack()
        {
            approachSlider.interactable = false;
            OnEventToTrigger?.Invoke(EventToTrigger.HandleFoxieAttackSnake);
            End();
        }

        public void Reset()
        {
            suspicionLevel = 0;
            awarenessLevel = 0;
            approachSlider.value = 0;
            hasAwarenessReachedMaxLevel = false;
            approachSlider.interactable = true;
            attackSnakeHandler.UpdateSuspicionUI(suspicionLevel);
            attackSnakeHandler.UpdateAwarenessUI(awarenessLevel);   
        }

        void End()
        {
            attackSnakeHandler.HideCanvas();
            enabled = false;
        }
    }
}