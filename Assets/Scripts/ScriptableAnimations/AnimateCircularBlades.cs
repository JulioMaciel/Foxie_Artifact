using UnityEngine;
using Random = UnityEngine.Random;

namespace ScriptableAnimations
{
    public class AnimateCircularBlades : MonoBehaviour
    {
        [SerializeField] int speed;

        void Start()
        {
            if (speed == 0)
                speed = Random.Range(45, 145);
        }

        void Update()
        {
            transform.RotateAround(transform.position, transform.forward, Time.deltaTime * speed);
        }
    }
}
