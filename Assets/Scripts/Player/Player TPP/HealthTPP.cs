using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthTPP : MonoBehaviour
{
    public PlayerHealthTPP playerHealth;

    public void TakeDamage(float amount, Vector3 hitSourcePosition)
    {
        playerHealth.TakeDamage(amount, hitSourcePosition);
    }
}
