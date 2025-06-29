using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiGoingCoverState : AiState
{
    float fireTimer = 0f;
    float fireDuration = 0f;
    float pauseDuration = 0f;
    float peakingTime = 0f;
    float peakDuration = 0f;
    bool isFiring = false;
    bool isPeaking = false;
    bool reloadChecked = false;
    bool movingActionsCheck = false;
    int walkFireChoice = 0;
    public AiStateId GetId()
    {
        return AiStateId.GoingCover;
    }

    public void Enter(AiAgent agent)
    {
        Debug.LogWarning("Name:" + agent.transform.name + "Entered CoverState");
        agent.aiCoverMovement.enabled = true;
        agent.aiLineOfSightChecker.enabled = true;
        agent.navMeshAgent.stoppingDistance = 0f;
        agent.FireOff();
        agent.animator.SetBool("WalkFire", true);
        agent.navMeshAgent.updateRotation = false;
        fireTimer = 0f;
        fireDuration = 0f;
        pauseDuration = 0f;
        peakingTime = 0f;
        peakDuration = 0f;
        isFiring = false;
        isPeaking = false;
        reloadChecked = false;
    }
    public void Update(AiAgent agent)
    {
        var hasTarget = agent.targeting.HasTarget;
        bool targetIsDead = hasTarget && agent.targeting.Target.GetComponent<CharacterDeadState>().IsDead();


        if (!hasTarget || targetIsDead)
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
            return;
        }

        Vector3 directionToTarget = agent.targeting.Target.transform.position - agent.transform.position;
        directionToTarget.y = 0f;
        /*
        // Switch to Chase if close enough and path is valid
        if (directionToTarget.magnitude <= agent.config.toChaseDistance)
        {
            if (agent.navMeshAgent.CalculatePath(agent.targeting.TargetPosition, agent.navMeshPath) &&
                agent.navMeshPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
                return;
            }
        }*/
        Vector3 targetDirection = directionToTarget.normalized;
        float targetAngle = Vector3.Angle(targetDirection, agent.transform.forward);
        if (agent.moving)
        {
            if (movingActionsCheck)
            {
                agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("HitMask"), 1);
                movingActionsCheck = false;
                walkFireChoice = Random.Range(0, 4);
                if (walkFireChoice == 0)
                {
                    agent.navMeshAgent.updateRotation = true; 
                    agent.navMeshAgent.speed = agent.runSpeed;
                    if(agent.fireCoroutine != null)
                    {
                        agent.StopCoroutine(agent.fireCoroutine);
                        agent.fireCoroutine = null;
                    }
                    agent.FireOff();
                    agent.animator.SetBool("Run", true);
                    agent.animator.SetBool("CoverIdle", false);
                    agent.animator.SetBool("WalkFire", false);
                }
                else agent.navMeshAgent.updateRotation = false;
            }

            if(walkFireChoice != 0)
            {
                agent.FaceTarget(targetDirection);
                agent.SetWalkFireMovementState();
                HandleFire(agent);
            }
        }
        else
        {
            if (!movingActionsCheck)
            {
                if (agent.fireCoroutine != null)
                {
                    agent.StopCoroutine(agent.fireCoroutine);
                    agent.fireCoroutine = null;
                }
                agent.FireOff();
                agent.AimAtTargetOff();
                agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("HitMask"), 0);
                walkFireChoice = 0;
                movingActionsCheck = true;
            }
            agent.animator.SetFloat("MoveX", 0f, 0.2f, Time.deltaTime);
            agent.animator.SetFloat("MoveZ", 0f, 0.2f, Time.deltaTime);
            SetCombatIdleState(agent, targetDirection, targetAngle);
        }

        if (agent.aiWeaponScript.isReloading)
        {
            if(!reloadChecked)
                HandleReloading(agent);
        }
        else
        {
            agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("Reloading"), 0);
            agent.animator.SetBool("Reloading1", false);
            agent.animator.SetBool("Reloading2", false);
            reloadChecked = false;
        }
    }

    private void SetCombatIdleState(AiAgent agent, Vector3 direction, float angle)
    {
        agent.animator.SetBool("WalkFire", false);
        agent.animator.SetBool("CoverIdle", true);
        HandlePeak(agent);
        if (angle > 30f && direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, rotation, Time.deltaTime * 2f);
        }
    }

    private void HandlePeak(AiAgent agent)
    {
        peakingTime += Time.deltaTime;
        if (isPeaking)
        {
            if (!agent.isFiring && agent.fireCoroutine == null && !agent.aiWeaponScript.isReloading)
            {
                int peakChoice = Random.Range(1, 3);
                agent.animator.SetBool("coverFire1", peakChoice == 1);
                agent.animator.SetBool("coverFire2", peakChoice == 2);
                agent.fireCoroutine = agent.StartCoroutine(agent.FireOnDelayed(0.5f));
            }

            if (peakingTime >= peakDuration)
            {
                isPeaking = false;
                peakingTime = 0f;
                pauseDuration = Random.Range(agent.coveringTime_Min_Max.x, agent.coveringTime_Min_Max.y);
                if (agent.isFiring)
                {
                    if (agent.fireCoroutine != null)
                    {
                        agent.StopCoroutine(agent.fireCoroutine);
                        agent.fireCoroutine = null;
                    }
                    agent.FireOff();
                    agent.animator.SetBool("coverFire1", false);
                    agent.animator.SetBool("coverFire2", false);
                }
                if (agent.fireCoroutine != null)
                {
                    agent.StopCoroutine(agent.fireCoroutine);
                    agent.fireCoroutine = null;
                }
            }
        }
        else
        {
            if (peakingTime >= pauseDuration)
            {
                isPeaking = true;
                peakingTime = 0f;
                peakDuration = Random.Range(agent.peakingTime_Min_Max.x, agent.peakingTime_Min_Max.y);
            }
        }
    }

    private void HandleFire(AiAgent agent) 
    {
        fireTimer += Time.deltaTime;
        agent.AimAtTargetOn();
        if (isFiring)
        {
            if(!agent.isFiring && !agent.aiWeaponScript.isReloading)
                agent.FireOn();

            if (fireTimer >= fireDuration)
            {
                isFiring = false;
                fireTimer = 0f;
                pauseDuration = Random.Range(0.5f, 1.5f);
                if(agent.isFiring)
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

    private void HandleReloading(AiAgent agent)
    {
        var anim = agent.animator;
        if (agent.fireCoroutine != null)
        {
            agent.StopCoroutine(agent.fireCoroutine);
            agent.fireCoroutine = null;
        }
        if (agent.isFiring)
        {
            agent.FireOff();
        }
        agent.AimAtTargetOff();
        isPeaking = false;
        anim.SetBool("coverFire1", false);
        anim.SetBool("coverFire2", false);
        anim.SetLayerWeight(anim.GetLayerIndex("Reloading"), 1);
        if (agent.moving)
        {
            agent.aiWeaponScript.reloadTime = 2.55f;
            anim.SetBool("Reloading2", true);
        }
        else
        {
            agent.aiWeaponScript.reloadTime = 6.40f;
            anim.SetBool("Reloading1", true);
        }
        reloadChecked = true;
    }

    public void Exit(AiAgent agent)
    {
        agent.navMeshAgent.speed = agent.runSpeed;
        agent.navMeshAgent.updateRotation = true;
        agent.navMeshAgent.angularSpeed = 720;
        if (agent.fireCoroutine != null)
        {
            agent.StopCoroutine(agent.fireCoroutine);
            agent.fireCoroutine = null;
        }
        agent.AimAtTargetOff();
        agent.FireOff();
        agent.aiCoverMovement.enabled = false;
        agent.aiLineOfSightChecker.enabled = false;
        agent.animator.SetLayerWeight(agent.animator.GetLayerIndex("HitMask"), 0);
        agent.animator.SetBool("WalkFire", false);
        agent.animator.SetBool("CoverIdle", false);
        agent.animator.SetBool("coverFire1", false);
        agent.animator.SetBool("coverFire2", false);
        agent.animator.SetBool("Reloading1", false);
        agent.animator.SetBool("Reloading2", false);
    }
}
