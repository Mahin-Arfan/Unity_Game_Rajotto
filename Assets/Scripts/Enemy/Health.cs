using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public Enemy_Damage enemy_Damage;
    [SerializeField] private float health = 50f;

    public void TakeDamage(float amount)
    {
        enemy_Damage.TakeDamage(health, amount);
    }
}
