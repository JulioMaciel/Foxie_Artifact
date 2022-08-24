using System;
using Managers;
using StaticData;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BiteHandler : MonoBehaviour
    {
        [SerializeField] GameObject targetInside;
        [SerializeField] AudioClip foxAttackClip;
        [SerializeField] bool isDriver;
        
        const float Speed = 1.5f;

        Camera mainCamera;
        GameObject player;
        Image img;

        bool hasPlayerAttacked;
        
        public event Action<EventToTrigger> OnEventToTrigger;

        void OnEnable()
        {
            mainCamera = Camera.main;
            player = Entity.Instance.player;
            targetInside.SetActive(true);
            img = targetInside.GetComponentInChildren<Image>();
        }

        void Update()
        {
            targetInside.transform.LookAt(mainCamera.transform);

            if (Input.GetMouseButton(0))
            {
                var rayToClick = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(rayToClick, out var hit, Layers.Interactable)) return;

                if (hit.transform.gameObject == img.gameObject)
                {
                    //img.gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
                    img.fillAmount -= Time.deltaTime * Speed;
                    
                    if (!hasPlayerAttacked)
                    {
                        player.transform.LookAt(transform.position);
                        player.GetComponent<Animator>().SetTrigger(AnimParam.Attack);
                        player.GetComponent<AudioSource>().PlayClip(foxAttackClip);
                        hasPlayerAttacked = true;
                    }
                }
            }
            // else if (Input.GetMouseButtonUp(0)) 
            //     img.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            if (img.fillAmount <= 0)
            {
                OnEventToTrigger?.Invoke(isDriver
                    ? EventToTrigger.ReactDriverToBite
                    : EventToTrigger.ReactPassengerToBite);
                
                enabled = false;
                Destroy(targetInside);
            }
        }
    }
}
