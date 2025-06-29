using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class WeaponSwitching : MonoBehaviour
{
    public bool canSwitchWeapon = true;
    public int selectedWeapon = 0;
    [SerializeField] private PlayerController playerController;
    private float weaponChangeTime = 0f;
    public float weaponChangeTimeLimit = 1f;

    void OnEnable()
    {
        playerController.weaponScript = GetComponentInChildren<WeaponScript>();
        playerController.weaponAnimator = playerController.weaponScript.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canSwitchWeapon)
        {
            return;
        }

        weaponChangeTime += Time.deltaTime;
        if (weaponChangeTime < weaponChangeTimeLimit)
        {
            return;
        }

        int previousSelectedWeapon = selectedWeapon;
        
        if (Input.GetKeyDown(KeyCode.Alpha1) && playerController.weaponScript.canChangeWeapon)
        {
            selectedWeapon = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2 && playerController.weaponScript.canChangeWeapon)
        {
            selectedWeapon = 1;
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            weaponChangeTime = 0f;
            StartCoroutine(SelectWeapon());
        }
    }

    IEnumerator SelectWeapon()
    {
        int i = 0;
        playerController.weaponAnimator.SetTrigger("WeaponOff");
        yield return new WaitForSeconds(1f);
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }

        playerController.weaponScript = GetComponentInChildren<WeaponScript>();
        playerController.weaponAnimator = playerController.weaponScript.GetComponent<Animator>();
    }
}
