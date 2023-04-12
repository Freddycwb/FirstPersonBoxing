using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyInput : MonoBehaviour, IInput
{
    private NavMeshAgent agent;
    private Vector3 target;
    private List<Vector3> pointsToCheck = new List<Vector3>();
    
    public List<Transform> patrolPoints = new List<Transform>();

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        agent.SetDestination(target);
        Patrol();
    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, target) <= 0.5f)
        {
            pointsToCheck.Remove(target);
        }
        if (pointsToCheck.Count == 0)
        {
            foreach (var point in patrolPoints)
            {
                pointsToCheck.Add(point.position);
            }
        }
        target = GetClosestPatrolPoint();
    }

    private Vector3 GetClosestPatrolPoint()
    {
        Vector3 currentPoint = pointsToCheck[0];
        foreach (var point in pointsToCheck)
        {
            if (Vector3.Distance(transform.position, currentPoint) > Vector3.Distance(transform.position, point))
            {
                currentPoint = point;
            }
        }
        return currentPoint;
    }

    public Vector3 direction
    {
        get
        {
            Vector3 diff = agent.steeringTarget - transform.position;
            Vector3 heading = (diff - Vector3.up * diff.y).normalized;
            return heading;
        }
    }

    public bool attack1
    {
        get
        {
            return false;
        }
    }

    public bool attack2
    {
        get
        {
            return false;
        }
    }

    public bool defending
    {
        get
        {
            return false;
        }
    }
}
