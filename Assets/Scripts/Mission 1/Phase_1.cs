using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase_1 : MonoBehaviour
{
    private AiAgent agent;
    public GameManagerScript gameManagerScript;
    private AudioManager audioManager;

    [Header("Position Managements")]
    public bool inPosition = false;
    public GameObject waypoint;

    [Header("Gun Equip")]
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject gunBelt;
    [SerializeField] private GameObject gunHand;
    [SerializeField] private Vector3 equipedGunPosition;
    [SerializeField] private Vector3 equipedGunRotation;
    [SerializeField] private Vector3 holsterGunPosition;
    [SerializeField] private Vector3 holsterGunRotation;

    [Header("GateActions")]
    [SerializeField] private GameObject gateLock;
    [SerializeField] private GameObject cutter;
    [SerializeField] private GameObject gate;

    [Header("MainDoor")]
    [SerializeField] private GameObject ledder;
    [SerializeField] private GameObject door;

    [Header("Animation Position")]
    public Vector3 gatePosition;
    public Vector3 gateRotation;
    public Vector3 yardWaitPosition;
    public Vector3 mainDoorPosition;
    public Vector3 mainDoorRotation;

    [Header("For Video")]
    public bool siriWala = false;
    public bool moraRab = false;
    public Vector3 coverPosition;
    public bool enemy = false;
    public bool enemyCovered = false;
    public bool manualTarget = false;
    public bool rpg = false;
    public GameObject rpgRocket;

    void Start()
    {
        agent = transform.GetComponent<AiAgent>();
        audioManager = gameManagerScript.GetComponentInChildren<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(inPosition && transform.position.x == gatePosition.x)
        {
            GetComponent<Animator>().SetBool("Phase1_GateIdle", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("Phase1_GateIdle", false);
        }

        if (inPosition && transform.position.x == mainDoorPosition.x)
        {
            GetComponent<Animator>().SetBool("Phase1_MainDoorIdle", true);
        }

        //For Video
        if (moraRab && transform.position.z > -20f && !gameManagerScript.maramariSuruHoise)
        {
            gameManagerScript.MaramariSuru();
        }
        if (enemy == true && !enemyCovered)
        {
            EnemyGoingCover();
        }
    }

    void GunUnequip()
    {
        if(gun!=null)
        {
            gun.transform.parent = gunBelt.transform;
            gun.transform.localPosition = holsterGunPosition;
            gun.transform.localRotation = Quaternion.Euler(holsterGunRotation);
        }
    }

    void GunEquip()
    {
        if (gun != null)
        {
            gun.transform.parent = gunHand.transform;
            gun.transform.localPosition = equipedGunPosition;
            gun.transform.localRotation = Quaternion.Euler(equipedGunRotation);
        }
    }

    void CutterEquip()
    {
        Vector3 cutterPosition = new Vector3(0.00143f, 0.0037f, -0.00083f);
        Vector3 cutterRotation = new Vector3(-12.61f, -12.306f, 23.115f);
        Vector3 cutterSpinePosition = new Vector3(0.002398366f, -0.000931986f, -0.0002204367f);
        Vector3 cutterSpineRotation = new Vector3(-174.417f, -90.90698f, 99.041f);
        if (cutter != null)
        {
            if(cutter.transform.parent == gunBelt.transform)
            {
                cutter.transform.parent = gunHand.transform;
                cutter.GetComponent<Animator>().enabled = true;
                cutter.GetComponent<Animator>().Play("Bolt Cutter_Action"); 
                cutter.transform.localPosition = cutterPosition; 
                cutter.transform.localRotation = Quaternion.Euler(cutterRotation);
            }
            else
            {
                cutter.transform.parent = gunBelt.transform;
                cutter.transform.localPosition = cutterSpinePosition;
                cutter.transform.localRotation = Quaternion.Euler(cutterSpineRotation);
                cutter.GetComponent<Animator>().Play("Bolt Cutter_Idle");
            }
        }
    }

    void GateLockBreak()
    {
        if(gateLock != null)
        {
            gateLock.GetComponent<Animator>().enabled = true;
        }
    }

    void GateOpen()
    {
        if (gate != null)
        {
            gate.GetComponent<Animator>().enabled = true;
        }
    }

    void LedderUnequip()
    {
        if (ledder != null)
        {
            ledder.transform.parent = null;
        }
    }

    void LedderAnim()
    {
        if(ledder != null)
        {
            ledder.transform.GetComponent<Animator>().enabled = true;
        }
        gameManagerScript.rabs[3].transform.GetComponent<Animator>().SetBool("Phase1_MainDoor", true);
    }

    void GatePositions()
    {
        transform.localPosition = new Vector3(gatePosition.x, transform.localPosition.y, gatePosition.z);
        transform.localRotation = Quaternion.Euler(gateRotation.x, gateRotation.y, gateRotation.z);
    }

    void StartAnimComplete()
    {
        gameManagerScript.StartActComplete();
    }

    void GateOpenContinueCheck()
    {
        gameManagerScript.GateOpenContinueCheck();
    }

    void YardWaitPosition()
    {
        waypoint.transform.position = yardWaitPosition;
        GetComponent<Animator>().SetBool("Phase1_Gate", false);
        GetComponent<Animator>().SetBool("Phase1_GateIdle", false);
        gameManagerScript.waypoint.parent = null;
        gameManagerScript.waypoint.localPosition = new Vector3(-1.002145f, 0.5f, -24.16363f);
    }

    void MainDoorOpenContinueCheck()
    {
        gameManagerScript.MainDoorOpenContinueCheck();
    }

    void MainDoorPosition()
    {
        transform.localPosition = mainDoorPosition;
        transform.localRotation = Quaternion.Euler(mainDoorRotation);
    }

    void MainDoorAnim()
    {
        if(door != null)
        {
            door.GetComponent<Animator>().enabled = true;
        }
    }

    //Dialogues
    void D_Mahin_LetsGo()
    {
        audioManager.Play("D_LetsGo");
    }

    void D_Mahin_BreakIt()
    {
        audioManager.Play("D_BreakIt");
    }

    void EnemyGoingCover()
    {
        float coverDistance = Vector3.Distance(transform.position, waypoint.transform.position);
        if (coverDistance < 1f)
        {
            if (manualTarget)
            {
                ManualTargetSet();
            }
            else if (rpg)
            {
                RPGMan();
            }
            else
            {
                transform.GetComponent<AiAgent>().stateMachine.ChangeState(AiStateId.GoingCover);
            }
            enemyCovered = true;
        }
    }

    public void ManualTargetSet()
    {
        transform.GetComponent<AiAgent>().stateMachine.ChangeState(AiStateId.Phase);
        transform.GetComponent<Animator>().SetBool("CoverIdle", true);
        transform.GetComponent<PeakingScript>().enabled = true;
    }

    void RPGMan()
    {
        transform.GetComponent<AiAgent>().stateMachine.ChangeState(AiStateId.Phase);
        transform.GetComponent<Animator>().Play("RPG_Fire_Anim");
    }

    public void RPGFire()
    {
        GameObject rpgFiredRocket = Instantiate(rpgRocket, agent.weaponIk.aimTransform.position, Quaternion.identity);
        rpgFiredRocket.transform.GetComponent<RocketScript>().target = gameManagerScript.player.transform;
    }
}
