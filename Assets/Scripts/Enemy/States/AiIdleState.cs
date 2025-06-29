using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiIdleState : AiState
{
    public AiStateId GetId()
    {
        return AiStateId.Idle;
    }

    public void Enter(AiAgent agent)
    {
        Debug.LogWarning("Name:" + agent.transform.name + "Entered IdleState");
        agent.navMeshAgent.stoppingDistance = Random.Range(5, 11);
        agent.animator.SetBool("CoverIdle", false);
        agent.navMeshAgent.speed = 3.5f;
        agent.FireOff();
        /*
        if (agent.player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                agent.player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player Not Found!");
            }
        }*/
    }
    public void Update(AiAgent agent)
    {
        if (agent.moving)
        {
            agent.animator.SetBool("Run", true);
            agent.navMeshAgent.speed = agent.runSpeed;
        }
        else
        {
            agent.animator.SetBool("Run", false);
        }
        /*
        if(agent.player != null)
        {
            agent.navMeshAgent.destination = agent.player.position;
        }*/

        if (agent.targeting.HasTarget)
        {
            float RandomChoice = Random.Range(0, 10);
            if (RandomChoice <= 8)
            {
                agent.stateMachine.ChangeState(AiStateId.GoingCover);
            }
            else if (RandomChoice > 8)
            {
                agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
            }
        }
    }
    public void Exit(AiAgent agent)
    {
        agent.animator.SetBool("Run", false);
        agent.navMeshAgent.stoppingDistance = 0f;
    }
}
