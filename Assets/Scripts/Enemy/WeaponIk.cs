using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponIk : MonoBehaviour
{
    public AiAgent agent;

    public Transform targetTransform;
    public Transform aimTransform;
    public Vector3 targetOffset;
    public Transform bone;
    public float targetAngle;
    public float angleLimit = 90f;
    public float distanceLimit = 1.5f;

    public int interations = 1;

    [Range(0, 1)]
    public float weight = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<AiAgent>();
        
    }
    void Update()
    {
        if(agent != null)
        {
            if (agent.targeting.HasTarget)
            {
                targetTransform = agent.targeting.Target.transform;
            }
        }
        if(weight > 0.98f)
        {
            weight = 1f;
        }
        else if (weight < 0.02f) 
        { 
            weight = 0f;
        }
    }

    Vector3 GetTargetPosition()
    {
        Vector3 targetDirection = (targetTransform.position + targetOffset) - aimTransform.position;
        Vector3 aimDirection = aimTransform.forward;
        float blendOut = 0.0f;

        targetAngle = Vector3.Angle(targetDirection, aimDirection);
        if (targetAngle > angleLimit)
        {
            blendOut += (targetAngle - angleLimit) / 50.0f;
        }

        float targetDistance = targetDirection.magnitude;
        if(targetDistance < distanceLimit)
        {
            blendOut += distanceLimit - targetDistance;
        }

        Vector3 direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
        return aimTransform.position + direction;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(aimTransform == null)
        {
            Debug.LogWarning("Aim Point not assigned!");
            return;
        }
        if (targetTransform == null)
        {
            return;
        }

        Vector3 targetPosition = GetTargetPosition();
        for (int i = 0; i < interations; i++)
        {
            AimAtTarget(bone, targetPosition, weight);
        }
    }

    private void AimAtTarget(Transform bone, Vector3 targetPosition, float weight)
    {
        Vector3 aimDirection = aimTransform.forward;
        Vector3 targetDirection = targetPosition - aimTransform.position;
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        bone.rotation = blendedRotation * bone.rotation;
    }

    public void SetTargetTransform(Transform target)
    {
        targetTransform = target;
    }
    public void SetAimTransform(Transform aim)
    {
        aimTransform = aim;
    }
}
