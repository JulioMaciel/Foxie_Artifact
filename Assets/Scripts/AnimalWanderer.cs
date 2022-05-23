using UnityEngine;
using UnityEngine.AI;

public class AnimalWanderer : MonoBehaviour
{
    [SerializeField] float wanderRadius = 5;
    [SerializeField] float wanderTimer = 10;

    Transform target;
    NavMeshAgent agent;
    float timer;
 
    void Awake () 
    {
        agent = GetComponent<NavMeshAgent> ();
        timer = wanderTimer - Random.Range(0, wanderTimer);
    }
 
    void Update () 
    {
        timer += Time.deltaTime;
        if (timer < wanderTimer) return;
        
        var newPos = RandomNavSphere(transform.position, wanderRadius, -1);
        agent.SetDestination(newPos);
        timer = 0;
    }
 
    static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask) 
    {
        var randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMesh.SamplePosition (randDirection, out var navHit, dist, layerMask);
        return navHit.position;
    }
}
