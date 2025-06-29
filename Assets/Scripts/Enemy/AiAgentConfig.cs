using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AiAgentConfig : ScriptableObject
{
    public float maxTime = 1f; //for Scan Frequency
    public float maxDistance = 1f; //For Scan Distance
    public float maxSightDistance = 20f;
    public float toChaseDistance = 3f;
    public float toMeleeDistance = 1.5f;
    public float meleeActionDistance = 0.8f;
}
