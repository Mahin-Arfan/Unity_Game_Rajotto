using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealthTPP : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 500f;
    public float health;
    public float healthIncreaseRate = 5f;
    public float initialIncreaseTime = 0.5f;
    public float damageEffectDuration = 0.4f;
    private float damageNotTakenTime = 5f;
    private float time = 0f;
    private float increaseTime = 0.5f;
    private bool isDead = false;
    private bool hitSkip = false;

    [Header("Death Effect")]
    private CinemachinePOV pov;
    [SerializeField] private CinemachineVirtualCamera playerCam;
    public Vector2 virticalDeathCamRange;
    private float deathCamSpeed = 5f;
    public float deathCamFOV = 90f;
    private VolumeProfile postProcessing;
    public Image bloodScreen;
    public GameObject crossHair;
    private Animator animator;
    private MovementScriptTPP movementScript;
    private PlayerPerspectiveScript perspectiveScript;

    public void Start()
    {
        pov = playerCam.GetCinemachineComponent<CinemachinePOV>();
        increaseTime = initialIncreaseTime;
        health = maxHealth;
        animator = GetComponent<Animator>();
        movementScript = GetComponent<MovementScriptTPP>();
        postProcessing = FindObjectOfType<Volume>().profile;
        perspectiveScript = movementScript.perspectiveScript;
    }

    public void Update()
    {
        if(movementScript == null) movementScript = GetComponent<MovementScriptTPP>();
        if (perspectiveScript == null) perspectiveScript = movementScript.perspectiveScript;
        damageNotTakenTime += Time.deltaTime;

        if (health > 0 && health < maxHealth && damageNotTakenTime >= 5f)
        {
            time += Time.deltaTime;

            if (time >= increaseTime)
            {
                health += healthIncreaseRate;
                health = Mathf.Min(health, maxHealth);
                time = 0f;
                increaseTime = Mathf.Max(0.1f, increaseTime - 0.025f);
            }
        }


        Vignette vignette;
        if (postProcessing.TryGet(out vignette))
        {
            float percent = 1.0f - (health / maxHealth);
            vignette.intensity.value = percent * 0.5f;
        }

        if (health <= 0f)
        {
            pov.m_VerticalAxis.Value = Mathf.Lerp(pov.m_VerticalAxis.Value, virticalDeathCamRange.y, Time.deltaTime * deathCamSpeed);
            playerCam.m_Lens.FieldOfView = Mathf.Lerp(playerCam.m_Lens.FieldOfView, deathCamFOV, Time.deltaTime * deathCamSpeed);
        }
        UpdateBloodImageAlpha();
    }

    public void TakeDamage(float amount, Vector3 hitSourcePosition)
    {
        if (hitSkip)
        {
            hitSkip = false;
            return;
        }
        health -= amount;
        healthIncreaseRate = 5f;
        increaseTime = initialIncreaseTime;
        int randomHitAnim = Random.Range(0, 3);
        animator.SetInteger("HitAnim", randomHitAnim);
        if(damageNotTakenTime > 0.5f)
            animator.SetTrigger("HitTrigger");
        CinemachineImpulseSource impulseSource = GetComponent<CinemachineImpulseSource>();
        impulseSource.GenerateImpulse(movementScript.playerCamera.forward);
        // Instantly show hit image
        if (perspectiveScript.hitFadeCoroutine != null)
            perspectiveScript.StopCoroutine(perspectiveScript.hitFadeCoroutine);
        perspectiveScript.hitFadeCoroutine = perspectiveScript.StartCoroutine(perspectiveScript.FadeOutHitBloodScreen(damageEffectDuration));
        ShowDirectionalHit(hitSourcePosition);

        if (health <= 0f && !isDead)
        {
            Die();
        }
        damageNotTakenTime = 0f;
        hitSkip = true;
    }

    public void Die()
    {
        health = -1f;
        setRigidBodyState(false);
        setColliderState(true);

        CharacterDeadState deadState = GetComponentInChildren<CharacterDeadState>();
        if (deadState) deadState.Dead();
        //FindObjectOfType<GameOver>().EndGame();
        if (crossHair) crossHair.SetActive(false);

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;
        }
        else
        {
            animator.enabled = false;
        }

        if (movementScript == null)
        {
            movementScript = GetComponent<MovementScriptTPP>();
            movementScript.rig.weight = 0f;
            movementScript.enabled = false;
        }
        else
        {
            movementScript.rig.weight = 0f;
            movementScript.enabled = false;
        }

        WeaponScriptTpp weaponScript = GetComponentInChildren<WeaponScriptTpp>();
        if (weaponScript) weaponScript.enabled = false;

        if (TryGetComponent(out CharacterController characterController))
            characterController.enabled = false;

        if (TryGetComponent(out CameraScriptTPP cameraScript))
            cameraScript.enabled = false;

        if (TryGetComponent(out RecoilTPP recoilScript))
            recoilScript.enabled = false;

        if (pov)
        {
            pov.m_VerticalAxis.m_MinValue = virticalDeathCamRange.x;
            pov.m_VerticalAxis.m_MaxValue = virticalDeathCamRange.y;
        }

        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
        isDead = true;
    }

    void ShowDirectionalHit(Vector3 hitSourcePosition)
    {
        Vector3 toHit = (movementScript.playerCamera.position - hitSourcePosition).normalized;
        // Flatten to horizontal plane
        toHit.y = 0;
        Vector3 playerForward = movementScript.playerCamera.forward;
        playerForward.y = 0;

        // Signed angle on Y axis between forward and hit direction
        float angle = Vector3.SignedAngle(playerForward, toHit, Vector3.up);

        float uiRotationZ = -angle; // Invert angle for UI Z rotation

        // Fade it out and destroy
        perspectiveScript.StartCoroutine(perspectiveScript.HitArrowMarkFadeAndDestroy(uiRotationZ, damageEffectDuration));
    }

    public void UpdateBloodImageAlpha()
    {
        float alpha = 0f;

        if (health <= 60f && health > 20f)
        {
            // Linearly map health from 60–20 to alpha 0–1
            alpha = 1f - ((health - 20f) / 40f);  // 60→0, 20→1
        }
        else if (health <= 20f)
        {
            alpha = 1f;
        }

        Color color = bloodScreen.color;
        color.a = Mathf.Clamp01(alpha);
        bloodScreen.color = color;
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
