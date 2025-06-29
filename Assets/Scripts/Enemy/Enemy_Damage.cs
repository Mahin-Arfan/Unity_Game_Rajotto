using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Damage : MonoBehaviour
{
    public bool canDie = true;
    public float mainHealth = 100f;
    public GameObject gunBone;
    public AudioClip[] hurtSounds;
    [HideInInspector] public AiAgent aiAgent;
    private AudioSource audioSource;
    private int previousHurtSound = -1;
    float lastHitTime = 0f;

    [HideInInspector] public bool takingHit = false;

    void Start()
    {
        setRigidBodyState(true);
        aiAgent = GetComponent<AiAgent>();
        audioSource = GetComponent<AudioSource>();
        if (aiAgent.aiWeaponScript != null)
        {
            gunBone = aiAgent.aiWeaponScript.enemyWeapon;
        }
    }

    void Update()
    {
        lastHitTime += Time.deltaTime;
        if(lastHitTime > 0.5f && takingHit)
        {
            takingHit = false;
        }
    }

    public void TakeDamage(float health, float damageAmount)
    {
        takingHit = true;
        if (canDie && mainHealth > 0)
        {
            mainHealth -= health * (damageAmount / 100);
            if (mainHealth <= 0)
            {
                Die();
            }
        }

        if (!audioSource.isPlaying && hurtSounds.Length > 0)
        {
            int randomSelectSound;
            do
            {
                randomSelectSound = Random.Range(0, hurtSounds.Length);
            }
            while (randomSelectSound == previousHurtSound && hurtSounds.Length > 1);

            previousHurtSound = randomSelectSound;
            audioSource.clip = hurtSounds[randomSelectSound];
            audioSource.Play();
        }
        lastHitTime = 0f;
    }

    public void Die()
    {
        setRigidBodyState(false);
        setColliderState(true);

        mainHealth = 0;
        GetComponentInChildren<CharacterDeadState>().Dead();
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Animator>().enabled = false;
        aiAgent.enabled = false;
        GetComponent<CharacterController>().enabled = false;
        GetComponent<WeaponIk>().enabled = false;
        GetComponent<AiSensor>().enabled = false;
        GetComponent<AiTargetingSystem>().enabled = false;
        GetComponent<AiCoverMovement>().enabled = false;
        GetComponentInChildren<AiLineOfSightChecker>().enabled = false;
        aiAgent.aiWeaponScript.muzzleLight.SetActive(false);
        aiAgent.aiWeaponScript.enabled = false;


        gunBone.transform.SetParent(null);
        BoxCollider boxCollider = gunBone.GetComponentInChildren<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
        //Destroy(gameObject, 60f);
    }

    void setRigidBodyState(bool state)
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = state;
        }
    }

    void setColliderState(bool state)
    {
        Collider[] colliders = GetComponentsInChildren<BoxCollider>();

        foreach (Collider collider in colliders)
        {
            collider.enabled = state;
        }
    }
}
