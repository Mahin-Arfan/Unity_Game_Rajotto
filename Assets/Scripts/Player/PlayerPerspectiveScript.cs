using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPerspectiveScript : MonoBehaviour
{
    [Header("Perspective Settings")]
    public bool canChangePerspective = true;
    public bool FPPEnabled = false;
    public float switchCooldown = 1.0f;
    private float lastSwitchTime = 0f;
    private Vector3 playerPosition;
    private Quaternion playerRotation;
    public float fppHeightOffset = 1.1f;    // FPP height adjustment
    public float cameraTransitionSpeed = 2.0f;
    public float cameraDistance = 1.4f;
    public float cameraHorizontalOffset = 1f;
    public bool changingPerspective = false;
    bool switchingToFPP = false;

    [Header("Info Store")]
    public float health;
    public int primaryAmmo;
    public int secondaryAmmo;
    public int weaponEquipped = 0;
    public bool reloading = false;

    [Header("References")]
    public GameObject TPP;
    public GameObject FPP;
    public Transform ghost;
    public Transform weaponHolderFPP;
    public Transform weaponHolderTPP;
    public CinemachineVirtualCamera camera;
    public Image hitBloodScreen;
    public GameObject hitBloodMarker;
    [HideInInspector] public Coroutine hitFadeCoroutine;
    private CinemachineFramingTransposer transposer;
    private CinemachinePOV pov;

    void OnEnable()
    {
        transposer = camera.GetCinemachineComponent<CinemachineFramingTransposer>();
        pov = camera.GetCinemachineComponent<CinemachinePOV>();
    }

    void Start()
    {
        // Get initial player position based on the active perspective
        if (TPP.activeSelf)
        {
            FPPEnabled = false;
            playerPosition = ghost.position;
            playerRotation = ghost.rotation;
            health = ghost.GetComponent<PlayerHealthTPP>().health;
            primaryAmmo = ghost.GetComponentInChildren<WeaponScriptTpp>().maxAmmo;
            weaponEquipped = ghost.GetComponent<WeaponSwitchingTPP>().selectedWeapon;
        }
        else
        {
            FPPEnabled = true;
            playerPosition = FPP.transform.position;
            playerPosition.y -= fppHeightOffset;
            playerRotation = FPP.transform.rotation;
            health = FPP.transform.GetComponent<PlayerHealth>().health;
            primaryAmmo = FPP.transform.GetComponentInChildren<WeaponScript>().maxAmmo;
            weaponEquipped = weaponHolderFPP.GetComponent<WeaponSwitching>().selectedWeapon;
        }
    }

    void Update()
    {
        if (changingPerspective) 
        {
            CameraDistance();
        }

        // Check if 'TAB' is pressed and cooldown has passed
        if (Input.GetKeyDown(KeyCode.Tab) && Time.time >= lastSwitchTime + switchCooldown && canChangePerspective)
        {
            changingPerspective = true;

            lastSwitchTime = Time.time;

            if (TPP.activeSelf)
            {
                switchingToFPP = true;
                cameraDistance = 0f;
            }
            else
            {
                switchingToFPP = false;
                cameraDistance = 1.4f;
            }
        }

        if (changingPerspective)
        {
            if (switchingToFPP)
            {
                if (transposer.m_CameraDistance <= 0.05f)
                {
                    changingPerspective = false;
                    playerPosition = ghost.position;
                    playerRotation = ghost.rotation;
                    SetFPP(true);
                }
            }
            else
            {
                playerPosition = FPP.transform.position;
                playerPosition.y -= fppHeightOffset;
                playerRotation = FPP.transform.rotation;
                SetFPP(false);
                if (transposer.m_CameraDistance >= 1.35f)
                {
                    changingPerspective = false;
                    transposer.m_CameraDistance = 1.4f;
                }
            }
        }
    }

    void SetFPP(bool enableFPP)
    {
        if (enableFPP)
        {
            //Gather Info
            WeaponScriptTpp weaponScriptTpp = ghost.GetComponentInChildren<WeaponScriptTpp>();
            health = ghost.GetComponent<PlayerHealthTPP>().health;
            reloading = weaponScriptTpp.isReloading;
            weaponEquipped = ghost.GetComponent<WeaponSwitchingTPP>().selectedWeapon;

            // Switch to FPP
            FPP.SetActive(true);
            TPP.SetActive(false);
            FPPEnabled = true;

            //WeaponSelect
            int i = 0;
            foreach (Transform weapon in weaponHolderFPP)
            {
                if (i == weaponEquipped)
                {
                    weapon.gameObject.SetActive(true);
                }
                else
                {
                    weapon.gameObject.SetActive(false);
                }
                i++;
            }

            //Appling Info
            weaponHolderFPP.GetComponent<WeaponSwitching>().selectedWeapon = weaponEquipped;
            WeaponScript weaponScriptFpp = FPP.transform.GetComponentInChildren<WeaponScript>();
            FPP.transform.GetComponent<CharacterController>().enabled = false;
            FPP.transform.position = new Vector3(playerPosition.x, playerPosition.y + fppHeightOffset, playerPosition.z);
            FPP.transform.rotation = playerRotation;
            FPP.transform.GetComponent<CharacterController>().enabled = true;
            FPP.transform.GetComponent<PlayerHealth>().health = health;
            FPP.transform.GetComponent<PlayerController>().GetWeaponScript();
            if (reloading)
            {
                weaponScriptFpp.isReloading = true;
                weaponScriptFpp.animator.Play("Reload2 2");
            }
            else
            {
                weaponScriptFpp.isReloading = false;
                weaponScriptFpp.animator.SetBool("Reloading", false);
                weaponScriptFpp.animator.SetBool("Reloading2", false);
            }
        }
        else
        {
            //Gather Info
            WeaponScript weaponScriptFpp = FPP.transform.GetComponentInChildren<WeaponScript>();
            health = FPP.transform.GetComponent<PlayerHealth>().health;
            reloading = weaponScriptFpp.isReloading;
            weaponEquipped = weaponHolderFPP.GetComponent<WeaponSwitching>().selectedWeapon;

            // Switch to TPP
            TPP.SetActive(true);
            FPP.SetActive(false);
            FPPEnabled = false;

            //WeaponSelect
            int i = 0;
            foreach (Transform weapon in weaponHolderTPP)
            {
                if (i == weaponEquipped)
                {
                    weapon.gameObject.SetActive(true);
                }
                else
                {
                    weapon.gameObject.SetActive(false);
                }
                i++;
            }

            //Appling Info
            ghost.GetComponent<WeaponSwitchingTPP>().selectedWeapon = weaponEquipped;
            WeaponScriptTpp weaponScriptTpp = ghost.GetComponentInChildren<WeaponScriptTpp>();
            weaponScriptTpp.canFire = true;
            ghost.GetComponent<CharacterController>().enabled = false;
            ghost.position = playerPosition;
            ghost.rotation = playerRotation;
            ghost.GetComponent<CharacterController>().enabled = true;
            ghost.GetComponent<PlayerHealthTPP>().health = health;
            ghost.GetComponent<WeaponSwitchingTPP>().WeaponHolsterCheck();

            if (reloading) 
            {
                weaponScriptTpp.isReloading = true;
                weaponScriptTpp.animator.SetBool("Reloading", true);
                weaponScriptTpp.animator.SetBool("Reloading2", true);
                ghost.GetComponent<Animator>().Play("Player_Reload_M4 2");
                weaponScriptTpp.animator.Play("Reload 2");
            }
            else
            {
                weaponScriptTpp.isReloading = false;
                weaponScriptTpp.animator.SetBool("Reloading", false);
                weaponScriptTpp.animator.SetBool("Reloading2", false);
                ghost.GetComponent<MovementScriptTPP>().animator.SetBool("Reloading", false);
            }
        }
    }

    void CameraDistance()
    {
        Vector3 forward = playerRotation * Vector3.forward;
        float targetHorizontal = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        float targetVertical = Mathf.Asin(forward.y) * Mathf.Rad2Deg;
        transposer.m_CameraDistance = Mathf.Lerp(transposer.m_CameraDistance, cameraDistance, Time.deltaTime * cameraTransitionSpeed);
        pov.m_HorizontalAxis.Value = Mathf.Lerp(pov.m_HorizontalAxis.Value, targetHorizontal + cameraHorizontalOffset, Time.deltaTime * cameraTransitionSpeed);
        pov.m_VerticalAxis.Value = Mathf.Lerp(pov.m_VerticalAxis.Value, targetVertical, Time.deltaTime * cameraTransitionSpeed);
    }

    public IEnumerator FadeOutHitBloodScreen(float duration)
    {
        float timer = 0f;
        Color color = hitBloodScreen.color;
        color.a = 1f;
        hitBloodScreen.color = color;

        while (timer < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            color.a = alpha;
            hitBloodScreen.color = color;

            timer += Time.deltaTime;
            yield return null;
        }

        color.a = 0f;
        hitBloodScreen.color = color;
    }

    public IEnumerator HitArrowMarkFadeAndDestroy(float uiRotationZ, float duration)
    {
        // Instantiate marker
        GameObject hitMark = Instantiate(hitBloodMarker, hitBloodScreen.transform.parent.transform);
        hitMark.transform.localRotation = Quaternion.Euler(0f, 0f, uiRotationZ);
        Image img = hitMark.GetComponent<Image>();
        float time = 0f;
        Color c = img.color;
        c.a = 1f;
        img.color = c;

        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, time / duration);
            img.color = c;
            yield return null;
        }

        Destroy(img.gameObject);
    }
}
