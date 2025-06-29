using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitchingTPP : MonoBehaviour
{
    public bool canChangeWeapon = true;
    public int selectedWeapon = 0;
    int lastSelectedWeapon;
    public float fireDelayTime = 0.5f;
    public float weaponChangeTimeLimit = 1f;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private GameObject primaryHolster;
    [SerializeField] private GameObject pistolHoslter;
    private float weaponChangeTime = 0f;
    private Animator animator;
    private MovementScriptTPP movementScriptTPP;

    void Start()
    {
        animator = GetComponent<Animator>();
        movementScriptTPP = GetComponent<MovementScriptTPP>();
    }

    void Update()
    {
        weaponChangeTime += Time.deltaTime;
        if(weaponChangeTime < weaponChangeTimeLimit)
        {
            return;
        }

        int previousSelectedWeapon = selectedWeapon;

        if (Input.GetKeyDown(KeyCode.Alpha1) && canChangeWeapon)
        {
            selectedWeapon = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2 && canChangeWeapon)
        {
            selectedWeapon = 1;
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            canChangeWeapon = false;
            animator.SetTrigger("WeaponSwitch");
            weaponHolder.GetComponentInChildren<Animator>().enabled = false;
            WeaponScriptTpp weaponSript = weaponHolder.GetComponentInChildren<WeaponScriptTpp>();
            if (weaponSript != null)
            {
                weaponSript.canFire = false;
                weaponSript.fullMag.SetActive(false);
            }
        }
    }

    public void ChangeWeapon()
    {
        int i = 0;
        foreach (Transform weapon in weaponHolder)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
                WeaponScriptTpp weaponSript = weaponHolder.GetComponentInChildren<WeaponScriptTpp>();
                if (weaponSript != null)
                {
                    weaponSript.canFire = false;
                    weaponSript.fullMag.SetActive(false);
                }
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
        if (selectedWeapon == 0)
        {
            primaryHolster.SetActive(false);
            pistolHoslter.SetActive(true);
        }
        else if(selectedWeapon == 1)
        {
            primaryHolster.SetActive(true);
            pistolHoslter.SetActive(false);
        }
        else
        {
            primaryHolster.SetActive(true);
            pistolHoslter.SetActive(true);
        }
        canChangeWeapon = true;
        Invoke("CanFireDelay", fireDelayTime);
    }

    public void WeaponHolster(int selectWeapon)
    {
        lastSelectedWeapon = selectedWeapon;
        selectedWeapon = selectWeapon;
        ChangeWeapon();
    }

    public void WeaponEquip()
    {
        selectedWeapon = lastSelectedWeapon;
        ChangeWeapon();
    }

    void CanFireDelay()
    {
        WeaponScriptTpp weaponScript = weaponHolder.GetComponentInChildren<WeaponScriptTpp>();
        if (weaponScript != null) 
        {
            weaponScript.canFire = true;
        }
    }

    public void WeaponHolsterCheck()
    {
        if (weaponHolder.childCount > 0)
        {
            if (weaponHolder.GetChild(0).gameObject.activeSelf)
            {
                selectedWeapon = 0;
                primaryHolster.SetActive(false);
                pistolHoslter.SetActive(true);
            }
            else
            {
                selectedWeapon = 1;
                primaryHolster.SetActive(true);
                pistolHoslter.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("No Weapons!");
        }
    }
}
