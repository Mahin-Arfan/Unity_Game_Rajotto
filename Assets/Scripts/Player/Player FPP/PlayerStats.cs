using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float runningMovementSpeed = 4.8f;
    public float walkingMovementSpeed = 3f;
    public float crouchingMovementSpeed = 1.5f;

    public float gravity = -21f;
    public float jumpHeight = 1f;

    public float crouchHeightY = 1f;
    public float standingHeightY = 2f;
    public float playerHeightSpeed = 10f;

    [Header("TPPMovement")]
    public float tppRunningMovementSpeed = 2;
    public float tppWalkingMovementSpeed = 0.5f;
    public float tppCrouchingMovementSpeed = 0.5f;
    public float tppCrouchRunningSpeed = 1.5f;

    [Header ("AimMovement")]
    public float aimForwardSpeed = 1f;
    public float aimBackwardSpeed = 0.5f;
    public float aimRightSpeed = 0.6f;
    public float aimLeftSpeed = 0.6f;
    public float crouchAimForwardSpeed = 0.7f;
    public float crouchAimBackwardSpeed = 0.5f;
    public float crouchAimRightSpeed = 0.6f;
    public float crouchAimLeftSpeed = 0.6f;
}
