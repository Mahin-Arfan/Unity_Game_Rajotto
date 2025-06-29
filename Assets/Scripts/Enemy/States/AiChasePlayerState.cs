using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiChasePlayerState : AiState
{
    float fireTimer = 0f;
    float fireDuration = 0f;
    float pauseDuration = 0f;
    bool isFiring = false;
    float timer = 0.0f;

    public AiStateId GetId()
    {
        return AiStateId.ChasePlayer;
    }

    public void Enter(AiAgent agent)
    {
        Debug.LogWarning("Name:" + agent.transform.name + "Entered ChaseState");
        agent.FireOff();
        agent.navMeshAgent.stoppingDistance = 0f;
    }
    public void Update(AiAgent agent)
    {
        if (!agent.enabled)
        {
            return;
        }

        if (!agent.targeting.HasTarget || agent.targeting.Target.GetComponent<CharacterDeadState>().IsDead())
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
            return;
        }

        Vector3 enemyDirection = agent.targeting.Target.transform.position - agent.transform.position;
        enemyDirection.y = 0f;
        if (enemyDirection.magnitude > agent.config.toChaseDistance)
        {
            agent.stateMachine.ChangeState(AiStateId.GoingCover);
            return;
        }
        if (agent.navMeshAgent.CalculatePath(agent.targeting.TargetPosition, agent.navMeshPath))
        {
            if (agent.navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                if (enemyDirection.magnitude <= agent.config.toMeleeDistance)
                {
                    agent.stateMachine.ChangeState(AiStateId.Melee);
                }
            }
            else
            {
                agent.stateMachine.ChangeState(AiStateId.GoingCover);
                return;
            }
        }

        timer -= Time.deltaTime;
        if (timer < 0.0f && agent.navMeshAgent != null && agent.navMeshAgent.enabled)
        {
            agent.navMeshAgent.destination = agent.targeting.TargetPosition;
            timer = agent.config.maxTime;
        }

        if (agent.aiWeaponScript.isReloading == true)
        {

            agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("Reloading"), 1);
            agent.aiWeaponScript.reloadTime = 2.55f;
            agent.animator.SetBool("Reloading2", true);
            agent.AimAtTargetOff();
        }
        else
        {
            agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("Reloading"), 0);
            agent.animator.SetBool("Reloading2", false);
        }

        if(enemyDirection.magnitude > 2f)
        {
            Vector3 targetDirection = enemyDirection.normalized;
            agent.FaceTarget(targetDirection);
            agent.SetWalkFireMovementState();
            agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("HitMask"), 1);
            HandleFire(agent);
        }
        else
        {
            agent.FireOff();
            agent.AimAtTargetOff();
            agent.animator.SetBool("WalkFire", false);
            agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("HitMask"), 0);
            agent.animator.SetBool("Run", true);
            agent.navMeshAgent.speed = agent.runSpeed;
        }
    }

    private void HandleFire(AiAgent agent)
    {
        fireTimer += Time.deltaTime;
        agent.AimAtTargetOn();
        if (isFiring)
        {
            if (!agent.isFiring)
                agent.FireOn();

            if (fireTimer >= fireDuration)
            {
                isFiring = false;
                fireTimer = 0f;
                pauseDuration = Random.Range(0.5f, 1.5f);
                if (agent.isFiring)
                    agent.FireOff();
            }
        }
        else
        {
            if (fireTimer >= pauseDuration)
            {
                isFiring = true;
                fireTimer = 0f;
                fireDuration = Random.Range(0.5f, 1.5f);
            }
        }
    }

    public void Exit(AiAgent agent)
    {
        agent.FireOff();
        agent.AimAtTargetOff();
        agent.navMeshAgent.speed = agent.runSpeed;
        agent.animator.SetBool("WalkFire", false);
        agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("HitMask"), 0);
    }
}
