using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameManagerScript gameManagerScript;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MovementOff()
    {
        gameManagerScript.MovementOff();
    }
    void MovementOn()
    {
        gameManagerScript.MovementOn();
    }

    void CameraAnim()
    {
        gameManagerScript.CameraAnim();
    }

    void CameraAnimReset()
    {
        gameManagerScript.CameraAnimReset();
    }

    void WeaponSpectate()
    {
        gameManagerScript.playerController.weaponScript.GetComponent<Animator>().Play("Phase1");
    }

    void Phase1Start()
    {
        gameManagerScript.MahinStartAnim();
    }
}
