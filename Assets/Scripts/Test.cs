using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public bool shake = false;
    // Update is called once per frame
    void Update()
    {
        if (shake)
        {
            TestDamage();
            shake = false;
        }
    }

    public void TestDamage()
    {
        playerHealth.TakeDamage(0f, transform.position);

    }
}
