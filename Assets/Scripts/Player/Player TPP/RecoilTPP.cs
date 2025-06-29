using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Animations.Rigging;

public class RecoilTPP : MonoBehaviour
{
    [Header("Recoil")]
    float verticalRecoil;
    float horizontalRecoil;
    float recoilTime;
    public float returnSpd = 0f;
    public float snappinss = 0f;
    [Header("Bullet Spread")]
    public float currentBulletSpread = 0f;
    float targetBulletSpread = 0f;
    float maxBulletSpread = 0.5f;
    [Header("HandRecoil")]
    private float handTargetWeight;
    private float handCurrentWeight;
    public TwoBoneIKConstraint rightHandWeightBone;
    public MultiAimConstraint clavicleWeightBone;
    public float maxWeight = 0.4f;
    public float handReturnSpd = 8f;
    public float handSnapinss = 10f;
    public float handRecoilRate = 0.3f;
    [Header("References")]
    public CinemachineVirtualCamera playerCamera;
    private CinemachinePOV pov;
    private MovementScriptTPP movementScript;

    void Start()
    {
        pov = playerCamera.GetCinemachineComponent<CinemachinePOV>();
        movementScript = GetComponent<MovementScriptTPP>();
    }

    void Update()
    {
        if(recoilTime > 0f)
        {
            pov.m_VerticalAxis.Value -= ((verticalRecoil/ 10) * Time.deltaTime) / snappinss;
            pov.m_HorizontalAxis.Value -= ((horizontalRecoil / 10) * Time.deltaTime) / snappinss;
            recoilTime -= Time.deltaTime;
        }
        //Bullet Spread
        targetBulletSpread = Mathf.Lerp(targetBulletSpread, 0f, returnSpd * Time.deltaTime);
        float newCurrentBulletSpread = Mathf.Lerp(currentBulletSpread, targetBulletSpread, snappinss * Time.fixedDeltaTime);
        currentBulletSpread = Mathf.Clamp(newCurrentBulletSpread, 0f, maxBulletSpread);

        if (Mathf.Abs(currentBulletSpread) < 0.0001f)
        {
            currentBulletSpread = 0f;
            targetBulletSpread = 0f;
        }
        //Hand Recoil
        handTargetWeight = Mathf.Lerp(handTargetWeight, 0f, handReturnSpd* Time.deltaTime);
        float newHandCurrentWeight = Mathf.Lerp(handCurrentWeight, handTargetWeight, handSnapinss * Time.fixedDeltaTime);
        handCurrentWeight = Mathf.Clamp(newHandCurrentWeight, 0f, maxWeight);
        rightHandWeightBone.weight = handCurrentWeight;
        clavicleWeightBone.weight = handCurrentWeight;
        if (Mathf.Abs(handCurrentWeight) < 0.0001f)
        {
            handCurrentWeight = 0f;
            handTargetWeight = 0f;
        }
    }

    public void RecoilFireTPP(float recX, float recY, float recZ, float RSpeed, float Snap, CinemachineImpulseSource source)
    {
        returnSpd = RSpeed;
        snappinss = Snap;
        recoilTime = snappinss/100;
        verticalRecoil = recX*50; 
        horizontalRecoil = Random.Range(-recY*50, recY*50);
        if(source == null)
        {
            Debug.LogWarning("Impulse Not Found!");
        }
        else
        {
            source.GenerateImpulse(movementScript.playerCamera.forward);
        }
        handTargetWeight += handRecoilRate;
    }

    public void BulletSpread(float spreadAmount)
    {
        maxBulletSpread = spreadAmount;
        targetBulletSpread += spreadAmount;
    }
}
