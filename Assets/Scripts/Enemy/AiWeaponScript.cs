using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiWeaponScript : MonoBehaviour
{
    public bool fire = false;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public float impactForce = 250f;

    public int maxAmmo = 10;
    private int currentAmmo;
    public float reloadTime = 2.55f;
    public bool isReloading = false;
    public float bulletSpread = 0.1f;

    private float nextTimeToFire = 0f;

    public GameObject enemyWeapon;
    public GameObject muzzleLight;
    public LineRenderer bulletTrail;
    public GameObject gunAudio;
    public GameObject impactEffect;
    public GameObject impactEffectBlood;
    public GameObject impactEffectWOHole;
    public GameObject impactEffectMetalWOHole;
    public GameObject impactEffectMetal;
    public AiAgent aiAgent;

    public void Start()
    {
        currentAmmo = maxAmmo;
        muzzleLight.SetActive(false);

    }

    public void OnEnable()
    {
        isReloading = false;
    }


    // Update is called once per frame
    public void Update()
    {
        if (isReloading)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            muzzleLight.SetActive(false);
            return;
        }


        if (fire && Time.time >= nextTimeToFire)
        {

            nextTimeToFire = Time.time + 1f / fireRate;
            muzzleLight.SetActive(true);

            Shoot();
        }
        else
        {
            muzzleLight.SetActive(false);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime - .025f);
        yield return new WaitForSeconds(.25f);

        currentAmmo = maxAmmo;
        isReloading = false;
    }

    public void Shoot()
    {
        currentAmmo--;

        RaycastHit hit;
        GameObject audioEffect = Instantiate(gunAudio, muzzleLight.transform.position, Quaternion.identity);
        Destroy(audioEffect, 1f);

        Vector3 aimDirection = muzzleLight.transform.forward;
        aimDirection += Random.insideUnitSphere * bulletSpread;

        if (Physics.Raycast(muzzleLight.transform.position, aimDirection, out hit, range, ~aiAgent.friendlyFire))
        {
            PlayerHealth target = hit.transform.GetComponent<PlayerHealth>();
            HealthTPP playerTarget = hit.transform.GetComponent<HealthTPP>();
            Health enemyTarget = hit.transform.GetComponent<Health>();
            if (target != null)
            {
                target.TakeDamage(damage / 10, transform.position);
            }
            else if (playerTarget != null)
            {
                playerTarget.TakeDamage(damage / 10, transform.position);
                GameObject impactGO = Instantiate(impactEffectBlood, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
            else if (enemyTarget != null)
            {
                enemyTarget.TakeDamage(damage);
                GameObject impactGO = Instantiate(impactEffectBlood, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }
            else if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Metal"))
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
            //Debug.LogWarning("Hit: " + hit.collider.gameObject.name);
            SpawnBulletTrail(hit.point);
        }
    }

    private void SpawnBulletTrail(Vector3 hitPoint)
    {
        GameObject bulletTrailEffect = Instantiate(bulletTrail.gameObject, muzzleLight.transform.position, Quaternion.identity);

        LineRenderer lineRender = bulletTrailEffect.GetComponent<LineRenderer>();

        lineRender.SetPosition(0, muzzleLight.transform.position);
        lineRender.SetPosition(1, hitPoint);

        Destroy(bulletTrailEffect, 0.05f);
    }
}
