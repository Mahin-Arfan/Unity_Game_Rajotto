using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraScriptTPP : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private GameObject actionCamera;
    public Animator actionCameraAnimator;
    private CinemachinePOV pov;
    private CinemachineFramingTransposer transposer;
    private MovementScriptTPP movementScript;
    private Animator animator;
    public float cameraSpeed = 10f;
    public float cameraFOVSpeed = 50f;
    VolumeProfile postProcessing;
    CinemachineBasicMultiChannelPerlin noiseComponent;

    [Header("Normal Position")]
    public float normalFOV = 40f;
    public Vector3 standNormalPosition = new Vector3(0f, 2.1f, 0f);
    public Vector3 crouchNormalPosition = new Vector3(0f, 1.5f, 0f);
    [Header("Running Position")]
    public float runningFOV = 40f;
    public Vector3 standRunningPosition = new Vector3(0f, 2.1f, 0f);
    public Vector3 crouchRunningPosition = new Vector3(0f, 1.5f, 0f);
    [Header("Aim Position")]
    public float aimFOV = 30f;
    public Vector3 crouchAimPosition = new Vector3(0f, 1.4f, 0f);
    public Vector3 standAimPosition = new Vector3(0f, 2f, 0f);
    [Header("Cover Aim Position")]
    public Vector3 coverCrouchMiddleAimPosition = new Vector3(0f, 2f, 0f);
    public float coverAimTPPScreenXRight = 0.15f;
    public float coverAimTPPScreenXLeft = 0.85f;

    [Header("TPP Camera Angle")]
    public bool tppSideRight = true;
    public float normalTPPScreenXRight = 0.3f;
    public float normalTPPScreenXLeft = 0.7f;
    public float aimTPPScreenXRight = 0.15f;
    public float aimTPPScreenXLeft = 0.85f;
    public float aimTPPScreenExtraMove = 0.1f;
    float tppSideChangeTime = 0f;
    public float tppSideChangeSpeed = 3f;
    float aimSide = 0f;

    [Header("Camera Noise")]
    public float runningNoiseAmplitude = 1f;
    public float runningNoiseFrequency = 3f;
    public float walkingNoiseAmplitude = 1f;
    public float walkingNoiseFrequency = 1f;
    public float idleNoiseAmplitude = 1f;
    public float idleNoiseFrequency = 1f;

    //Hashes
    int aimSideHash;

    void Start()
    {
        if(GetComponent<MovementScriptTPP>() == null)
        {
            Debug.LogWarning("MovementScript Not Attached To the Object!");
        }
        else
        {
            movementScript = GetComponent<MovementScriptTPP>();
        }
        transposer = camera.GetCinemachineComponent<CinemachineFramingTransposer>();
        pov = camera.GetCinemachineComponent<CinemachinePOV>();
        noiseComponent = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (transposer == null)
        {
            Debug.LogWarning("transposer not found!");
        }
        if (pov == null) 
        {
            Debug.LogWarning("POV not found!");
        }
        if (noiseComponent == null)
        {
            Debug.LogWarning("NoiseComponent not found!");
        }
        animator = GetComponent<Animator>();
        aimSideHash = Animator.StringToHash("AimSide");
        postProcessing = FindObjectOfType<Volume>().profile;
    }

    void Update()
    {
        AimSideAnimation();
        if(noiseComponent != null)
        {
            SetNoise();
        }

        if (!movementScript.isCrouching)
        {
            if (movementScript.isAiming)
            {
                transposer.m_TrackedObjectOffset = Vector3.Lerp(transposer.m_TrackedObjectOffset, standAimPosition, cameraSpeed * Time.deltaTime);
                camera.m_Lens.FieldOfView = Mathf.MoveTowards(camera.m_Lens.FieldOfView, aimFOV, Time.deltaTime * cameraFOVSpeed);
            }
            else
            {
                if (movementScript.isRunning)
                {
                    transposer.m_TrackedObjectOffset = Vector3.Lerp(transposer.m_TrackedObjectOffset, standRunningPosition, cameraSpeed * Time.deltaTime);
                    camera.m_Lens.FieldOfView = Mathf.MoveTowards(camera.m_Lens.FieldOfView, runningFOV, Time.deltaTime * cameraFOVSpeed);
                }
                else
                {
                    transposer.m_TrackedObjectOffset = Vector3.Lerp(transposer.m_TrackedObjectOffset, standNormalPosition, cameraSpeed * Time.deltaTime);
                    camera.m_Lens.FieldOfView = Mathf.MoveTowards(camera.m_Lens.FieldOfView, normalFOV, Time.deltaTime * cameraFOVSpeed);
                }
            }
        }
        else
        {
            if (movementScript.isAiming)
            {
                if(movementScript.isCovering && movementScript.coverAimSide == 0)
                {
                    transposer.m_TrackedObjectOffset = Vector3.Lerp(transposer.m_TrackedObjectOffset, coverCrouchMiddleAimPosition, cameraSpeed * Time.deltaTime);
                    camera.m_Lens.FieldOfView = Mathf.MoveTowards(camera.m_Lens.FieldOfView, aimFOV, Time.deltaTime * cameraFOVSpeed);
                }
                else
                {
                    transposer.m_TrackedObjectOffset = Vector3.Lerp(transposer.m_TrackedObjectOffset, crouchAimPosition, cameraSpeed * Time.deltaTime);
                    camera.m_Lens.FieldOfView = Mathf.MoveTowards(camera.m_Lens.FieldOfView, aimFOV, Time.deltaTime * cameraFOVSpeed);
                }
            }
            else
            {
                if (movementScript.isRunning)
                {
                    transposer.m_TrackedObjectOffset = Vector3.Lerp(transposer.m_TrackedObjectOffset, crouchRunningPosition, cameraSpeed * Time.deltaTime);
                    camera.m_Lens.FieldOfView = Mathf.MoveTowards(camera.m_Lens.FieldOfView, runningFOV, Time.deltaTime * cameraFOVSpeed);
                }
                else
                {
                    transposer.m_TrackedObjectOffset = Vector3.Lerp(transposer.m_TrackedObjectOffset, crouchNormalPosition, cameraSpeed * Time.deltaTime);
                    camera.m_Lens.FieldOfView = Mathf.MoveTowards(camera.m_Lens.FieldOfView, normalFOV, Time.deltaTime * cameraFOVSpeed);
                }
            }
        }

        TPPSide();
        tppSideChangeTime += Time.deltaTime;
    }

    void TPPSide()
    {
        if(Input.GetKeyDown(KeyCode.V) && tppSideChangeTime > 0.5f)
        {
            if (!(movementScript.isCovering && movementScript.isAiming)) 
            {
                tppSideRight = !tppSideRight;
                tppSideChangeTime = 0f;
            }
        }
        if(movementScript.isAiming)
        {
            DepthOfField depthOfField;
            if (postProcessing.TryGet(out depthOfField))
            {
                depthOfField.focalLength.value = 90f;
            }

            if (tppSideRight)
            {
                if (movementScript.horizontalInput > 0)
                {
                    if (!movementScript.isCovering)
                    {
                        transposer.m_ScreenX = Mathf.MoveTowards(transposer.m_ScreenX, aimTPPScreenXRight - aimTPPScreenExtraMove, Time.deltaTime * tppSideChangeSpeed);
                    }
                }
                else
                {
                    if(movementScript.isCovering && movementScript.coverAimSide != 0)
                    {
                        transposer.m_ScreenX = Mathf.MoveTowards(transposer.m_ScreenX, coverAimTPPScreenXRight, Time.deltaTime * tppSideChangeSpeed);
                    }
                    else
                    {
                        transposer.m_ScreenX = Mathf.MoveTowards(transposer.m_ScreenX, aimTPPScreenXRight, Time.deltaTime * tppSideChangeSpeed);
                    }
                }
            }
            else
            {
                if (movementScript.horizontalInput < 0)
                {
                    if (!movementScript.isCovering)
                    {
                        transposer.m_ScreenX = Mathf.MoveTowards(transposer.m_ScreenX, aimTPPScreenXLeft + aimTPPScreenExtraMove + 0.1f, Time.deltaTime * tppSideChangeSpeed);
                    }
                }
                else
                {
                    if (movementScript.isCovering && movementScript.coverAimSide != 0)
                    {
                        transposer.m_ScreenX = Mathf.MoveTowards(transposer.m_ScreenX, coverAimTPPScreenXLeft, Time.deltaTime * tppSideChangeSpeed);
                    }
                    else
                    {
                        transposer.m_ScreenX = Mathf.MoveTowards(transposer.m_ScreenX, aimTPPScreenXLeft, Time.deltaTime * tppSideChangeSpeed);
                    }
                }
            }
        }
        else
        {
            DepthOfField depthOfField;
            if (postProcessing.TryGet(out depthOfField))
            {
                depthOfField.focalLength.value = 0f;
            }

            if (tppSideRight)
            {
                transposer.m_ScreenX = Mathf.MoveTowards(transposer.m_ScreenX, normalTPPScreenXRight, Time.deltaTime * tppSideChangeSpeed);
            }
            else
            {
                transposer.m_ScreenX = Mathf.MoveTowards(transposer.m_ScreenX, normalTPPScreenXLeft, Time.deltaTime * tppSideChangeSpeed);
            }
        }
    }

    void AimSideAnimation()
    {
        if (tppSideRight)
        {
            aimSide = Mathf.MoveTowards(aimSide, 0f, Time.deltaTime * tppSideChangeSpeed);
        }
        else
        {
            aimSide = Mathf.MoveTowards(aimSide, 1f, Time.deltaTime * tppSideChangeSpeed);
        }

        animator.SetFloat(aimSideHash, aimSide);
    }

    public float GetHorizontalCameraValue()
    {
        if (pov != null)
            return pov.m_HorizontalAxis.Value;
        else
            return 0f;
    }

    void SetNoise()
    {
        if (movementScript.isRunning)
        {
            noiseComponent.m_AmplitudeGain = runningNoiseAmplitude;
            noiseComponent.m_FrequencyGain = runningNoiseFrequency;
        }else if (movementScript.isWalking)
        {
            noiseComponent.m_AmplitudeGain = walkingNoiseAmplitude;
            noiseComponent.m_FrequencyGain = walkingNoiseFrequency;
        }
        else
        {
            noiseComponent.m_AmplitudeGain = idleNoiseAmplitude;
            noiseComponent.m_FrequencyGain = idleNoiseFrequency;
        }
    }

    public void ActionCamera()
    {
        camera.gameObject.SetActive(false);
        actionCamera.SetActive(true);
        actionCameraAnimator.gameObject.SetActive(true);
    }

    public void ResetCamera()
    {
        camera.gameObject.SetActive(true);
        actionCamera.SetActive(false);
        actionCameraAnimator.gameObject.SetActive(false);
    }
}
