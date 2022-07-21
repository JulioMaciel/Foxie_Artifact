using StaticData;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]

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
            animator.SetFloat(AnimParam.MoveSpeed, hasAlmostArrived ? 0 : 0.9f);
        }

        void Move()
        {
            var newPos = RandomNavSphere(transform.position, wanderRadius, -1);
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
    }
}
