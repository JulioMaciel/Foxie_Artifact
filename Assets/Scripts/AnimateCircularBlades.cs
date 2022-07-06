using UnityEngine;
using Random = UnityEngine.Random;

public class AnimateCircularBlades : MonoBehaviour
{
    [SerializeField] int Speed;

    void Start()
    {
        if (Speed == 0)
            Speed = Random.Range(45, 145);
    }

    void Update()
    {
        transform.RotateAround(transform.position, transform.forward, Time.deltaTime * Speed);
    }
}
