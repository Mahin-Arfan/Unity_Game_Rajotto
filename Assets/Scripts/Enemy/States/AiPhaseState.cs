using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPhaseState : AiState
{
    private float waypointDistance;

    public AiStateId GetId()
    {
        return AiStateId.Phase;
    }

    public void Enter(AiAgent agent)
    {
        Debug.LogWarning("Name:" + agent.transform.name + "Entered PhaseState");
        agent.FireOff();
        agent.navMeshAgent.stoppingDistance = 0; 
        agent.navMeshAgent.speed = 2.75f;
        if(agent.transform.GetComponent<Phase_1>() != null)
        {
            if (!agent.transform.GetComponent<Phase_1>().manualTarget)
            {
                agent.animator.SetBool("PhaseMode", true);
            }
        }
    }
    public void Update(AiAgent agent)
    {
        if(agent.transform.GetComponent<Phase_1>() != null)
        {
            waypointDistance = Vector3.Distance(agent.transform.position, agent.transform.GetComponent<Phase_1>().waypoint.transform.position);

            if (waypointDistance < 0.5f)
            {
                agent.transform.GetComponent<Phase_1>().inPosition = true;
            }
            else
            {
                agent.transform.GetComponent<Phase_1>().inPosition = false;
            }

            if (agent.transform.GetComponent<Phase_1>().inPosition == false && agent.navMeshAgent.enabled)
            {
                agent.navMeshAgent.destination = agent.transform.GetComponent<Phase_1>().waypoint.transform.position;
            }
        }

        if (agent.moving)
        {
            agent.animator.SetBool("WalkFire", true);
        }
        else
        {
            agent.animator.SetBool("WalkFire", false);
        }
    }

    public void Exit(AiAgent agent)
    {
        agent.animator.SetBool("WalkFire", false);
        agent.animator.SetBool("PhaseMode", false);
        agent.navMeshAgent.speed = 3.5f;
    }
}