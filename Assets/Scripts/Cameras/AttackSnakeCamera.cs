using Managers;
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
            player = Entity.Instance.player;
            snake = Entity.Instance.snake;
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