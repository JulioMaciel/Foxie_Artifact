using UnityEngine;

public class WaypointHandler : MonoBehaviour
{
    [SerializeField] Camera freeCamera;
    
    Transform target;

    void Update()
    {
        var direction = (freeCamera.transform.position - target.transform.position).normalized;
        transform.rotation = Quaternion.LookRotation (direction);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}