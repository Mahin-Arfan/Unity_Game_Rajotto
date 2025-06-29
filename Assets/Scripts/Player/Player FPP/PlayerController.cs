using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("States")]
    public bool canMove = true;
    public bool canLook = true;
    public bool canChangePerspective = true;
    public bool meleeing = false;
    public bool isGrounded { get; private set; } = false;
    public bool isRunning { get; private set; } = false;
    public bool isJumping { get; private set; } = false;
    public bool isCrouching { get; private set; } = false;
    public bool isGoingForward { get; private set; } = false;
    private Vector3 jumpVelocity = Vector3.zero;

    [Header("Leaning")]
    public bool leanEnabled = true;
    public Transform leanPivot;
    private float currentLean;
    private float targetLean;
    public float leanAngle;
    public float leanSmoothing;
    private float leanVelocity;

    [Header("Looking At")]
    [SerializeField] private LayerMask lookIgnore;
    public float lookDistance;

    [Header("Melee")]
    public float meleePositionSpeed = 5f;
    [HideInInspector] public Transform meleePerson;
    [HideInInspector] public Vector3 meleePosition;
    [HideInInspector] public float meleeCounterTime = 0f;

    [Header("Animations")]
    [HideInInspector] public CharacterController characterController;
    private PlayerStats playerStats;
    public Animator weaponAnimator;
    public Animator gunHolderAnimator;

    [Header("References")]
    public Camera playerCamera;
    private MouseLook mouselook;
    [SerializeField] Transform feetTouchCheck;
    [SerializeField] LayerMask groundMask;
    public WeaponScript weaponScript;
    [HideInInspector] public WeaponSwitching weaponSwitching;
    [HideInInspector] public WeaponSway weaponSway;
    [HideInInspector] public Transform cameraFollower;
    [HideInInspector] public PlayerPerspectiveScript perspectiveScript;
    [HideInInspector] public CharacterDeadState deathState;
    [HideInInspector] public Recoil recoilScript;
    [HideInInspector] public GameManagerScript gameManager;

    void OnEnable()
    {
        if(playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (mouselook == null) mouselook = playerCamera.transform.GetComponent<MouseLook>();
        if (recoilScript == null) recoilScript = playerCamera.transform.parent.GetComponent<Recoil>();
        if (weaponSwitching == null) weaponSwitching = GetComponentInChildren<WeaponSwitching>();
        if (deathState == null) deathState = GetComponentInChildren<CharacterDeadState>();
        if (gameManager == null) gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager == null)
        {
            Debug.LogError("GameManagerScript Not Found!");
        }
        if (weaponSwitching != null)
        {
            if (weaponSway == null) weaponSway = weaponSwitching.transform.GetComponent<WeaponSway>();
            if(cameraFollower == null) cameraFollower = weaponSwitching.transform.parent.parent;
        }
        
        if(perspectiveScript == null)
        {
            if (transform.parent != null)
            {
                perspectiveScript = transform.parent.GetComponent<PlayerPerspectiveScript>();
            }
            else
            {
                Debug.LogError("Error Object: " + transform.name + "Parent is null. playerPerspectiveScript not assigned.");
            }
        }
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(feetTouchCheck.position, 0.4f, groundMask);

        LookingAt();
        if (canMove)
        {
            HandleMovement();
            HandleJumpInput();
        }
        if (leanEnabled)
        {
            CalculateLeaning();
        }
        else
        {
            Vector3 currentEuler = leanPivot.localRotation.eulerAngles;
            currentEuler.x = 0f;
            currentEuler.z = 0f;
            leanPivot.localRotation = Quaternion.Euler(currentEuler);
        }

        if (meleeing)
        {
            weaponAnimator.SetBool("Meleeing", true);
            meleeCounterTime += Time.deltaTime;
            UpdateMeleePosition();
            if (Input.GetKeyDown(KeyCode.F) && !weaponAnimator.GetBool("MeleePass") && meleeCounterTime <= 0.5f)
            {
                weaponAnimator.SetBool("MeleePass", true);
                meleePerson.GetComponent<Animator>().SetBool("MeleePass", true);
                gameManager.timeManager.ResetTime();
                gameManager.ActionButtonOff();
            }else if(meleeCounterTime > 0.5f)
            {
                gameManager.timeManager.ResetTime();
                gameManager.ActionButtonOff();
            }
            if (meleePerson == null || meleePerson.GetComponent<AiAgent>().deathState.IsDead() || !meleePerson.GetComponent<AiAgent>().meleeing)
            {
                weaponScript.MeleeFinish();
                weaponAnimator.Play("Idle");
            }
        }
        else
        {
            weaponAnimator.SetBool("Meleeing", false);
            gameManager.timeManager.ResetTime();
            gameManager.ActionButtonOff();
        }

        if (canLook)
        {
            mouselook.MouseLookMode = true;
        }
        else
        {
            mouselook.MouseLookMode = false;
        }

        if (canChangePerspective)
        {
            perspectiveScript.canChangePerspective = true;
        }
        else
        {
            perspectiveScript.canChangePerspective = false;
        }
    }

    void LookingAt()
    {
        RaycastHit hit;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 10, ~lookIgnore))
        {
            lookDistance = hit.distance;
        }
        else
        {
            lookDistance = 10;
        }
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isJumping = true;
            jumpVelocity.y = Mathf.Sqrt(playerStats.jumpHeight * -2f * playerStats.gravity);
        }

        if (isGrounded && jumpVelocity.y < 0)
        {
            jumpVelocity.y = -2f;
            isJumping = false;   
        }
        else
        {
            jumpVelocity.y += playerStats.gravity * Time.deltaTime;
        }

        characterController.Move(jumpVelocity * Time.deltaTime);
    }

    void HandleMovement()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);
        isCrouching = Input.GetKey(KeyCode.LeftControl);
        isGoingForward = Input.GetKey(KeyCode.W);

        if (isCrouching)
        {
            HandleCrouch();
        }
        else
        {
            HandleStand();
        }

        Vector3 movementVector = Vector3.ClampMagnitude(transform.right * horizontalInput + transform.forward * verticalInput, 1.0f);

        if (isCrouching)
        {
            characterController.Move(movementVector * playerStats.crouchingMovementSpeed * Time.deltaTime);
            weaponAnimator.SetBool("isSprinting", false);
        }
        else if (isRunning && isGoingForward && !weaponScript.isFiring)
        {
            characterController.Move(movementVector * playerStats.runningMovementSpeed * Time.deltaTime);
            gunHolderAnimator.SetBool("isWalking", false);
            weaponAnimator.SetBool("isSprinting", true);
        }
        else
        {
            characterController.Move(movementVector * playerStats.walkingMovementSpeed * Time.deltaTime);
            weaponAnimator.SetBool("isSprinting", false);
        }



        if (movementVector != Vector3.zero && !isRunning)
        {
            gunHolderAnimator.SetBool("isWalking", true);
        }
        else
        {
            gunHolderAnimator.SetBool("isWalking", false);
        }
    }

    void HandleCrouch()
    {
        if (characterController.height > playerStats.crouchHeightY)
        {
            UpdateCharacterHeight(playerStats.crouchHeightY);

            if (characterController.height - 0.05f <= playerStats.crouchHeightY)
            {
                characterController.height = playerStats.crouchHeightY;
            }
        }
    }

    void HandleStand()
    {
        if (characterController.height < playerStats.standingHeightY)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.up, out hit, playerStats.standingHeightY, ~lookIgnore))
            {
                Debug.Log("Cant Stand because of: "+ hit.transform.name);
            }
            else
            {
                UpdateCharacterHeight(playerStats.standingHeightY);
            }

            if (characterController.height + 0.05f >= playerStats.standingHeightY)
            {
                characterController.height = playerStats.standingHeightY;
            }
        }
    }

    void UpdateCharacterHeight(float newHeight)
    {
        characterController.height = Mathf.Lerp(characterController.height, newHeight, playerStats.playerHeightSpeed * Time.deltaTime);
    }

    void CalculateLeaning()
    {
        currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSmoothing);

        Vector3 currentEuler = leanPivot.localRotation.eulerAngles;
        currentEuler.x = 0f;
        currentEuler.z = currentLean;
        leanPivot.localRotation = Quaternion.Euler(currentEuler);

        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        float maxLeanDistance = 0.7f;

        targetLean = 0f;

        RaycastHit hit;

        // Lean Right
        if (Input.GetKey(KeyCode.E) && !isRunning)
        {
            if (Physics.Raycast(rayOrigin, transform.right, out hit, maxLeanDistance, ~lookIgnore))
            {
                float factor = hit.distance / maxLeanDistance;
                targetLean = -leanAngle * factor;
            }
            else
            {
                targetLean = -leanAngle;
            }
        }
        // Lean Left
        else if (Input.GetKey(KeyCode.Q) && !isRunning)
        {
            if (Physics.Raycast(rayOrigin, -transform.right, out hit, maxLeanDistance, ~lookIgnore))
            {
                float factor = hit.distance / maxLeanDistance;
                targetLean = leanAngle * factor;
            }
            else
            {
                targetLean = leanAngle;
            }
        }
    }

    public void PlayerOccupied()
    {
        deathState.occupied = true;
    }

    public void PlayerUnoccupied()
    {
        deathState.occupied = false;
    }

    public void ControlOn()
    {
        canMove = true;
        canLook = true;
        leanEnabled = true;
        canChangePerspective = true;
        weaponScript.canFire = true;
        weaponSwitching.canSwitchWeapon = true;
        weaponSway.enabled = true;
    }

    public void ControlOff()
    {
        canMove = false;
        canLook = false;
        leanEnabled = false;
        canChangePerspective = false;
        weaponScript.canFire = false;
        weaponSwitching.canSwitchWeapon = false;
        weaponSway.enabled = false;
    }

    public void ResetCamera()
    {
        playerCamera.transform.parent = recoilScript.transform;
        playerCamera.transform.localPosition = Vector3.zero;
        playerCamera.transform.localRotation = Quaternion.identity;
    }

    public void AttachCameraToHead(Transform headBone)
    {
        playerCamera.transform.parent = headBone;
        playerCamera.transform.localPosition = Vector3.zero;
        playerCamera.transform.localRotation = Quaternion.identity;
    }

    void UpdateMeleePosition()
    {
        playerCamera.transform.localPosition = Vector3.zero;
        playerCamera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        cameraFollower.transform.localPosition = Vector3.zero;
        cameraFollower.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        Vector3 enemyDirection = (meleePerson.position - transform.position).normalized;
        float targetYRotation = Quaternion.LookRotation(enemyDirection).eulerAngles.y + 6.5f;
        Quaternion targetRotation = Quaternion.Euler(0, targetYRotation, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, meleePositionSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, meleePosition, meleePositionSpeed * Time.deltaTime);
    }

    public void GetWeaponScript()
    {
        weaponScript = GetComponentInChildren<WeaponScript>();
        weaponAnimator = weaponScript.GetComponent<Animator>();
        if(weaponScript == null || weaponAnimator == null)
        {
            Debug.LogWarning("No WeaponScript Found!");
        }
    }
}
