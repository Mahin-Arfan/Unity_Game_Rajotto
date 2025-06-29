using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class MovementScriptTPP : MonoBehaviour
{
    [Header("States")]
    public bool canMove = true;
    public bool canLook = true;
    public bool canAim = true;
    public bool canCover = true;
    public bool canJump = true;
    public bool weaponCloseMode = true;
    public bool aimSwayEnabled = true;

    [Header("References")]
    public Transform playerCamera;
    private CameraScriptTPP cameraScriptTPP;
    [SerializeField] Transform feetTouchCheck;
    [SerializeField] LayerMask groundMask;
    public Animator animator;
    private CharacterController characterController;
    private PlayerStats playerStats;
    public GameObject fullMag;
    public CharacterDeadState deathState;
    public GameManagerScript gameManager;
    [HideInInspector] public PlayerPerspectiveScript perspectiveScript;

    [Header("MovementSettings")]
    [HideInInspector] public float verticalInput;
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float currentMovement = 0f;
    private Vector3 movementVector;
    float playerState = 0f;
    [SerializeField] private float playerStateSpeed = 0.5f;
    [HideInInspector] public float playerAimState = 0f;
    [HideInInspector] public float playerVelocity = 0.0f;
    [SerializeField] private float playerAcceleration = 1f;
    [SerializeField] private float playerDeceleration = 1f;
    [SerializeField] private float turnSmoothTime = 0.3f;
    [SerializeField] private float velocityToStopRatio = 1f;
    float turnSmoothVelocity = 5f;
    float adjustMovementSpeed = 0f;
    float crouchTime = 0f;
    bool canStand = true;
    private bool forwardMovement = false;
    [SerializeField]
    private Vector3 feetCheckBox = new Vector3(0.4f, 0.32f, 0.4f);

    [Header("JumpSettings")]
    bool canNormalJump = true;
    public LayerMask jumpLayer;
    private float jumpTime = 0f;
    private float inAirTime = 0f;
    public GameObject jumpChecker;
    public Collider colliderBottom;
    public Collider colliderMiddle;
    public Collider colliderTop;
    public Collider colliderFront;

    bool isCollidingBottom = false;
    bool isCollidingMid = false;
    bool isCollidingTop = false;
    bool isCollidingFront = false;

    [Header("AimSettings")]
    [SerializeField] private float aimCoolDown = 0.5f;
    [SerializeField] private float aimSwaySmooth = 5f;
    [SerializeField] private float playerAimSpeed = 5f;
    float aimVelocityX = 0f;
    float aimVelocityZ = 0f;
    float aimForwardSpeed = 1f;
    float aimBackwardSpeed = 1f;
    float aimRightSpeed = 1f;
    float aimLeftSpeed = 1f;
    float lastAimEndTime = -Mathf.Infinity;

    [Header("CoverSettings")]
    public bool isCovering = false;
    [SerializeField] private LayerMask coverMask;
    [SerializeField] private GameObject coverCheck;
    [SerializeField] private float coverRange = 5f;
    [SerializeField] private float coverRotationSpeed = 500f;
    private float coverTime = 0f;
    bool aimPositionChecked = false;
    [SerializeField] private Transform coverAimChecker;
    [HideInInspector] public float coverAimSide = 0f; //Left = -1, Middle = 0, Right = 1
    private Collider coverCollider;
    private bool coverAimCameraAngleCheck = false;
    private bool coverToAim = false;
    private bool canMoveInCover = true;
    [SerializeField] private Vector3 coverMovementCheckRight; //(-0.3, 1.6, 0)
    [SerializeField] private Vector3 coverMovementCheckLeft; //(0.3, 1.6, 0)
    float coverAimCameraMinusMinV = -180f;
    float coverAimCameraMinusMaxV = 0f;
    float coverAimCameraPlusMinV = 0f;
    float coverAimCameraPlusMaxV = 180f;
    bool coverAimedBehind = false;

    [Header("Looking At")]
    [SerializeField] private Transform lookSource;
    [SerializeField] private LayerMask lookIgnore;
    public float lookDistance;
    float horizontalCameraValue = 0f;
    public bool weaponClose = false;

    [Header("Rigs")]
    public float chestWeight = 0.3f;
    public float spineWeight = 0.3f;
    public bool rigWeight = true;
    public bool leftHandRigState = true;
    public float leftHandRigSpeed = 5f;
    public float aimRigState = 0f;
    public Rig rig;
    public MultiAimConstraint rightHand;
    public Transform pistolWeaponPoint;
    public Vector3 pistolStandWeaponPoint;
    public Vector3 pistolCrouchWeaponPoint;
    public Vector3 pistolCrouchWeaponPointMoving;
    public Vector3 pistolStandWeaponPointMoving;
    public TwoBoneIKConstraint rightHandWeaponPoint;
    public TwoBoneIKConstraint rightHandWeaponPointPistol;
    public MultiAimConstraint rightHandPistol;
    public TwoBoneIKConstraint leftHand;
    public TwoBoneIKConstraint leftHandPistol;
    public MultiAimConstraint chest;
    public MultiAimConstraint spine;
    public MultiAimConstraint head;
    public bool rightHandRigPistol = false;

    [Header("Materials")]
    public Renderer[] renderers;
    public Transform lookAtTarget;
    public float alphaMinimumValue = 0.4f;

    //States
    public bool isGrounded { get; private set; } = true;
    public bool isRunning { get; private set; } = false;
    public bool isWalking { get; private set; } = false;
    public bool isJumping { get; private set; } = false;
    public bool isCrouching { get; private set; } = false;
    public bool isAiming { get; private set; } = false;
    public bool isHipFiring = false;
    //Hashes
    int playerVelocityHash;
    int playerStateHash;
    int playerAimStateHash;
    int playerVelocityXHash;
    int playerVelocityZHash;

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();
        cameraScriptTPP = GetComponent<CameraScriptTPP>();
        deathState = GetComponentInChildren<CharacterDeadState>();
        gameManager = FindObjectOfType<GameManagerScript>();
        if (transform.parent.parent != null)
        {
            perspectiveScript = transform.parent.parent.GetComponent<PlayerPerspectiveScript>();
        }
        else
        {
            Debug.LogError("Error Object: " + transform.name + "Parent is null. playerPerspectiveScript not assigned.");
        }
        playerVelocityHash = Animator.StringToHash("Velocity");
        playerStateHash = Animator.StringToHash("PlayerState");
        playerAimStateHash = Animator.StringToHash("AimState");
        playerVelocityXHash = Animator.StringToHash("VelocityX");
        playerVelocityZHash = Animator.StringToHash("VelocityZ");


        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnEnable()
    {
        rigWeight = true;
        leftHandRigState = true;
    }

    void Update()
    {
        isGrounded = Physics.CheckBox(feetTouchCheck.position, feetCheckBox, Quaternion.identity, groundMask);
        if (isGrounded)
        {
            if (!animator.GetBool("IsGrounded"))
            {
                animator.SetBool("IsGrounded", true);
                animator.SetFloat("InAirTime", inAirTime);
            }
            inAirTime = 0f;
        }
        else
        {
            inAirTime += Time.deltaTime;
            if (inAirTime >= 0.25f && animator.GetBool("IsGrounded"))
            {
                animator.SetBool("IsGrounded", false);
            }
        }

        crouchTime += Time.deltaTime;
        if (canMove && Input.GetKey(KeyCode.LeftControl) && crouchTime > 1f)
        {
            crouchTime = 0f;
            if (isCrouching)
            {
                CheckUp();
                if (canStand)
                {
                    HandleStandTPP();
                }
            }
            else
            {
                HandleCrouchTPP();
            }
        }
        coverTime += Time.deltaTime;
        if (Input.GetButton("Fire2"))
        {
            if (canAim && !isRunning && !isAiming && (Time.time > lastAimEndTime + aimCoolDown))
            {
                isHipFiring = false;
                isAiming = true;
            }
        }
        else
        {
            if (isAiming)
            {
                isAiming = false;
                coverToAim = false;
                lastAimEndTime = Time.time;
            }
            if (Input.GetButton("Fire1"))
            {
                isHipFiring = true;
            }
            else
            {
                isHipFiring = false;
            }
        }

        if (canCover)
        {
            if (isCovering)
            {
                animator.SetBool("Covering", true);
                HandleCover();
            }
            else
            {
                animator.SetBool("Covering", false);
                coverCollider = null;
            }
        }
        else
        {
            isCovering = false;
            animator.SetBool("Covering", false);
            coverCollider = null;
        }



        AimRigWeight();
        LeftHandRigWeight();
        PlayerBodyAlpha();
        LookingAt();

        if (canJump && !isCovering)
        {
            if (jumpTime > 0.5f)
            {
                isJumping = false;
            }
            jumpTime += Time.deltaTime;
            if (Input.GetKey(KeyCode.Space) && jumpTime > 1f)
            {
                CheckUp();
                jumpTime = 0f;
                jumpChecker.SetActive(true);
                // Check collision states
                isCollidingBottom = IsColliding(colliderBottom);
                isCollidingMid = IsColliding(colliderMiddle);
                isCollidingTop = IsColliding(colliderTop);
                isCollidingFront = IsColliding(colliderFront);
                HandleJump();
                isJumping = true;
                jumpChecker.SetActive(false);
            }
            else
            {
                animator.SetBool("Jump Normal", false);
                animator.SetBool("Jump Vault", false);
                animator.SetBool("Jump Over", false);
            }
        }

        if (!isJumping && canMove && isGrounded)
        {
            HandleMovementTPP();
            CharacterMovement();
        }

        animator.SetFloat(playerVelocityHash, playerVelocity);
        animator.SetFloat(playerStateHash, playerState);
        animator.SetFloat(playerAimStateHash, playerAimState);
        animator.SetFloat(playerVelocityXHash, aimVelocityX);
        animator.SetFloat(playerVelocityZHash, aimVelocityZ);
    }

    void HandleMovementTPP()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (verticalInput != 0 || horizontalInput != 0)
        {
            isRunning = Input.GetKey(KeyCode.LeftShift);
            isWalking = !isRunning;
        }
        else
        {
            isWalking = false;
            isRunning = false;
        }

        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        adjustMovementSpeed = currentMovement + playerVelocity * velocityToStopRatio;

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

        if (isAiming || isHipFiring)
        {
            if (playerAimState < 1)
            {
                playerAimState += Time.deltaTime * playerAimSpeed;
            }
            playerVelocity = 0;
            AimSway();
        }
        else
        {
            if (playerAimState > 0)
            {
                playerAimState -= Time.deltaTime * playerAimSpeed;
            }
        }

        if(playerAimState > 1)
        {
            playerAimState = 1;
        }
        if (playerAimState < 0)
        {
            playerAimState = 0;
        }

        if (direction.magnitude >= 0.1f)
        {
            if (isAiming || isHipFiring)
            {
                if(horizontalInput > 0f)
                {
                    currentMovement = aimRightSpeed;
                    if (aimVelocityX < 1f)
                    {
                        aimVelocityX += Time.deltaTime * playerAcceleration*2;
                    }
                }
                else if(horizontalInput < 0f)
                {
                    currentMovement = aimLeftSpeed;
                    if (aimVelocityX > -1f)
                    {
                        aimVelocityX -= Time.deltaTime * playerAcceleration*2;
                    }
                }
                else
                {
                    aimVelocityX = Mathf.MoveTowards(aimVelocityX, 0f, Time.deltaTime);
                    if (aimVelocityX < 0.1f && aimVelocityX > -0.1f)
                    {
                        aimVelocityX = 0f;
                    }
                }
                if (verticalInput > 0f)
                {
                    currentMovement = aimForwardSpeed;
                    if (aimVelocityZ < 1f)
                    {
                        aimVelocityZ += Time.deltaTime * playerAcceleration * 2;
                    }
                }
                else if (verticalInput < 0f)
                {
                    currentMovement = aimBackwardSpeed;
                    if (aimVelocityZ > -1f)
                    {
                        aimVelocityZ -= Time.deltaTime * playerAcceleration * 2;
                    }
                }
                else
                {
                    aimVelocityZ = Mathf.MoveTowards(aimVelocityZ, 0f, Time.deltaTime);
                    if (aimVelocityZ < 0.1f && aimVelocityZ > -0.1f)
                    {
                        aimVelocityZ = 0f;
                    }
                }
            }
            else
            {
                if (!isCovering)
                {
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }
                if (isCrouching)
                {
                    if (isRunning && !isCovering)
                    {
                        currentMovement = playerStats.tppCrouchRunningSpeed;
                        if (playerVelocity < 1f)
                        {
                            playerVelocity += Time.deltaTime * playerAcceleration * 2;
                        }
                    }
                    else
                    {
                        currentMovement = playerStats.tppCrouchingMovementSpeed;
                        if (playerVelocity < 0.5f)
                        {
                            playerVelocity += Time.deltaTime * playerAcceleration;
                        }
                        else if (playerVelocity > 0.51f)
                        {
                            playerVelocity -= Time.deltaTime * playerDeceleration;
                        }
                    }
                }
                else if (isRunning && !isCovering)
                {
                    currentMovement = playerStats.tppRunningMovementSpeed;
                    if (playerVelocity < 1f)
                    {
                        playerVelocity += Time.deltaTime * playerAcceleration * 2;
                    }
                }
                else
                {
                    currentMovement = playerStats.tppWalkingMovementSpeed;

                    if (playerVelocity < 0.5f)
                    {
                        playerVelocity += Time.deltaTime * playerAcceleration;
                    }
                    else if (playerVelocity > 0.51f)
                    {
                        playerVelocity -= Time.deltaTime * playerDeceleration;
                    }
                }
            }
            if(!isAiming && !isCovering && !isHipFiring)
            {
                //Aim Velocity normalize
                aimVelocityX = Mathf.MoveTowards(aimVelocityX, 0f, Time.deltaTime);
                aimVelocityZ = Mathf.MoveTowards(aimVelocityZ, 0f, Time.deltaTime);
                if (aimVelocityX < 0.1f && aimVelocityX > -0.1f)
                {
                    aimVelocityX = 0f;
                }
                if (aimVelocityZ < 0.1f && aimVelocityZ > -0.1f)
                {
                    aimVelocityZ = 0f;
                }
            }
        }
        else
        {
            currentMovement = 0f;
            if (playerVelocity > 0f)
            {
                playerVelocity -= Time.deltaTime * playerDeceleration;
            }else if(playerVelocity < 0f)
            {
                playerVelocity = 0f;
            }
            aimVelocityX = Mathf.MoveTowards(aimVelocityX, 0f, Time.deltaTime);
            aimVelocityZ = Mathf.MoveTowards(aimVelocityZ, 0f, Time.deltaTime);
            if(aimVelocityX < 0.1f && aimVelocityX > -0.1f)
            {
                aimVelocityX = 0f;
            }
            if(aimVelocityZ < 0.1f && aimVelocityZ > -0.1f)
            {
                aimVelocityZ = 0f;
            }
        }

        if (isCrouching)
        {
            playerState = Mathf.MoveTowards(playerState, 1f, Time.deltaTime / playerStateSpeed);
        }
        else
        {
            playerState = Mathf.MoveTowards(playerState, 0f, Time.deltaTime / playerStateSpeed);
        }
    }

    void HandleJump()
    {
        if (isCollidingTop)
        {
            // Player jump In Place
            if (canNormalJump)
            {
                animator.SetBool("Jump Normal", true);
                return;
            }
        }
        else if (isCollidingBottom && isCollidingMid && !isCollidingTop && !isCollidingFront)
        {
            // Play jumpVault animation
            animator.SetBool("Jump Vault", true);
        }
        else if (isCollidingBottom && isCollidingMid && !isCollidingTop && isCollidingFront)
        {
            // Play jumpOver animation
            animator.SetBool("Jump Over", true);
        }
        else if (isCollidingBottom && !isCollidingMid && !isCollidingTop)
        {
            // Play jump animation
            animator.SetBool("Jump Normal", true);
        }
        else
        {
            animator.SetBool("Jump Normal", true);
        }
    }

    void CharacterMovement()
    {
        if (!characterController.enabled)
            return;

        bool isShootingStance = isAiming || isHipFiring;
        forwardMovement = isShootingStance || isCovering;

        if (isCovering)
        {
            if (isShootingStance)
            {
                movementVector = Vector3.zero;
            }
            else
            {
                movementVector = canMoveInCover
                    ? Vector3.ClampMagnitude(transform.right * -horizontalInput, 1.0f)
                    : Vector3.zero;
            }
        }
        else if (isShootingStance)
        {
            movementVector = Vector3.ClampMagnitude(transform.right * horizontalInput + transform.forward * verticalInput, 1.0f);
        }
        else
        {
            movementVector = Vector3.zero;
        }

        Vector3 moveDirection = forwardMovement
            ? movementVector.normalized
            : transform.forward.normalized;

        characterController.Move(moveDirection * adjustMovementSpeed * Time.deltaTime);
    }

    void HandleCrouchTPP()
    {
        isCrouching = true;
        aimForwardSpeed = playerStats.crouchAimForwardSpeed;
        aimBackwardSpeed = playerStats.crouchAimBackwardSpeed;
        aimRightSpeed = playerStats.crouchAimRightSpeed;
        aimLeftSpeed = playerStats.crouchAimLeftSpeed;
        Vector3 center = characterController.center;
        center.y = 0.65f;
        characterController.center = center;
        characterController.height = playerStats.crouchHeightY;
    }

    void HandleStandTPP()
    {
        isCrouching = false;
        aimForwardSpeed = playerStats.aimForwardSpeed;
        aimBackwardSpeed = playerStats.aimBackwardSpeed;
        aimRightSpeed = playerStats.aimRightSpeed;
        aimLeftSpeed = playerStats.aimLeftSpeed;
        Vector3 center = characterController.center;
        center.y = 1f;
        characterController.center = center;
        characterController.height = playerStats.standingHeightY;
    }

    void CheckUp()
    {
        RaycastHit hit;
        if (Physics.Raycast(feetTouchCheck.position, Vector3.up, out hit, 3f, groundMask))
        {
            if(hit.distance < 1.88f)
            {
                canStand = false;
            }
            else
            {
                canStand = true;
            }
            if(hit.distance < 2.2f)
            {
                canNormalJump = false;
            }
            else
            {
                canNormalJump = true;
            }
        }
        else
        {
            canStand = true;
            canNormalJump = true;
        }
    }

    void HandleCover()
    {
        Vector3 coverSurface;
        RaycastHit hit;
        if (Physics.Raycast(coverCheck.transform.position, (coverCollider.transform.position - coverCheck.transform.position).normalized, out hit, coverRange, coverMask))
        {
            Debug.DrawRay(coverCheck.transform.position, (coverCollider.transform.position - coverCheck.transform.position).normalized * coverRange, Color.red);
            coverSurface = hit.normal;
        }
        else
        {
            isCovering = false;
            coverCollider = null; // Clear the stored collider
            coverTime = 0f;
            coverToAim = false;
            return;
        }
        if ((Input.GetKey(KeyCode.Q) && coverTime > 0.5f) || isRunning)
        {
            isCovering = false;
            coverTime = 0f;
            coverToAim = false;
            return;
        }
        if (!isAiming || !isHipFiring)
        {
            if (horizontalInput > 0f)
            {
                HandleCoverMovementCheck();
                if (canMoveInCover)
                {
                    currentMovement = aimRightSpeed;
                    if (aimVelocityX < 1f)
                    {
                        aimVelocityX += Time.deltaTime * playerAcceleration * 2;
                    }
                }
                else
                {
                    aimVelocityX = Mathf.MoveTowards(aimVelocityX, 0f, Time.deltaTime);
                    if (aimVelocityX < 0.1f && aimVelocityX > -0.1f)
                    {
                        aimVelocityX = 0f;
                    }
                }
                
            }
            else if (horizontalInput < 0f)
            {
                HandleCoverMovementCheck();
                if (canMoveInCover)
                {
                    currentMovement = aimLeftSpeed;
                    if (aimVelocityX > -1f)
                    {
                        aimVelocityX -= Time.deltaTime * playerAcceleration * 2;
                    }
                }
                else
                {
                    aimVelocityX = Mathf.MoveTowards(aimVelocityX, 0f, Time.deltaTime);
                    if (aimVelocityX < 0.1f && aimVelocityX > -0.1f)
                    {
                        aimVelocityX = 0f;
                    }
                }
            }
            else
            {
                aimVelocityX = Mathf.MoveTowards(aimVelocityX, 0f, Time.deltaTime);
                if (aimVelocityX < 0.1f && aimVelocityX > -0.1f)
                {
                    aimVelocityX = 0f;
                }
            }
            Quaternion coverRotation = Quaternion.LookRotation(coverSurface);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, coverRotation, coverRotationSpeed * Time.deltaTime);
            aimPositionChecked = false;
            coverAimCameraAngleCheck = false;
            coverToAim = false;
            coverAimedBehind = false;
        }
        else
        {
            if (!aimPositionChecked) 
            {
                HandleCoverAim();
            }
            if (coverAimSide == 0)
            {
                coverToAim = true;
                Quaternion coverRotation = Quaternion.Euler(0f, playerCamera.eulerAngles.y, 0f);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, coverRotation, aimSwaySmooth * Time.deltaTime);
            }
            else
            {
                Quaternion coverRotation = Quaternion.LookRotation(coverSurface);
                if (coverAimedBehind)
                {
                    Quaternion coverTargetRotation = Quaternion.Euler(0f, playerCamera.eulerAngles.y, 0f);
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, coverTargetRotation, aimSwaySmooth * Time.deltaTime);
                }
                if (!coverAimCameraAngleCheck)
                {
                    // Default values
                    coverAimCameraPlusMinV = 0f;
                    coverAimCameraPlusMaxV = 180f;
                    coverAimCameraMinusMinV = -180f;
                    coverAimCameraMinusMaxV = 0f;
                    // Normalize cover surface yaw to [-180, 180]
                    float yaw = coverRotation.eulerAngles.y;
                    float coverSurfaceYaw = yaw >= 0f ? yaw - 180f : yaw + 180f;
                    coverSurfaceYaw = NormalizeAngle(coverSurfaceYaw);

                    // Compute left and right bounds (±90° around the surface yaw)
                    float left = NormalizeAngle(coverSurfaceYaw - 90f);
                    float right = NormalizeAngle(coverSurfaceYaw + 90f);

                    // Assign left bound
                    if (left >= 0f)
                        coverAimCameraPlusMinV = left;
                    else
                        coverAimCameraMinusMinV = left;

                    // Assign right bound
                    if (right >= 0f)
                        coverAimCameraPlusMaxV = right;
                    else
                        coverAimCameraMinusMaxV = right;

                    coverAimCameraAngleCheck = true;
                }

                // Helper method
                float NormalizeAngle(float angle)
                {
                    angle %= 360f;
                    if (angle > 180f) angle -= 360f;
                    if (angle < -180f) angle += 360f;
                    return angle;
                }
                if (!coverToAim)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, coverRotation, (coverRotationSpeed * 10f) * Time.deltaTime);
                    coverToAim = true;
                }
                horizontalCameraValue = cameraScriptTPP.GetHorizontalCameraValue();
                bool isInPlusRange = horizontalCameraValue >= coverAimCameraPlusMinV && horizontalCameraValue <= coverAimCameraPlusMaxV;

                bool isInMinusRange = horizontalCameraValue >= coverAimCameraMinusMinV && horizontalCameraValue <= coverAimCameraMinusMaxV;

                if (!coverAimedBehind && !isInPlusRange && !isInMinusRange)
                {
                    animator.SetTrigger("CoverAimedBehind");
                    coverAimedBehind = true;
                }
            }
        }
    }

    void HandleCoverMovementCheck()
    {
        if(horizontalInput > 0f)
        {
            coverAimChecker.localPosition = coverMovementCheckRight;
            RaycastHit hit;
            if (Physics.Raycast(coverAimChecker.position, coverAimChecker.forward, out hit, 0.5f, coverMask))
            {
                canMoveInCover = true;
            }
            else
            {
                canMoveInCover = false;
            }
            Debug.DrawRay(coverAimChecker.position, coverAimChecker.forward * 0.5f, Color.red);
        }
        else
        {
            coverAimChecker.localPosition = coverMovementCheckLeft;
            RaycastHit hit;
            if (Physics.Raycast(coverAimChecker.position, coverAimChecker.forward, out hit, 0.5f, coverMask))
            {
                canMoveInCover = true;
            }
            else
            {
                canMoveInCover = false;
            }
            Debug.DrawRay(coverAimChecker.position, coverAimChecker.forward * 0.5f, Color.red);
        }
    }

    void HandleCoverAim() 
    {
        float coverAimSpace = 0f;

        if (cameraScriptTPP.tppSideRight)
        {
            Quaternion cameraAngle = Quaternion.Euler(0f, playerCamera.eulerAngles.y, 0f);
            coverAimChecker.localPosition = new Vector3(0f, 1.6f, 0f);

            RaycastHit rightHit;
            if (Physics.Raycast(coverAimChecker.position, cameraAngle * Vector3.right, out rightHit, 0.9f, coverMask))
            {
                coverAimSpace = 0f;
            }
            else
            {
                coverAimSpace = 1f;
            }

            RaycastHit hit;
            if (Physics.Raycast(coverAimChecker.position, cameraAngle * Vector3.forward, out hit, 1f, coverMask))
            {
                if(coverAimSpace == 1f)
                {
                    RaycastHit hit2;
                    coverAimChecker.localPosition = new Vector3(-0.5f, 1.6f, 0f);
                    if (Physics.Raycast(coverAimChecker.position, cameraAngle * Vector3.forward, out hit2, 1f, coverMask))
                    {
                        animator.SetFloat("Cover Side Aim", 0);
                        coverAimSide = 0;
                    }
                    else
                    {
                        animator.SetFloat("Cover Side Aim", 1);
                        coverAimSide = 1;
                    }
                }
                else
                {
                    animator.SetFloat("Cover Side Aim", 0);
                    coverAimSide = 0;
                }
            }
            else
            {
                animator.SetFloat("Cover Side Aim", 0);
                coverAimSide = 0;
            }
        }
        else
        {
            Quaternion cameraAngle = Quaternion.Euler(0f, playerCamera.eulerAngles.y, 0f);
            coverAimChecker.localPosition = new Vector3(0f, 1.6f, 0f);

            RaycastHit leftHit;
            if (Physics.Raycast(coverAimChecker.position, cameraAngle * Vector3.left, out leftHit, 0.9f, coverMask))
            {
                coverAimSpace = 0f;
            }
            else
            {
                coverAimSpace = -1f;
            }

            RaycastHit hit;
            if (Physics.Raycast(coverAimChecker.position, cameraAngle * Vector3.forward, out hit, 1f, coverMask))
            {
                if(coverAimSpace == -1f)
                {
                    RaycastHit hit2;
                    coverAimChecker.localPosition = new Vector3(0.5f, 1.6f, 0f);
                    if (Physics.Raycast(coverAimChecker.position, cameraAngle * Vector3.forward, out hit2, 1f, coverMask))
                    {
                        animator.SetFloat("Cover Side Aim", 0);
                        coverAimSide = 0;
                    }
                    else
                    {
                        animator.SetFloat("Cover Side Aim", 1);
                        coverAimSide = -1;
                    }
                }
                else
                {
                    animator.SetFloat("Cover Side Aim", 0);
                    coverAimSide = 0;
                }
            }
            else
            {
                animator.SetFloat("Cover Side Aim", 0);
                coverAimSide = 0;
            }
        }
        aimPositionChecked = true;
    }


    void AimSway()
    {
        if (!isCovering && aimSwayEnabled)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, playerCamera.eulerAngles.y, 0f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, aimSwaySmooth * Time.deltaTime);
        }
    }

    void LookingAt()
    {
        RaycastHit hit;

        if (Physics.Raycast(lookSource.position, lookSource.forward, out hit, 10, ~lookIgnore))
        {
            lookDistance = hit.distance;
        }
        else
        {
            lookDistance = 10;
        }
    }

    void AimRigWeight()
    {
        if (animator.GetBool("Reloading") || !rigWeight || weaponClose)
        {
            aimRigState = 0f;
        }
        else
        {
            aimRigState = playerAimState;
        }

        if (rightHandRigPistol)
        {
            rightHandWeaponPoint.weight = 0f;
            rightHand.weight = 0f;
            if(playerState >= 0.5)
            {
                pistolWeaponPoint.localPosition = Vector3.Lerp(pistolWeaponPoint.localPosition, pistolCrouchWeaponPoint, playerStateSpeed*2 * Time.deltaTime);
            }
            else
            {
                if (movementVector == Vector3.zero)
                {
                    pistolWeaponPoint.localPosition = Vector3.Lerp(pistolWeaponPoint.localPosition, pistolStandWeaponPoint, playerStateSpeed*2 * Time.deltaTime);
                }
                else
                {
                    pistolWeaponPoint.localPosition = Vector3.Lerp(pistolWeaponPoint.localPosition, pistolStandWeaponPointMoving, playerStateSpeed*2 * Time.deltaTime);
                }
            }
            rightHandWeaponPointPistol.weight = Mathf.Lerp(rightHandWeaponPointPistol.weight, aimRigState, playerAimSpeed * Time.deltaTime);
            rightHandPistol.weight = Mathf.Lerp(rightHandPistol.weight, aimRigState, playerAimSpeed * Time.deltaTime);
        }
        else
        {
            rightHandWeaponPointPistol.weight = 0f;
            rightHandPistol.weight = 0f;
            rightHand.weight = Mathf.Lerp(rightHand.weight, aimRigState, playerAimSpeed * Time.deltaTime);
            if (playerState >= 0.5)
            {
                rightHandWeaponPoint.weight = Mathf.Lerp(rightHandWeaponPoint.weight, aimRigState, playerAimSpeed * Time.deltaTime);
            }
            else
            {
                rightHandWeaponPoint.weight = Mathf.Lerp(rightHandWeaponPoint.weight, 0f, playerAimSpeed * Time.deltaTime);
            }
        }


        if (head.weight > 0.5f)
        {
            head.weight = Mathf.Lerp(head.weight, aimRigState, playerAimSpeed * 0.1f * Time.deltaTime);
        }
        else
        {
            head.weight = Mathf.Lerp(head.weight, aimRigState, playerAimSpeed * Time.deltaTime);
        }
        if(aimRigState <= 0)
        {
            chest.weight = Mathf.Lerp(chest.weight, aimRigState, playerAimSpeed * 0.5f * Time.deltaTime);
            spine.weight = Mathf.Lerp(spine.weight, aimRigState, playerAimSpeed * 0.5f * Time.deltaTime);
        }
        else
        {
            chest.weight = Mathf.Lerp(chest.weight, chestWeight, playerAimSpeed * Time.deltaTime);
            spine.weight = Mathf.Lerp(spine.weight, spineWeight, playerAimSpeed * Time.deltaTime);
        }
    }

    void LeftHandRigWeight()
    {
        if (leftHandRigState && rigWeight)
        {
            if (rightHandRigPistol)
            {
                leftHand.weight = Mathf.Lerp(leftHand.weight, 0f, leftHandRigSpeed * Time.deltaTime);
                leftHandPistol.weight = Mathf.Lerp(leftHandPistol.weight, 1f, leftHandRigSpeed * Time.deltaTime);
            }
            else
            {
                leftHand.weight = Mathf.Lerp(leftHand.weight, 1f, leftHandRigSpeed * Time.deltaTime);
                leftHandPistol.weight = Mathf.Lerp(leftHandPistol.weight, 0f, leftHandRigSpeed * Time.deltaTime);
            }
        }
        else
        {
            leftHandPistol.weight = Mathf.Lerp(leftHandPistol.weight, 0f, leftHandRigSpeed * Time.deltaTime);
            leftHand.weight = Mathf.Lerp(leftHand.weight, 0f, leftHandRigSpeed * Time.deltaTime);
        }
    }

    public void RigWeightOn()
    {
        rigWeight = true;
    }
    public void RigWeightOff()
    {
        rigWeight = false;
    }

    public void LeftHandRigOn()
    {
        leftHandRigState = true;
    }
    public void LeftHandRigOff()
    {
        leftHandRigState = false;
    }

    void MagLoading()
    {
        fullMag.gameObject.SetActive(true);
    }

    public void ControlOn()
    {
        canMove = true;
        canLook = true;
        canCover = true;
        canAim = true;
        canJump = true;
        weaponCloseMode = true;
        perspectiveScript.canChangePerspective = true;
        GetComponent<WeaponSwitchingTPP>().canChangeWeapon = true;
        aimSwayEnabled = true;
    }

    public void ControlOff()
    {
        canMove = false;
        canLook = false;
        canCover = false;
        canAim = false;
        canJump = false;
        weaponCloseMode = false;
        perspectiveScript.canChangePerspective = false;
        GetComponent<WeaponSwitchingTPP>().canChangeWeapon = false;
        aimSwayEnabled = false;
    }

    public void PlayerOccupied()
    {
        deathState.occupied = true;
    }

    public void PlayerUnoccupied()
    {
        deathState.occupied = false;
    }

    void CharacterControllerToggle()
    {
        if (characterController.enabled)
        {
            characterController.enabled = false;
        }
        else
        {
            characterController.enabled = true;
        }
    }

    bool IsColliding(Collider col)
    {
        Collider[] hitColliders = Physics.OverlapBox(col.bounds.center, col.bounds.extents, col.transform.rotation, jumpLayer);
        return hitColliders.Length > 0;
    }

    void OnTriggerStay(Collider collision)
    {
        if (canCover && !isCovering && coverMask == (coverMask | (1 << collision.gameObject.layer)))
        {
            RaycastHit hit;
            if (Physics.Raycast(coverCheck.transform.position, (collision.transform.position - coverCheck.transform.position).normalized, out hit, coverRange, coverMask))
            {
                Debug.DrawRay(coverCheck.transform.position, (collision.transform.position - coverCheck.transform.position).normalized * coverRange, Color.red);
                if (Input.GetKey(KeyCode.Q) && coverTime > 0.5f && !isCovering)
                {
                    isCovering = true;
                    coverTime = 0f;
                    coverCollider = collision; // Store the collider
                    Debug.Log("Cover Object Hit: " + hit.collider.name);
                }
            }
            else
            {
                isCovering = false;
                coverCollider = null; // Clear the stored collider
            }
        }
    }
    void OnTriggerExit(Collider collision)
    {
        // Only reset cover state if the collider exiting is the one we are taking cover with
        if (collision == coverCollider && !coverToAim)
        {
            isCovering = false;
            coverCollider = null; // Clear the stored collider
            Debug.LogWarning("Exit");
        }
    }

    void PlayerBodyAlpha()
    {
        float cameraDistance = Vector3.Distance(lookAtTarget.position, playerCamera.position);
        if (cameraDistance < alphaMinimumValue)
        {
            foreach (Renderer rend in renderers)
            {
                rend.enabled = false;
            }
        }
        else
        {
            foreach (Renderer rend in renderers)
            {
                rend.enabled = true;
            }
        }
    }
}
