using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;
using Cinemachine;

public class WeaponScriptTpp : MonoBehaviour
{
    public enum WeaponOptions
    {
        AR,
        Pistol,
        Shotgun
    }

    public WeaponOptions weaponSelect;
    [HideInInspector] public int weaponType = 0;

    [Header("Bullet Impact")]
    public bool canFire = true;
    public bool isFiring = false;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float fireRate = 15f;
    [SerializeField] private float impactForce = 250f;
    [SerializeField] private LayerMask bulletIgnore;
    [SerializeField] private float bulletSpread = 0.015f;

    private float nextTimeToFire = 0f;

    public GameObject bulletShell;
    public GameObject bulletFired;
    public float bulletRigidForce;
    public float bulletThrowAngle;

    [Header("Ammo")]
    public int maxAmmo = 10;
    public int currentAmmo;

    [Header("Reload")]
    public bool isReloading = false;
    public GameObject thrownMag;
    public GameObject emptyMag;
    public GameObject fullMag;
    public float magRigidForce;
    public float magThrowAngle;

    [Header("Recoil")]
    [SerializeField] private float recoilX = 5f;
    [SerializeField] private float recoilY = 3.5f;
    [SerializeField] private float recoilZ = 5f;
    [SerializeField] private float snappiness = 8f;
    [SerializeField] private float returnSpeed = 4f;

    [Header("Weapon Close")]
    [SerializeField] private float weaponLenght = 0.65f;
    private float percentage;
    public bool weaponClose = false;


