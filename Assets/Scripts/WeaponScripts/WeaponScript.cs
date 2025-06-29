using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;

public class WeaponScript : MonoBehaviour
{
    public enum WeaponOptions
    {
        AR,
        Pistol,
        Shotgun
    }

    public WeaponOptions weaponSelect;
    [HideInInspector] public int weaponType = 0;
    private int stabCount = 0;

    [Header("Bullet Impact")]
    public bool canFire = true;
    public bool isFiring = false;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float fireRate = 15f;
    [SerializeField] private float impactForce = 250f;
    [SerializeField] private LayerMask bulletIgnore;
    [SerializeField] private float bulletSpread = 0.05f;

    private float nextTimeToFire = 0f;

    [Header("Ammo")]
    public int maxAmmo = 10;
    public int currentAmmo;

    [Header("Reload")]
    public bool isReloading = false;
    public bool adsOn;

    [Header("Recoil")]
    //Hipfire Recoil
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    //Aimfire Recoil
    [SerializeField] private float aimRecoilX;
    [SerializeField] private float aimRecoilY;
    [SerializeField] private float aimRecoilZ;

    //Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    [SerializeField] private float hipFireRecoilMultiplier = 1f;

    [Header("Weapon Close")]
    public bool weaponCloseActive = true;
    [SerializeField] private GameObject gunRotate;
    public float weaponLenght = 0.65f;
    [SerializeField] private float weaponRotate = 75f;
    [SerializeField] private float weaponPositionx = 0.025f;
    [SerializeField] private float weaponPositiony = 0f;
    [SerializeField] private float weaponPositionz = -0.05f;
    private float percentage;
    [HideInInspector] public bool weaponClose = false;


    [Header("References")]
    public bool canChangeWeapon = false;
    [SerializeField] private PlayerPerspectiveScript playerPerspectiveScript;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private Recoil recoilScript;
    [HideInInspector] public Animator animator;
    public PlayerController playerController;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private GameObject muzzlePrefeb;
    [SerializeField] private LineRenderer bulletTrail;
    [SerializeField] private GameObject gunAudio;
    [SerializeField] private GameObject stabAudio;
    [SerializeField] private GameObject stabSkullAudio;
    [SerializeField] private AudioClip stabSkullHurtAudio;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private GameObject impactEffectBlood;
    [SerializeField] private GameObject impactEffectWOHole;
    [SerializeField] private GameObject impactEffectMetal;
    [SerializeField] private GameObject impactEffectMetalWOHole;
    [SerializeField] private GameObject currentAmmoText;
    [SerializeField] private Transform headBone;
    [SerializeField] private GameObject knife;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void OnEnable()
    {
        switch (weaponSelect)
        {
            case WeaponOptions.AR:
                weaponType = 0;
                break;
            case WeaponOptions.Pistol:
                weaponType = 1;
                break;
            case WeaponOptions.Shotgun:
                weaponType = 2;
                break;
            default:
                weaponType = 0;
                break;
        }
        if (weaponType == 1)
        {
            currentAmmo = playerPerspectiveScript.secondaryAmmo;
        }
        else
        {
            currentAmmo = playerPerspectiveScript.primaryAmmo;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        currentAmmoText.GetComponent<TextMeshPro>().text = currentAmmo.ToString();
        if(gunRotate != null)
        {
            if (weaponCloseActive)
            {
                WeaponClose();
            }
            else
            {
                gunRotate.transform.localRotation = Quaternion.Lerp(gunRotate.transform.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * 5f);
                gunRotate.transform.localPosition = Vector3.Slerp(gunRotate.transform.localPosition, Vector3.zero, Time.deltaTime * 5f);
            }
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Weapon_On") || animator.GetCurrentAnimatorStateInfo(0).IsName("Weapon_Off"))
        {
            canChangeWeapon = false;
            isReloading = false;
        }
        else
        {
            canChangeWeapon = true;
        }


        if (isReloading)
        {
            return;
        }
        
        if (currentAmmo <= 0 || Input.GetKey(KeyCode.R))
        {
            Reload();
            return;
        }

        if (animator.GetBool("adsOn") && canFire)
        {
            if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && !Input.GetKey(KeyCode.LeftShift) && !weaponClose)
            {
                canChangeWeapon = false;
                nextTimeToFire = Time.time + 1f / fireRate;
                if(currentAmmo == 1)
                {
                    animator.SetTrigger("isFiringEmpty");
                }
                else
                {
                    int animChoice = Random.Range(1, 3);
                    if (animChoice == 1)
                    {
                        animator.SetTrigger("isFiring");
                    }
                    else
                    {
                        animator.SetTrigger("isFiring2");
                    }
                }
                Shoot();
            }
        }
        if(Input.GetButtonUp("Fire1") || Input.GetKey(KeyCode.LeftShift) || weaponClose)
        {
            isFiring = false;
            canChangeWeapon = true;
        }
    }

    void Reload()
    {
        isFiring = false;
        isReloading = true;
        int animChoice = Random.Range(1, 3);
        if(animChoice == 1)
        {
            animator.SetBool("Reloading", true);
        }
        else
        {
            animator.SetBool("Reloading2", true);
        }
    }

    void Shoot()
    {
        isFiring = true;
        currentAmmo--;
        if (adsOn == true)
        {
            recoilScript.RecoilFire(aimRecoilX, aimRecoilY, aimRecoilZ, returnSpeed, snappiness);
        }
        else
        {
            recoilScript.RecoilFire(recoilX, recoilY, recoilZ, returnSpeed, snappiness);
        }

        if (weaponType == 1)
        {
            playerPerspectiveScript.secondaryAmmo--;
        }
        else
        {
            playerPerspectiveScript.primaryAmmo--;
        }

        recoilScript.BulletSpread(bulletSpread);

        RaycastHit hit;

        int muzzleRotation = Random.Range(-41, 41);
        GameObject audioEffect = Instantiate(gunAudio, muzzleFlash.transform.position, Quaternion.identity);
        Destroy(audioEffect, 1f);
        GameObject muzzle = Instantiate(muzzlePrefeb, muzzleFlash.transform.position, Quaternion.identity);
        muzzle.transform.rotation = Quaternion.Euler(new Vector3(muzzleRotation, 0, 0));
        Destroy(muzzle, 0.075f);

        Vector3 aimDirection = fpsCam.transform.forward;
        if (Input.GetButton("Fire2"))
        {
            aimDirection = fpsCam.transform.forward;
        }
        else
        {
            aimDirection += Random.insideUnitSphere * recoilScript.currentBulletSpread * hipFireRecoilMultiplier;
        }

        if (Physics.Raycast(fpsCam.transform.position, aimDirection, out hit, range, ~bulletIgnore))
        {
            Debug.Log(hit.transform.name);
            Health target = hit.transform.GetComponent<Health>();
            if (target != null)
            {
                target.TakeDamage(damage);
                hit.rigidbody.AddForce(-hit.normal * impactForce);
                GameObject impactGO = Instantiate(impactEffectBlood, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
            else
            {
                if(hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * impactForce);
                    if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Metal"))
                    {
                        GameObject impactGO = Instantiate(impactEffectMetalWOHole, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impactGO, 2f);
                    }
                    else
                    {
                        GameObject impactGO = Instantiate(impactEffectWOHole, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impactGO, 2f);
                    }
                }
                else
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Metal"))
                    {
                        GameObject impactGO = Instantiate(impactEffectMetal, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impactGO, 10f);
                    }
                    else
                    {
                        GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impactGO, 10f);
                    }
                }
            }

            SpawnBulletTrail(hit.point);
        }
    }

