using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : MonoBehaviour {
    private enum State {
        PATROL,
        FOLLOW
    }

    public float fieldOfViewAngle = 110f;
    public float randomWalkRadius = 10f;
    public Transform[] patrolPoints;

    private SphereCollider col;
    private Animator anim;
    private GameObject player;
    private NavMeshAgent agent;
    private Transform playerPos;
    private State currentState = State.PATROL;
    private int currentPatrolPoint = 0;

    private void Awake () {
        col = GetComponent<SphereCollider> ();
        anim = GetComponent<Animator> ();
        player = GameObject.FindGameObjectWithTag ("Player");
        agent = GetComponent<NavMeshAgent> ();
        agent.isStopped = false;
    }

    private void Update () {
        if (agent.velocity.magnitude > 0f) {
            transform.rotation = Quaternion.LookRotation (agent.velocity.normalized);
        }

        switch (currentState) {
            case State.PATROL:
                if (!agent.pathPending && !agent.hasPath) {
                    agent.SetDestination (patrolPoints[currentPatrolPoint].position);
                    currentPatrolPoint++;

                    if (currentPatrolPoint >= patrolPoints.Length) {
                        currentPatrolPoint = 0;
                    }
                }
                break;
            case State.FOLLOW:
                agent.SetDestination (player.transform.position);
                break;
        }

    }

    private void OnTriggerStay (Collider other) {
        if (other.gameObject == player) {
            currentState = State.PATROL;

            Vector3 direction = other.transform.position - transform.position;
            float angle = Vector3.Angle (direction, transform.forward);

            if (angle < fieldOfViewAngle * 0.5f) {
                RaycastHit hit;

                if (Physics.Raycast (transform.position, direction.normalized, out hit, col.radius)) {
                    if (hit.collider.gameObject == player) {
                        currentState = State.FOLLOW;
                    }
                }
            }
        }
    }

    // helper method to pick random position on navmesh nearby
    // private Vector3 pickRandomPosition () {
    //     Vector3 randomDirection = Random.insideUnitSphere * randomWalkRadius;
    //     randomDirection += gameObject.transform.position;
    //     NavMeshHit hit;
    //     NavMesh.SamplePosition (randomDirection, out hit, randomWalkRadius, 1);
    //     Vector3 finalPosition = hit.position;

    //     NavMeshPath path = new NavMeshPath ();
    //     if (agent.CalculatePath (finalPosition, path)) {
    //         GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
    //         sphere.transform.position = finalPosition;

    //         return finalPosition;
    //     } else {
    //         return pickRandomPosition ();
    //     }
    // }
}