using StaticData;
using UnityEngine;

namespace Cameras
{
    public class AttackSnakeCamera : MonoBehaviour
    {
        [SerializeField] Vector3 offset = new(0, 2, -2.66f);
        
        GameObject player;
        GameObject snake;

        void Awake()
        {
            player = GameObject.FindWithTag(Tags.Player);
            snake = GameObject.FindWithTag(Tags.Snake);
        }

        void LateUpdate()
        {
            var playerPos = player.transform.position;
            var snakePos = snake.transform.position;
            var middlePos = Vector3.Lerp(playerPos, snakePos, .5f);

            transform.position = playerPos + offset;
            transform.LookAt(middlePos);
        }
        
    }
}