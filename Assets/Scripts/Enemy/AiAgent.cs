using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiAgent : MonoBehaviour
{
    [Header("Agent Settings")]
    public AiStateId initialState;
    public bool moving = true;
    public float walkFireSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float weaponAimSpeed = 5f;
    [HideInInspector] public bool isFiring = false;
    [HideInInspector] public Coroutine fireCoroutine;
    public bool weaponAim = false;

    [Header("Cover Settings")]
    public Vector2Int coveringTime_Min_Max = new Vector2Int(1, 6);
    public Vector2Int peakingTime_Min_Max = new Vector2Int(1, 3);

    [Header("Melee Settings")]
    public float mForwardOffset = 0.42f;
    public float mRightOffset = 0.15f;
    [HideInInspector] public bool meleeing = false;
    [HideInInspector] public Transform meleePerson;
    [HideInInspector] public AiAgent meleePersonAgent;
    private int stabCount = 0;
    private int meleeAttempt = 0;
    [HideInInspector] public Vector3 meleePosition;
    private bool meleeingPlayer = false;
    private bool meleeingPlayerFpp = false;

    [Header("References")]
    public GameObject knife;
    public GameObject stabAudio;
    public GameObject stabSkullAudio;
    public GameObject meleeAttemptSound;
    public GameObject meleeFailSound1;
    public GameObject meleeFailSound2;
    public AudioClip stabSkullHurtAudio;
    public AudioClip screamAudio;
    public AiStateMachine stateMachine;
    public AiAgentConfig config;
    public LayerMask friendlyFire;
    public Transform weaponHolster;
    private Vector3 gunEquipPosition = new Vector3(9.9e-05f, 0.001364f, 0.00024f);
    private Vector3 gunEquipRotation = new Vector3(-95.876f, 52.211f, 40.83701f);
    [HideInInspector] public AiLineOfSightChecker aiLineOfSightChecker;
    [HideInInspector] public AiWeaponScript aiWeaponScript;
    //[HideInInspector] public Transform player;
    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public AiSensor sensor;
    [HideInInspector] public Animator animator;
    [HideInInspector] public WeaponIk weaponIk;
    [HideInInspector] public AiTargetingSystem targeting;
    [HideInInspector] public Enemy_Damage enemyDamage;
    [HideInInspector] public AiCoverMovement aiCoverMovement;
    [HideInInspector] public CharacterDeadState deathState;
    [HideInInspector] public GameObject gameScripts;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public NavMeshPath navMeshPath;
    [HideInInspector] public AudioSource audioSource;
    public Transform rightHand;

    // Start is called before the first frame update
    void Start()
    {
        gameScripts = GameObject.FindGameObjectWithTag("GameController");
        aiCoverMovement = GetComponent<AiCoverMovement>();
        aiLineOfSightChecker = GetComponentInChildren<AiLineOfSightChecker>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        aiWeaponScript = GetComponentInChildren<AiWeaponScript>();
        weaponIk = GetComponent<WeaponIk>();
        enemyDamage = GetComponent<Enemy_Damage>();
        sensor = GetComponent<AiSensor>();
        targeting = GetComponent<AiTargetingSystem>();
        audioSource = GetComponent<AudioSource>();
        stateMachine = new AiStateMachine(this);
        stateMachine.RegisterState(new AiChasePlayerState());
        stateMachine.RegisterState(new AiIdleState());
        stateMachine.RegisterState(new AiMeleeState());
        stateMachine.RegisterState(new AiGoingCoverState());
        stateMachine.RegisterState(new AiPhaseState());
        stateMachine.ChangeState(initialState);
        navMeshPath = new NavMeshPath(); 

        rb = GetComponent<Rigidbody>();
        deathState = GetComponentInChildren<CharacterDeadState>();
        rightHand = aiWeaponScript.transform.parent.parent;
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
        MovementCheck();

        if (meleeing)
        {
            animator.SetBool("Meleeing", true);
        }
        else
        {
            animator.SetBool("Meleeing", false);
        }

        if (weaponAim)
        {
            if (weaponIk.weight < 1) 
                weaponIk.weight = Mathf.Lerp(weaponIk.weight, 1f, weaponAimSpeed * Time.deltaTime);
        }
        else
        {
            if(weaponIk.weight > 0f)
                weaponIk.weight = Mathf.Lerp(weaponIk.weight, 0f, weaponAimSpeed * Time.deltaTime);
        }

        if (enemyDamage.takingHit == true)
        {
            animator.SetLayerWeight(animator.GetLayerIndex("Reloading"), 0);
            animator.SetBool("Reloading2", false);
            animator.SetBool("Reloading1", false);
            int hitAnimChoice = Random.Range(1, 4);
            if (hitAnimChoice == 1)
            {
                animator.SetBool("Hit", true);
            }
            else if(hitAnimChoice == 2)
            {
                animator.SetBool("Hit1", true);
            }
            else
            {
                animator.SetBool("Hit2", true);
            }
        }
        else
        {
            animator.SetBool("Hit", false);
            animator.SetBool("Hit1", false);
            animator.SetBool("Hit2", false);
        }
    }

    public void FaceTarget(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // 5 is smooth speed
        }
    }

    public void SetWalkFireMovementState()
    {
        navMeshAgent.speed = walkFireSpeed;
        animator.SetBool("WalkFire", true);
        animator.SetBool("CoverIdle", false);
        animator.SetBool("coverFire1", false);
        animator.SetBool("coverFire2", false);


        if (!navMeshAgent.enabled || !navMeshAgent.hasPath)
        {
            animator.SetFloat("MoveX", 0f, 0.2f, Time.deltaTime);
            animator.SetFloat("MoveZ", 0f, 0.2f, Time.deltaTime);
            return;
        }

        Vector3 worldVelocity = navMeshAgent.desiredVelocity;
        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity.normalized);

        animator.SetFloat("MoveX", localVelocity.x, 0.2f, Time.deltaTime);
        animator.SetFloat("MoveZ", localVelocity.z, 0.2f, Time.deltaTime);
    }

    void MovementCheck()
    {
        if(navMeshAgent != null)
        {
            if (navMeshAgent.velocity.sqrMagnitude > 0.5f && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                moving = true;
            }
            else
            {
                moving = false;
            }
        }
        else
        {
            Debug.LogWarning("NavMeshAgent not found!");
        }
    }

    public void FireOn()
    {
        if (!deathState.isDead)
        {
            isFiring = true;
            aiWeaponScript.fire = true;
            AimAtTargetOn();
        }
    }
    public void FireOff()
    {
        isFiring = false;
        aiWeaponScript.fire = false;
        aiWeaponScript.muzzleLight.SetActive(false);
        AimAtTargetOff();
    }

    public IEnumerator FireOnDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        FireOn();

        fireCoroutine = null;
    }

    public void AimAtTargetOn()
    {
        if(!aiWeaponScript.isReloading)
            weaponAim = true;
    }

    public void AimAtTargetOff()
    {
        if(!isFiring) 
            weaponAim = false;
    }

    public void AnimatorRootOn()
    {
        animator.applyRootMotion = true;
        navMeshAgent.enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        //GetComponent<CharacterController>().enabled = false;
        weaponAim = false;
        weaponIk.weight = 0f;
    }
    public void AnimatorRootOff()
    {
        animator.applyRootMotion = false;
        navMeshAgent.enabled = true;
        GetComponent<BoxCollider>().enabled = true;
        //GetComponent<CharacterController>().enabled = true;
    }

    public void MeleeTrigger()
    {
        meleeing = true;
        stabCount = 0;
        meleeAttempt = 0;

        if (targeting.Target.transform.GetComponent<MouseLook>() != null)
        {
            meleeingPlayer = true;
            MeleeAgentToPlayerFPP();
        }
        else if(targeting.Target.transform.GetComponent<HealthTPP>() != null)
        {
            meleeingPlayer = true;
            MeleeAgentToPlayerTPP();
        }
        else
        {
            meleeingPlayer = false;
            MeleeAgentToAgent();
        }
    }

    public void MeleeAgentToPlayerFPP()
    {
        meleeingPlayerFpp = true;
        meleePerson = targeting.Target.transform.GetComponent<MouseLook>().playerBody;
        PlayerController playerController = meleePerson.GetComponent<PlayerController>();
        playerController.meleePerson = this.transform;
        animator.SetBool("MeleePass", false);
        playerController.weaponAnimator.SetTrigger("Melee");
        animator.SetTrigger("Melee");
        deathState.occupied = true;
        Vector3 enemyDirection = (transform.position - meleePerson.position).normalized;
        Quaternion enemyRotation = Quaternion.LookRotation(enemyDirection);
        meleePosition = meleePerson.position + enemyRotation * Vector3.forward * mForwardOffset - enemyRotation * Vector3.right * mRightOffset;
        playerController.meleePosition = meleePerson.position;
    }

    public void MeleeAgentToPlayerTPP()
    {
        meleeingPlayerFpp = false;
        meleePerson = targeting.Target.transform.GetComponent<HealthTPP>().playerHealth.transform;
        MeleeScriptTPP playerMeleeScript = meleePerson.GetComponent<MeleeScriptTPP>();
        playerMeleeScript.meleePerson = this.transform;
        animator.SetBool("MeleePass", false);
        playerMeleeScript.animator.SetTrigger("Melee");
        animator.SetTrigger("Melee");
        deathState.occupied = true;
        Vector3 enemyDirection = (transform.position - meleePerson.position).normalized;
        Quaternion enemyRotation = Quaternion.LookRotation(enemyDirection);
        meleePosition = meleePerson.position + enemyRotation * Vector3.forward * config.meleeActionDistance;
        playerMeleeScript.meleePosition = meleePerson.position;
    }

    public void MeleeAgentToAgent()
    {
        meleePerson = targeting.Target.transform.GetComponent<Health>().enemy_Damage.transform;
        meleePersonAgent = meleePerson.GetComponent<AiAgent>();
        meleePersonAgent.deathState.occupied = true;
        meleePersonAgent.meleePerson = this.transform;
        meleePersonAgent.meleePersonAgent = this;
        meleePersonAgent.meleeing = true;
        meleePersonAgent.stateMachine.ChangeState(AiStateId.Melee);
        animator.SetBool("MeleePass", true);
        meleePerson.GetComponent<Animator>().SetTrigger("Melee");
        meleePerson.GetComponent<Animator>().SetBool("MeleePass", true);
        animator.SetTrigger("Melee");
        deathState.occupied = true;
        meleePosition = transform.position;
        Vector3 enemyDirection = (meleePerson.position - transform.position).normalized;
        meleePersonAgent.meleePosition = transform.position + enemyDirection * config.meleeActionDistance;
    }

    public void MeleeFinished()
    {
        deathState.occupied = false;
        meleeing = false;
        meleePerson = null;
        meleePersonAgent = null;
        GunEquip();
    }

    public void MeleeStab()
    {
        GameObject impactBlood = aiWeaponScript.impactEffectBlood;
        GameObject impactEffect = Instantiate(impactBlood, rightHand.position, Quaternion.identity);
        if(stabCount < 2)
        {
            GameObject impactAudio = Instantiate(stabAudio, rightHand.position, Quaternion.identity);
            stabCount++;
            Destroy(impactAudio, 2f);
        }
        else
        {
            GameObject impactAudio = Instantiate(stabSkullAudio, rightHand.position, Quaternion.identity);
            Destroy(impactAudio, 2f);
            AudioSource enemyAudioSource = meleePerson.GetComponent<AudioSource>();
            enemyAudioSource.clip = stabSkullHurtAudio;
            enemyAudioSource.Play();
        }
        Destroy(impactEffect, 2f);
    }

    public void MeleeScream()
    {
        if (!audioSource.isPlaying && screamAudio != null)
        {
            audioSource.clip = screamAudio;
            audioSource.Play();
        }
    }

    public void MeleeStabHurt()
    {
        enemyDamage.mainHealth -= 30f;
        if(enemyDamage.mainHealth <= 0f)
        {
            enemyDamage.mainHealth = 1f;
        }
        enemyDamage.takingHit = true;
    }

    public void MeleeAttempts()
    {
        if (!meleeingPlayer) { return; }
        if (meleeAttempt == 0)
        {
            GameObject meleeEffect = Instantiate(meleeAttemptSound, enemyDamage.gunBone.transform.position, Quaternion.identity);
            Destroy(meleeEffect, 1f);
            meleeAttempt++;
        }
        else if(meleeAttempt == 1)
        {
            GameObject meleeEffect = Instantiate(meleeFailSound1, enemyDamage.gunBone.transform.position, Quaternion.identity);
            Destroy(meleeEffect, 1f);
            if (meleeingPlayerFpp)
            {
                meleePerson.GetComponent<PlayerHealth>().health = 11f;
            }
            else
            {
                meleePerson.GetComponent<PlayerHealthTPP>().health = 11f;
            }
            meleeAttempt++;
        }
        else
        {
            GameObject meleeEffect = Instantiate(meleeFailSound2, enemyDamage.gunBone.transform.position, Quaternion.identity);
            Destroy(meleeEffect, 1f);
            if (meleeingPlayerFpp)
            {
                meleePerson.GetComponent<PlayerHealth>().Die();
            }
            else
            {
                meleePerson.GetComponent<PlayerHealthTPP>().Die();
            }
            meleeAttempt = 0;
        }
    }

    public void WeaponHolster()
    {
        if (enemyDamage != null && enemyDamage.gunBone != null && weaponHolster != null && knife != null)
        {
            enemyDamage.gunBone.transform.SetParent(weaponHolster);
            enemyDamage.gunBone.transform.localPosition = Vector3.zero;
            enemyDamage.gunBone.transform.localRotation = Quaternion.identity;
            knife.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Error while holstering Weapon");
        }
    }

    public void GunEquip()
    {
        if (enemyDamage != null && enemyDamage.gunBone != null && weaponHolster != null && knife != null)
        {
            enemyDamage.gunBone.transform.SetParent(rightHand);
            enemyDamage.gunBone.transform.localPosition = gunEquipPosition;
            enemyDamage.gunBone.transform.localRotation = Quaternion.Euler(gunEquipRotation);
            knife.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Error while equiping Weapon");
        }
    }
}
