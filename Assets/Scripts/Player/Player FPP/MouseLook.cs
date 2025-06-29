using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public bool MouseLookMode = true;

    public float mouseSensitivity = 100f;

    public Transform playerBody;

    float xRotation = 0f;
    float yRotation = 0f;

    public float clampXRotaion = 90f;
    public bool clampYEnable = false;
    [SerializeField] private float clampYRotaion;
    [SerializeField] Transform cameraFollower;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        if (MouseLookMode)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -clampXRotaion, clampXRotaion);

            if (clampYEnable)
            {
                yRotation += mouseX;
                yRotation = Mathf.Clamp(yRotation, -clampYRotaion, clampYRotaion);
                transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
                cameraFollower.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            }
            else
            {
                playerBody.Rotate(Vector3.up * mouseX);
                transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                cameraFollower.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
        }
    }
}

