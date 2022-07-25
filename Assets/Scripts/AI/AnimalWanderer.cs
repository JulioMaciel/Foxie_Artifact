using StaticData;
using Tools;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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
        NavMeshAgent navMesh;

        public float Timer { get; private set; }

        void Awake () 
        {
            navMesh = GetComponent<NavMeshAgent> ();
            animator = gameObject.GetComponent<Animator>();
        }

        void Start()
        {
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
            if (!navMesh.hasPath) return;
            
            animator.SetFloat(AnimParam.MoveSpeed, navMesh.HasPracticallyArrived() ? 0 : 0.9f);
        }

        void Move()
        {
            var newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            navMesh.SetDestination(newPos);
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
