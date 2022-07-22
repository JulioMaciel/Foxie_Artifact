using System;
using UnityEngine;

namespace Tools
{
    public class PlayerColliderAction : MonoBehaviour
    {
        public event Action OnPlayerTouches;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
                OnPlayerTouches?.Invoke();
        }
    }
}
