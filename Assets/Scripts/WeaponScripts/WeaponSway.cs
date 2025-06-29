using System.Collections;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float movementRotation = 50f;
    [SerializeField] private float movementPosition = 0.025f;
    [SerializeField] private float movementSwaySmooth = 5f;
    [SerializeField] private float smooth = 6f;
    [SerializeField] private float swayMultiplier = 2f;
    private Quaternion zRotation;
    private Vector3 zPosition;

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * swayMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * swayMultiplier;
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);

        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if(direction.magnitude >= 0.1f)
        {
            if (horizontalInput > 0f && !Input.GetButton("Fire2"))
            {
                zRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, movementRotation);
            }
            else if(horizontalInput < 0f && !Input.GetButton("Fire2"))
            {
                zRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, -movementRotation);
            }
            else
            {
                zRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0f);
            }
            if(verticalInput > 0f)
            {
                zPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -movementPosition);
            }
            else if(verticalInput < 0f)
            {
                zPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, movementPosition);
            }
            else
            {
                zPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
            }
        }
        else
        {
            zRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0f); 
            zPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
        }
        transform.localPosition = Vector3.Lerp(transform.localPosition, zPosition, movementSwaySmooth * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, zRotation, movementSwaySmooth/10 * Time.deltaTime);
    }
}
