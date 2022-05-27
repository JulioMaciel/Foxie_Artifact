using UnityEngine;
using UnityEngine.AI;

public class AnimalWanderer : MonoBehaviour
{
    [SerializeField] float wanderRadius = 5;
    [SerializeField] public float wanderTimer = 10;

    Animator animator;
    Transform target;
    NavMeshAgent agent;

    public float Timer { get; private set; }

    void Awake () 
    {
        agent = GetComponent<NavMeshAgent> ();
        animator = gameObject.GetComponent<Animator>();
        
        Timer = wanderTimer - Random.Range(0, wanderTimer);
    }
 
    void Update () 
    {
        SyncAnimation();

        Timer += Time.deltaTime;
        if (Timer < wanderTimer) return;

        Move();
    }

    void SyncAnimation()
    {
        if (!agent.hasPath) return;
        
        var hasAlmostArrived = agent.remainingDistance < 0.1;
        animator.SetFloat(Anim.MoveSpeed, hasAlmostArrived ? 0 : 0.9f);
    }

    void Move()
    {
        var newPos = RandomNavSphere(transform.position, wanderRadius, -1);
        FaceDestination(newPos);
        agent.SetDestination(newPos);
        Timer = 0;
    }

    static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask) 
    {
        var randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMesh.SamplePosition (randDirection, out var navHit, dist, layerMask);
        return navHit.position;
    }
    
    void FaceDestination(Vector3 destination)
    {
        var lookPos = destination - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10);  
    }
}
