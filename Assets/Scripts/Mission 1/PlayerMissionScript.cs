using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissionScript : MonoBehaviour
{
    [SerializeField] private GameObject gameManager;
    [SerializeField] private AudioManager audioManager;

    [SerializeField] private GameObject m4A1;
    bool m4AudioEnabled = false;

    void Start()
    {
        audioManager = gameManager.GetComponentInChildren<AudioManager>();
    }

    void Update()
    {
        if(m4A1.transform.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Phase1") && m4AudioEnabled == false)
        {
            StartCoroutine(M4ReadySound());
        }
    }

    IEnumerator M4ReadySound()
    {
        audioManager.Play("M4 Ready");
        m4AudioEnabled = true;
        yield return new WaitForSeconds(2);
        audioManager.Play("D_road clear");
    }
}
