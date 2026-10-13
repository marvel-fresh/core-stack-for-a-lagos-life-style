using UnityEngine;
using UnityEngine.AI;

namespace LagosLifestyle.AI
{
    /// <summary>
    /// NavMesh-based Pedestrian AI for bustling Lagos street crowds and markets.
    /// Handles sidewalk wandering, obstacle avoidance, idle pauses, and market stall stops.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class PedestrianAI : MonoBehaviour
    {
        [Header("Wander Settings")]
        [SerializeField] private float wanderRadius = 35f;
        [SerializeField] private float minWaitTime = 2f;
        [SerializeField] private float maxWaitTime = 6f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private NavMeshAgent agent;
        private float waitTimer = 0f;
        private bool isWaiting = false;

        private readonly int hashSpeed = Animator.StringToHash("Speed");

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            SetRandomDestination();
        }

        private void Update()
        {
            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    SetRandomDestination();
                }
            }
            else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isWaiting = true;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }

            if (animator != null)
            {
                float normalizedSpeed = agent.velocity.magnitude / agent.speed;
                animator.SetFloat(hashSpeed, normalizedSpeed, 0.15f, Time.deltaTime);
            }
        }

        private void SetRandomDestination()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
}