    public void ReloadBoolOff()
    {
        isReloading = false;
        animator.SetBool("Reloading", false);
        animator.SetBool("Reloading2", false);
        currentAmmo = maxAmmo;
        if (weaponType == 1)
        {
            playerPerspectiveScript.secondaryAmmo = maxAmmo;
        }
        else
        {
            playerPerspectiveScript.primaryAmmo = maxAmmo;
        }
    }

    private void SpawnBulletTrail(Vector3 hitPoint)
    {
        GameObject bulletTrailEffect = Instantiate(bulletTrail.gameObject, muzzleFlash.transform.position, Quaternion.identity);

        LineRenderer lineRender = bulletTrailEffect.GetComponent<LineRenderer>();

        lineRender.SetPosition(0, muzzleFlash.transform.position);
        lineRender.SetPosition(1, hitPoint);

        Destroy(bulletTrailEffect, 0.05f);
    }

    void WeaponClose()
    {
        if (playerController.lookDistance > weaponLenght)
        {
            weaponClose = false;
            percentage = 0;
        }
        else
        {
            weaponClose = true;
            percentage = (playerController.lookDistance - weaponLenght) / (0.2f - weaponLenght);
        }
        float angle = percentage * weaponRotate;
        float distancex = percentage * weaponPositionx;
        float distancey = percentage * weaponPositiony;
        float distancez = percentage * weaponPositionz;
        Vector3 weaponPosition = new Vector3(distancex, distancey, distancez);
        gunRotate.transform.localRotation = Quaternion.Lerp(gunRotate.transform.localRotation, Quaternion.Euler(angle, 0f, 0f), Time.deltaTime * 5f);
        gunRotate.transform.localPosition = Vector3.Slerp(gunRotate.transform.localPosition, weaponPosition, Time.deltaTime * 5f);
    }

    public void MeleeStart()
    {
        animator.SetBool("MeleePass", false);
        playerController.meleeing = true;
        playerController.meleeCounterTime = 0f;
        playerController.ControlOff();
        weaponCloseActive = false;
        playerController.AttachCameraToHead(headBone);
        playerController.PlayerOccupied();
        if (knife != null) { knife.SetActive(true); }
        stabCount = 0;
        playerController.gameManager.timeManager.DoSlowMotion();
        playerController.gameManager.ActionButton("F", true);
    }

    public void MeleeFinish()
    {
        playerController.meleeing = false;
        playerController.meleePerson = null;
        playerController.ControlOn();
        playerController.ResetCamera();
        weaponCloseActive = true;
        playerController.PlayerUnoccupied();
        if (knife != null) { knife.SetActive(false); }
        ShieldScript shieldScript = GetComponent<ShieldScript>();
        if (shieldScript != null)
        {
            shieldScript.WeaponActive();
        }
    }

    public void MeleeStab()
    {
        if (knife == null) { return; }
        GameObject impactEffect = Instantiate(impactEffectBlood, knife.transform.position, Quaternion.identity);
        if (stabCount < 2)
        {
            GameObject impactAudio = Instantiate(stabAudio, knife.transform.position, Quaternion.identity);
            stabCount++;
            Destroy(impactAudio, 2f);
        }
        else
        {
            GameObject impactAudio = Instantiate(stabSkullAudio, knife.transform.position, Quaternion.identity);
            Destroy(impactAudio, 2f);
            AudioSource enemyAudioSource = playerController.meleePerson.GetComponent<AudioSource>();
            enemyAudioSource.clip = stabSkullHurtAudio;
            enemyAudioSource.Play();
        }
        Destroy(impactEffect, 2f);
    }
}
