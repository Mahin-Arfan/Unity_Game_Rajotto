using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeaponSight : MonoBehaviour
{
    [SerializeField] private Vector3 aimDownSight;
    [SerializeField] private Vector3 hipSight;
    [SerializeField] private float aimSightSpeed;
    [SerializeField] private float hipSightSpeed;

    [SerializeField] private Camera zoomCamera;
    [SerializeField] private float adsZoom;
    private float normalCameraFov = 60f;
    private GameObject crossHair;
    [SerializeField] private GameObject holoDot;
    [SerializeField] private Renderer scopeTransparency;
    private Color tempColor;
    private WeaponScript weaponScript;
    VolumeProfile postProcessing;

    // Start is called before the first frame update
    void Start()
    {
        crossHair = GameObject.Find("CrossHair");
        weaponScript = GetComponent<WeaponScript>();
        postProcessing = FindObjectOfType<Volume>().profile;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetButton("Fire2") && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift) && !weaponScript.isReloading && !weaponScript.weaponClose && !weaponScript.playerController.meleeing)
        {
            if (crossHair != null) { crossHair.SetActive(false); }
            if(holoDot!= null)
            {
                holoDot.SetActive(true);
            }
            transform.localPosition = Vector3.Slerp(transform.localPosition, aimDownSight, aimSightSpeed * Time.deltaTime);
            zoomCamera.fieldOfView = Mathf.Lerp(zoomCamera.fieldOfView, adsZoom, aimSightSpeed * Time.deltaTime);
            DepthOfField depthOfField;
            if (postProcessing.TryGet(out depthOfField))
            {
                depthOfField.focalLength.value = 90f;
            }
            weaponScript.adsOn = true;
            if(scopeTransparency != null)
            {
                tempColor.a = .1f;
                scopeTransparency.material.color = tempColor;
            }

        } else
        {
            if (crossHair != null) { crossHair.SetActive(true); }
            if (holoDot != null)
            {
                holoDot.SetActive(false);
            }
            transform.localPosition = Vector3.Slerp(transform.localPosition, hipSight, hipSightSpeed * Time.deltaTime);
            zoomCamera.fieldOfView = Mathf.Lerp(zoomCamera.fieldOfView, normalCameraFov , aimSightSpeed * Time.deltaTime);
            DepthOfField depthOfField;
            if (postProcessing.TryGet(out depthOfField))
            {
                depthOfField.focalLength.value = 0f;
            }
            weaponScript.adsOn = false;
            if (scopeTransparency != null)
            {
                tempColor.a = .5f;
                scopeTransparency.material.color = tempColor;
            }
        }

        if (weaponScript.weaponClose)
        {
            crossHair.SetActive(false);
        }
    }
}
