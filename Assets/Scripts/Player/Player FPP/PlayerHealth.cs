using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float health;
    public float maxHealth = 500f;
    public float healthIncreaseRate = 5f;
    public float initialIncreaseTime = 0.5f;
    public float damageEffectDuration = 0.4f;
    private float damageNotTakenTime = 5f;
    private float time = 0f;
    private float increaseTime = 0.5f;
    private bool isDead = false;

    [Header("Camera Effect")]
    public float cameraShakeMagnitude = 5f; // degrees
    public Camera playerCam;
    public GameObject cameraCollider;
    private Quaternion cameraOriginalRotation;
    private float elapsed = 0f;
    private AnimationCurve cameraShakeFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
    private bool isShaking = false;

    [Header("References")]
    VolumeProfile postProcessing;
    public Image bloodScreen;
    public GameObject crossHair;
    private PlayerController playerController;
    private PlayerPerspectiveScript perspectiveScript;

    public void Start()
    {
        health = maxHealth;
        increaseTime = initialIncreaseTime;
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("Player Controller Not Found!");
        }
        perspectiveScript = playerController.perspectiveScript;
        postProcessing = FindObjectOfType<Volume>().profile;
    }

    public void Update()
    {
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (perspectiveScript == null) perspectiveScript = playerController.perspectiveScript;
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

        UpdateBloodImageAlpha();
        if (isShaking)
        {
            CameraShake();
        }
    }

    public void TakeDamage(float amount, Vector3 hitSourcePosition)
    {
        health -= amount;
        healthIncreaseRate = 5f;
        damageNotTakenTime = 0f;
        increaseTime = initialIncreaseTime;
        // Instantly show hit image
        if (perspectiveScript.hitFadeCoroutine != null)
            perspectiveScript.StopCoroutine(perspectiveScript.hitFadeCoroutine);
        perspectiveScript.hitFadeCoroutine = perspectiveScript.StartCoroutine(perspectiveScript.FadeOutHitBloodScreen(damageEffectDuration));
        ShowDirectionalHit(hitSourcePosition);

        if (health <= 0f && !isDead)
        {
            Die();
            return;
        }

        if (!isShaking)
        {
            cameraOriginalRotation = playerCam.transform.localRotation;
            elapsed = 0f;
            isShaking = true;
        }
    }

    public void Die()
    {
        health = -1f;
        if (playerController != null)
        {
            playerController.deathState.Dead();
            playerController.enabled = false;
        }
        else
        {
            if (TryGetComponent(out PlayerController playerController))
            {
                playerController.deathState.Dead();
                playerController.enabled = false;
            }
        }
        //FindObjectOfType<GameOver>().EndGame();
        crossHair.SetActive(false);
        
        GetComponent<CharacterController>().enabled = false;
        playerCam.transform.SetParent(null);
        cameraCollider.SetActive(true);
        playerCam.transform.GetComponent<Rigidbody>().isKinematic = false;
        GameObject.Find("WeaponHolder").SetActive(false);
        playerCam.transform.GetComponent<MouseLook>().enabled = false;
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
        isDead = true;
    }

    void CameraShake()
    {
        if (elapsed < damageEffectDuration)
        {
            elapsed += Time.deltaTime;
            float strength = cameraShakeFalloff.Evaluate(elapsed / damageEffectDuration);

            float rotZ = cameraOriginalRotation.eulerAngles.z + Random.Range(-1f, 1f) * cameraShakeMagnitude * strength;

            Vector3 currentEuler = playerCam.transform.localRotation.eulerAngles;

            playerCam.transform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, rotZ);
        }
        else
        {
            Vector3 currentEuler = playerCam.transform.localRotation.eulerAngles;
            playerCam.transform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, 0f);
            isShaking = false;
        }
    }

    void ShowDirectionalHit(Vector3 hitSourcePosition)
    {
        Vector3 toHit = (transform.position - hitSourcePosition).normalized;

        // Flatten to horizontal plane
        toHit.y = 0;
        Vector3 playerForward = transform.forward;
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
}
