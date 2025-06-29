using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiMeleeState : AiState
{
    float meleePositionSpeed = 5f;
    public AiStateId GetId()
    {
        return AiStateId.Melee;
    }

    public void Enter(AiAgent agent)
    {
        Debug.LogWarning("Name:" + agent.transform.name + "Entered MeleeState");
        agent.MeleeScream();
        agent.AnimatorRootOn();
        agent.FireOff();
        agent.aiWeaponScript.isReloading = false;
        agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("Reloading"), 0);
        agent.animator.SetBool("Reloading2", false);
        agent.animator.SetBool("Reloading1", false);
        if (!agent.meleeing)
        {
            agent.MeleeTrigger();
        }
    }
    public void Update(AiAgent agent)
    {
        if (agent.meleePerson == null)
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
        }
        else if (agent.meleePersonAgent != null && agent.meleePersonAgent.deathState.IsDead())
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
        }
        else
        {
            Vector3 enemyDirection = (agent.meleePerson.position - agent.transform.position).normalized;
            if (enemyDirection.magnitude > 1.5f)
            {
                agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
            }
            else
            {
                agent.transform.position = Vector3.Lerp(agent.transform.position, agent.meleePosition, Time.deltaTime * meleePositionSpeed);
                if (enemyDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(enemyDirection);
                    agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, Time.deltaTime * meleePositionSpeed);
                }
            }
        }
    }

    public void Exit(AiAgent agent)
    {
        agent.MeleeFinished();
        agent.AnimatorRootOff();
    }
}
