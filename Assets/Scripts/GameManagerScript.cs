using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class GameManagerScript : MonoBehaviour
{
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public Transform waypoint;
    public float waypointDistance = 0f;
    public GameObject deadEnemyChair;
    public GameObject deadEnemyCar;

    [Header("Player Settings")]
    public GameObject player;
    public GameObject mainCamera;
    public GameObject cameraBone;
    public GameObject weapon;
    public PlayerController playerController;
    public WeaponSwitching weaponSwitching;

    [Header("RABS")]
    public GameObject[] rabs;

    [Header("Phase Settings")]
    public bool gateOpenContinue = false;
    public bool mainDoorOpenContinue = false;
    private bool mainDoorApproached = false;

    [Header("References")]
    [HideInInspector] public TimeManagerScript timeManager;
    [HideInInspector] public AudioManager audioManager;
    public AudioMixer audioMixer;
    public GameObject actionButton;

    [Header("For Video")]
    public bool tpp = false;
    public bool maramariSuruHoise = false;
    public GameObject moraRabta;
    public GameObject firstEnemy;
    bool firstEnemyFired = false;
    public GameObject enemys;
    public GameObject rpgEnemy;
    public GameObject playerTPP;
    public Camera cameraTPP;
    public GameObject playerGhost;
    public GameObject crosshair;
    bool dorjaKhulbe = false;
    public GameObject dorja1;
    public GameObject dorja2;

    // Start is called before the first frame update
    void Start()
    {
        timeManager = GetComponent<TimeManagerScript>();
        audioManager = GetComponentInChildren<AudioManager>();
        //deadEnemyChair.transform.GetComponent<Animator>().Play("Yard_Dead_Chair_Anim");
        //deadEnemyCar.transform.GetComponent<Animator>().Play("Yard_Dead_Car_Anim"); 
    }

    // Update is called once per frame
    void Update()
    {
        /*waypointDistance = Vector3.Distance(waypoint.position, player.transform.position);
        scoreText.text = score.ToString();

        if(score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", score);
        }

        if (Input.GetKey(KeyCode.P))
        {
            LoadScene();
        }

        //For Phase 1
        if(waypoint.position.x == -1.002145f)
        {
            if(waypointDistance < 1.5f && mainDoorApproached == false)
            {
                ApproachMainDoor();
            }
        }

        //For Video
        if (!firstEnemyFired)
        {
            FirstEnemyFired();
        }
        StateChange();
        if (dorjaKhulbe)
        {
            Dorja();
        }
        */
    }

    public void ActionButton(string button, bool anim)
    {
        actionButton.SetActive(true);
        actionButton.GetComponent<TextMeshProUGUI>().text = button;
        if (anim)
        {
            actionButton.GetComponent<Animator>().enabled = true;
        }
        else
        {
            actionButton.GetComponent<Animator>().enabled = false;
        }
    }
    public void ActionButtonOff()
    {
        actionButton.SetActive(false);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CameraAnim()
    {
        weapon.transform.parent = player.transform;
        mainCamera.GetComponentInChildren<MouseLook>().clampYEnable = true;
        mainCamera.GetComponentInChildren<MouseLook>().clampXRotaion = 10f;
        mainCamera.transform.parent = cameraBone.transform;
        Vector3 cameraBonePosition = new Vector3(0, 0, 0);
        Vector3 cameraBoneRotation = new Vector3(0, 180, 0);
        mainCamera.transform.localPosition = cameraBonePosition;
        mainCamera.transform.localRotation = Quaternion.Euler(cameraBoneRotation);
    }

    public void CameraAnimReset()
    {
        mainCamera.transform.parent = player.transform;
        mainCamera.transform.localPosition = new Vector3(0, 0.65f, 0);
        mainCamera.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        mainCamera.GetComponentInChildren<MouseLook>().clampYEnable = false;
        mainCamera.GetComponentInChildren<MouseLook>().clampXRotaion = 90f;
        weapon.transform.parent = mainCamera.GetComponentInChildren<MouseLook>().transform;

        //For Video

        if (tpp)
        {
            player.SetActive(false);
            playerTPP.SetActive(true);
            player = playerGhost;
            GetComponent<WayPointScript>().cam = cameraTPP;
            crosshair.SetActive(true);
        }
    }

    public void MovementOff()
    {
        playerController.weaponScript.transform.GetComponent<WeaponSight>().enabled = false;
        playerController.weaponScript.enabled = false;
        playerController.enabled = false;
        weaponSwitching.enabled = false;
    }

    public void MovementOn()
    {
        playerController.enabled = true;
        playerController.weaponScript.enabled = true;
        playerController.weaponScript.transform.GetComponent<WeaponSight>().enabled = true;
        weaponSwitching.enabled = true;
    }

    public void MahinStartAnim()
    {
        rabs[0].transform.GetComponent<Animator>().Play("Phase1_Mahin_StartAnim");
    }

    public void StartActComplete() 
    { 
        foreach(GameObject obj in rabs)
        {
            Phase_1 phaseScript = obj.transform.GetComponent<Phase_1>();
            obj.transform.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
            obj.transform.GetComponent<CharacterController>().enabled = true;
            obj.transform.GetComponent<BoxCollider>().enabled = true;
            phaseScript.waypoint.transform.position = phaseScript.gatePosition;
            phaseScript.waypoint.transform.rotation = Quaternion.Euler(phaseScript.gateRotation);
        }
    }

    public void GateOpenContinueCheck()
    {
        for (int i = 0; i < rabs.Length; i++)
        {
            if (!rabs[i].transform.GetComponent<Phase_1>().inPosition || waypointDistance > 3)
            {
                break;
            }
            gateOpenContinue = true;
        }
        if (gateOpenContinue)
        {
            foreach (GameObject obj in rabs)
            {
                obj.transform.GetComponent<Animator>().SetBool("Phase1_Gate", true);
            }
        }
    }

    void ApproachMainDoor()
    {
        waypoint.parent = rabs[0].transform;
        foreach (GameObject obj in rabs)
        {
            obj.transform.GetComponent<Animator>().SetBool("Phase1_GateIdle", false);
            obj.transform.GetComponent<Animator>().SetBool("Phase1_Gate", false);
            obj.transform.GetComponent<AiAgent>().AnimatorRootOff();
            Phase_1 phaseScript = obj.transform.GetComponent<Phase_1>();
            phaseScript.waypoint.transform.position = phaseScript.mainDoorPosition;
            phaseScript.waypoint.transform.rotation = Quaternion.Euler(phaseScript.mainDoorRotation);
        }
        waypoint.localPosition = new Vector3(0, 0, 0);
        mainDoorApproached = true;
        Invoke("D_Mahin_Movement", 1.0f);
    }

    public void MainDoorOpenContinueCheck()
    {
        for (int i = 0; i < rabs.Length; i++)
        {
            if (!rabs[i].transform.GetComponent<Phase_1>().inPosition || waypointDistance > 3)
            {
                break;
            }
            mainDoorOpenContinue = true;
        }
        if (mainDoorOpenContinue)
        {
            foreach (GameObject obj in rabs)
            {
                obj.transform.GetComponent<Animator>().SetBool("Phase1_MainDoor", true);
            }
        }
    }

    void D_Mahin_Movement()
    {
        //audioManager.Play("D_Movement");

        //Invoke("D_Mahin_ApproachingBuilding", 3.5f);
    }
    void D_Mahin_ApproachingBuilding()
    {
        audioManager.Play("D_ApproachingBuilding");
    }

    //For Video
    public void MaramariSuru()
    {
        maramariSuruHoise = true;
        firstEnemy.SetActive(true);
        firstEnemy.transform.GetComponent<Animator>().SetBool("PhaseMode", true);
        enemys.SetActive(true);
        Invoke("RPGEnemy", 10f);
        dorjaKhulbe = true;
    }
    void FirstEnemyFired()
    {
        if (firstEnemy.transform.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Phase_1_CoverFire"))
        {
            firstEnemyFired = true;
            moraRabta.transform.GetComponent<Animator>().Play("MorarAnimation");
            if(player.transform.GetComponent<Animator>() != null)
            {
                player.transform.GetComponent<Animator>().Play("Dodge_Bullet_Anim");
            }
            GoToDestination();
            foreach (GameObject obj in rabs)
            {
                if (obj.transform.GetComponent<Phase_1>().siriWala)
                {
                    obj.transform.GetComponent<Phase_1>().waypoint.transform.position = new Vector3(-5.3f, 0.1f, -26.5f);
                }
            }
        }
    }

    void GoToDestination()
    {
        foreach (GameObject obj in rabs)
        {
            Phase_1 phaseScript = obj.transform.GetComponent<Phase_1>();
            phaseScript.waypoint.transform.position = phaseScript.coverPosition;
        }
    }

    void StateChange()
    {
        foreach (GameObject obj in rabs)
        {
            Phase_1 phaseScript = obj.transform.GetComponent<Phase_1>();
            float coverDistance = Vector3.Distance(obj.transform.position, phaseScript.coverPosition);
            if(coverDistance < 0.5f)
            {
                obj.transform.GetComponent<AiAgent>().stateMachine.ChangeState(AiStateId.GoingCover);
            }
        }
    }

    void RPGEnemy()
    {
        rpgEnemy.SetActive(true);
    }

    void Dorja()
    {
        dorja1.transform.rotation = Quaternion.RotateTowards(dorja1.transform.rotation, Quaternion.Euler(0f, -125f, -90f), 100f * Time.deltaTime);
        dorja2.transform.rotation = Quaternion.RotateTowards(dorja2.transform.rotation, Quaternion.Euler(0f, 215f, 90f), 90f * Time.deltaTime);
    }
}
