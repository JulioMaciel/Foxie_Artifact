using UnityEngine;

public class AttackSnakeCamera : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject snake;
    [SerializeField] Vector3 offset = new(0, 2, -2.66f);

    void LateUpdate()
    {
        var playerPos = player.transform.position;
        var snakePos = snake.transform.position;
        var middlePos = Vector3.Lerp(playerPos, snakePos, .5f);

        transform.position = playerPos + offset;
        transform.LookAt(middlePos);
    }
        
}