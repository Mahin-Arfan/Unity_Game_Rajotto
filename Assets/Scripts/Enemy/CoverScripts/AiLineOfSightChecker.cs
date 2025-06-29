using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AiLineOfSightChecker : MonoBehaviour
{
    public AiAgent agent;
    public SphereCollider Collider;
    public float FieldOfView = 90f;
    public LayerMask LineOfSightLayers;

    public delegate void GainSightEvent(Transform Target);
    public GainSightEvent OnGainSight;
    public delegate void LoseSightEvent(Transform Target);
    public LoseSightEvent OnLoseSight;

    private Coroutine CheckForLineOfSightCoroutine;

    private void Awake()
    {
        Collider = GetComponent<SphereCollider>();
    }
    public void Update(){}

    private void OnTriggerEnter(Collider other)
    {
        if (!CheckLineOfSight(other.transform))
        {
            CheckForLineOfSightCoroutine = StartCoroutine(CheckForLineOfSight(other.transform));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        OnLoseSight?.Invoke(other.transform);
        if (CheckForLineOfSightCoroutine != null)
        {
            StopCoroutine(CheckForLineOfSightCoroutine);
        }
    }

    private bool CheckLineOfSight(Transform Target)
    {
        if(Target != null)
        {
            Vector3 direction = (Target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, direction);

            if (dotProduct >= Mathf.Cos(FieldOfView))
            {
                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, Collider.radius, LineOfSightLayers))
                {
                    if (agent.targeting.HasTarget)
                    {
                        GameObject target = agent.targeting.Target;
                        if (hit.transform == target.transform)
                        {

                            OnGainSight?.Invoke(Target);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    private IEnumerator CheckForLineOfSight(Transform Target)
    {
        WaitForSeconds Wait = new WaitForSeconds(0.5f);

        while (!CheckLineOfSight(Target))
        {
            yield return Wait;
        }
    }
}
