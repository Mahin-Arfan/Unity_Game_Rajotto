using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeScriptTPP : MonoBehaviour
{
    public float meleePositionSpeed = 5f;
    int stabCount = 0;

    [Header("References")]
    public GameManagerScript gameManager;
    public GameObject knife;
    public GameObject impactEffectBlood;
    public GameObject stabAudio;
    public GameObject stabSkullAudio;
    public AudioClip stabSkullHurtAudio;

    [HideInInspector] public Transform meleePerson;
    [HideInInspector] public Animator animator;
    [HideInInspector] public MovementScriptTPP movementScript;
    [HideInInspector] public CameraScriptTPP cameraScript;
    [HideInInspector] public Vector3 meleePosition;
    [HideInInspector] public float meleeCounterTime = 0f;
    [HideInInspector] public bool meleeing = false;
    private bool scriptChecked = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        movementScript = GetComponent<MovementScriptTPP>();
        cameraScript = GetComponent<CameraScriptTPP>();
        gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager == null)
        {
            Debug.LogError("GameManagerScript Not Found!");
        }
    }

    void Update()
    {
        if (scriptChecked) { return; }
        if (meleeing)
        {
            animator.SetBool("Meleeing", true);
            meleeCounterTime += Time.deltaTime;
            UpdateMeleePosition();
            if (Input.GetKeyDown(KeyCode.F) && !animator.GetBool("MeleePass") && meleeCounterTime <= 0.5f)
            {
                animator.SetBool("MeleePass", true);
                meleePerson.GetComponent<Animator>().SetBool("MeleePass", true);
                cameraScript.actionCameraAnimator.SetBool("MeleePass", true);
                gameManager.timeManager.ResetTime();
                gameManager.ActionButtonOff();
            }
            else if (meleeCounterTime > 0.5f)
            {
                gameManager.timeManager.ResetTime();
                gameManager.ActionButtonOff();
            }
            if (meleePerson == null || meleePerson.GetComponent<AiAgent>().deathState.IsDead() || !meleePerson.GetComponent<AiAgent>().meleeing)
            {
                MeleeFinish();
            }
        }
        else
        {
            animator.SetBool("Meleeing", false);
            gameManager.timeManager.ResetTime();
            gameManager.ActionButtonOff();
            scriptChecked = true;
        }
    }

    public void MeleeStart()
    {
        movementScript.ControlOff();
        animator.SetBool("MeleePass", false);
        meleeing = true;
        meleeCounterTime = 0f;
        movementScript.leftHandRigState = false;
        movementScript.PlayerOccupied();
        cameraScript.ActionCamera();
        cameraScript.actionCameraAnimator.SetTrigger("Melee");
        cameraScript.actionCameraAnimator.SetBool("MeleePass", false);
        if (knife != null) { knife.SetActive(true); }
        stabCount = 0;
        movementScript.gameManager.timeManager.DoSlowMotion();
        movementScript.gameManager.ActionButton("F", true);
        scriptChecked = false;
    }

    public void MeleeFinish()
    {
        meleeing = false;
        meleePerson = null;
        cameraScript.ResetCamera();
        movementScript.leftHandRigState = true;
        GetComponent<WeaponSwitchingTPP>().WeaponEquip();
        movementScript.PlayerUnoccupied();
        if (knife != null) { knife.SetActive(false); }
        movementScript.ControlOn();
    }

    public void MeleeStab()
    {
        if (knife == null) { return; }
        GameObject impactEffect = Instantiate(impactEffectBlood, knife.transform.position, Quaternion.identity);
        if (stabCount < 2)
        {
            GameObject impactAudio = Instantiate(stabAudio, knife.transform.position, Quaternion.identity);
            stabCount++;
            Destroy(impactAudio, 2f);
        }
        else
        {
            GameObject impactAudio = Instantiate(stabSkullAudio, knife.transform.position, Quaternion.identity);
            Destroy(impactAudio, 2f);
            AudioSource enemyAudioSource = meleePerson.GetComponent<AudioSource>();
            enemyAudioSource.clip = stabSkullHurtAudio;
            enemyAudioSource.Play();
        }
        Destroy(impactEffect, 2f);
    }

    void UpdateMeleePosition()
    {
        Vector3 enemyDirection = (meleePerson.position - transform.position).normalized;
        float targetYRotation = Quaternion.LookRotation(enemyDirection).eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, targetYRotation, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, meleePositionSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, meleePosition, meleePositionSpeed * Time.deltaTime);
    }
}