    [Header("References")]
    [SerializeField] private PlayerPerspectiveScript playerPerspectiveScript;
    [SerializeField] private Camera mainCam;
    [SerializeField] private RecoilTPP recoilScript;
    [HideInInspector] public Animator animator;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private GameObject muzzlePrefeb;
    [SerializeField] private LineRenderer bulletTrail;
    [SerializeField] private GameObject gunAudio;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private GameObject impactEffectBlood;
    [SerializeField] private GameObject impactEffectWOHole;
    [SerializeField] private GameObject impactEffectMetalWOHole;
    [SerializeField] private GameObject impactEffectMetal;
    [SerializeField] private GameObject currentAmmoText;
    [SerializeField] private MovementScriptTPP movementScript;

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
        if(weaponType == 1)
        {
            movementScript.animator.SetBool("Pistol", true);
            movementScript.animator.SetLayerWeight(2, 1f);
            movementScript.rightHandRigPistol = true;
            currentAmmo = playerPerspectiveScript.secondaryAmmo;
        }
        else
        {
            movementScript.animator.SetBool("Pistol", false);
            movementScript.animator.SetLayerWeight(2, 0f);
            movementScript.rightHandRigPistol = false;
            currentAmmo = playerPerspectiveScript.primaryAmmo;
        }
        animator = GetComponent<Animator>();
        animator.enabled = true;
        movementScript.fullMag = fullMag;
        isReloading = false;
        isFiring = false;
        movementScript.RigWeightOn();
        movementScript.LeftHandRigOn();
        animator.SetBool("Reloading", false);
        animator.SetBool("Reloading2", false);
        movementScript.animator.SetBool("Reloading", false);
        GunMagReset();
    }

    void Update()
    {
        currentAmmoText.GetComponent<TextMeshPro>().text = currentAmmo.ToString();
        if (movementScript.weaponCloseMode)
        {
            WeaponCloseTPP();
            movementScript.weaponClose = weaponClose;
        }

        if (isReloading)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            ReloadTPP();
            return;
        }
        if (Input.GetKey(KeyCode.R) && currentAmmo != maxAmmo)
        {
            ReloadTPP();
            return;
        }

        if (!movementScript.isAiming)
        {
            weaponClose = false;
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && movementScript.isAiming && !weaponClose && canFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            if(currentAmmo == 1)
            {
                    animator.SetBool("isFiringEmpty", true);
            }
            else
            {
                 int animChoice = Random.Range(1, 3);
                 if (animChoice == 1)
                 {
                     animator.SetBool("isFiring", true);

                 }
                 else
                 {
                     animator.SetBool("isFiring2", true);
                 }
            }
            ShootTPP();
        }
        else if (Input.GetButtonUp("Fire1") || !movementScript.isAiming || weaponClose)
        {
            isFiring = false;
        }
    }

    void ReloadTPP()
    {
        isFiring = false;
        animator.SetBool("isFiring", false);
        animator.SetBool("isFiring2", false);
        animator.SetBool("isFiringEmpty", false);
        isReloading = true;
        Debug.Log("Reloading...");
        if (currentAmmo == 0)
        {
            animator.SetBool("Reloading", true);
            movementScript.animator.SetBool("Reloading", true);
        }
        else
        {
            animator.SetBool("Reloading2", true);
            movementScript.animator.SetBool("Reloading", true);
        }
    }

    void ShootTPP()
    {
        isFiring = true;
        currentAmmo--;
        if (weaponType == 1)
        {
            playerPerspectiveScript.secondaryAmmo--;
        }
        else
        {
            playerPerspectiveScript.primaryAmmo--;
        }

        recoilScript.BulletSpread(bulletSpread);
        CinemachineImpulseSource impulseSource = GetComponent<CinemachineImpulseSource>();
        recoilScript.RecoilFireTPP(recoilX, recoilY, recoilZ, returnSpeed, snappiness, impulseSource);

        RaycastHit hit;

        int muzzleRotation = Random.Range(-41, 41);
        GameObject audioEffect = Instantiate(gunAudio, muzzleFlash.transform.position, Quaternion.identity);
        Destroy(audioEffect, 1f);
        GameObject muzzle = Instantiate(muzzlePrefeb, muzzleFlash.transform.position, Quaternion.identity);
        muzzle.transform.rotation = Quaternion.Euler(new Vector3(muzzleRotation, 0, 0));
        Destroy(muzzle, 0.075f);

        Vector3 aimDirection = mainCam.transform.forward + Random.insideUnitSphere * recoilScript.currentBulletSpread;

        if (Physics.Raycast(mainCam.transform.position, aimDirection, out hit, range, ~bulletIgnore))
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

            SpawnBulletTrailTPP(hit.point);
            BulletShellThrow();
        }
    }

    private void SpawnBulletTrailTPP(Vector3 hitPoint)
    {
        GameObject bulletTrailEffect = Instantiate(bulletTrail.gameObject, muzzleFlash.transform.position, Quaternion.identity);

        LineRenderer lineRender = bulletTrailEffect.GetComponent<LineRenderer>();

        lineRender.SetPosition(0, muzzleFlash.transform.position);
        lineRender.SetPosition(1, hitPoint);

        Destroy(bulletTrailEffect, 0.05f);
    }

    void WeaponCloseTPP()
    {
        if(movementScript.lookDistance > weaponLenght)
        {
            weaponClose = false;
            percentage = 0;
        }
        else
        {
            weaponClose = true;
            percentage = (movementScript.lookDistance - weaponLenght) / (0.2f - weaponLenght);
        }
        movementScript.animator.SetLayerWeight(3, percentage);
    }

    public void ReloadBoolOff()
    {
        isReloading = false;
        animator.SetBool("Reloading", false);
        animator.SetBool("Reloading2", false);
        movementScript.animator.SetBool("Reloading", false);
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

    void FireOff()
    {
        animator.SetBool("isFiring", false);
        animator.SetBool("isFiring2", false);
        animator.SetBool("isFiringEmpty", false);
    }

    void BulletShellThrow()
    {
        GameObject thrownBulletSpwaned = Instantiate(bulletShell.gameObject, bulletFired.transform.position, Quaternion.identity); 
        Rigidbody bulletRb = thrownBulletSpwaned.GetComponent<Rigidbody>();
        Vector3 playerFacingDirection = movementScript.transform.forward;
        Vector3 bulletForceDirection = Quaternion.AngleAxis(bulletThrowAngle, Vector3.up) * playerFacingDirection;
        bulletRb.AddForce(bulletForceDirection * bulletRigidForce, ForceMode.Impulse);
        Destroy(thrownBulletSpwaned, 5f);
    }

    void MagThrow()
    {
        currentAmmo = 0;
        emptyMag.gameObject.SetActive(false);
        GameObject thrownMagSpwaned = Instantiate(thrownMag.gameObject, emptyMag.transform.position, Quaternion.identity);
        thrownMagSpwaned.transform.rotation = Quaternion.Euler(37.671f, 27.316f, 59.197f);
        Rigidbody magRb = thrownMagSpwaned.GetComponent<Rigidbody>();
        Vector3 playerFacingDirection = movementScript.transform.forward;
        Vector3 magForceDirection = Quaternion.AngleAxis(magThrowAngle, Vector3.up) * playerFacingDirection;
        magRb.AddForce(magForceDirection * magRigidForce, ForceMode.Impulse);
        Destroy(thrownMagSpwaned, 10f);
    }

    void GunMagReset()
    {
        emptyMag.gameObject.SetActive(true);
        fullMag.gameObject.SetActive(false);
    }
}

