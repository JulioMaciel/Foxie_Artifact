using System;
using UnityEngine;

namespace Controller
{
    public class LimitWall : MonoBehaviour
    {
        public event Action OnPlayerTouches;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
                OnPlayerTouches?.Invoke();
        }
    }
}
