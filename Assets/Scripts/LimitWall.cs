using System;
using UnityEngine;

public class LimitWall : MonoBehaviour
{
    public event Action OnPlayerTouches;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            OnPlayerTouches?.Invoke();
    }
}
