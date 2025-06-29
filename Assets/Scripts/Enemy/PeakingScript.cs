using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeakingScript : MonoBehaviour
{
    [SerializeField] private Vector2Int coveringTime_Min_Max = new Vector2Int(1, 6);
    [SerializeField] private Vector2Int peakingTime_Min_Max = new Vector2Int(1, 3);

    private AiAgent agent;
    private int coverTimeChoice;
    private int peakChoice;
    private int peakTimeChoice;
    private bool calculating = false;
    float peakingTime = 0f;
    bool isPeaking = false;
    int peakDuration = 0;
    int pauseDuration = 0;

    void Start()
    {
        agent = GetComponent<AiAgent>();
        agent.FireOff();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.LogError("Here bsdk!");
        HandlePeak();
        //StartCoroutine(PeakingState());
    }

    void HandlePeak()
    {
        peakingTime += Time.deltaTime;
        if (isPeaking)
        {
            if (!agent.isFiring)
            {
                agent.animator.SetBool("coverFire1", true);
                agent.FireOn();
            }

            if (peakingTime >= peakDuration)
            {
                isPeaking = false;
                peakingTime = 0f;
                pauseDuration = Random.Range(coveringTime_Min_Max.x, coveringTime_Min_Max.y);
                if (agent.isFiring)
                {
                    agent.FireOff();
                    agent.animator.SetBool("coverFire1", false);
                }
            }
        }
        else
        {
            if (peakingTime >= pauseDuration)
            {
                isPeaking = true;
                peakingTime = 0f;
                peakDuration = Random.Range(peakingTime_Min_Max.x, peakingTime_Min_Max.y);
            }
        }
    }

    IEnumerator PeakingState()
    {
        calculating = true;
        agent.FireOff();

        coverTimeChoice = Random.Range(coveringTime_Min_Max.x, coveringTime_Min_Max.y);
        yield return new WaitForSeconds(coverTimeChoice);

        peakChoice = Random.Range(1, 3);
        peakTimeChoice = Random.Range(peakingTime_Min_Max.x, peakingTime_Min_Max.y);
        if (peakChoice == 1)
        {
            agent.animator.SetBool("coverFire1", true);
            yield return new WaitForSeconds(0.5f);
            if (!agent.deathState.isDead)
            {
                agent.FireOn();
            }
            yield return new WaitForSeconds(peakTimeChoice);
            agent.FireOff();
            agent.animator.SetBool("coverFire1", false);

        }
        else if (peakChoice >= 2)
        {
            agent.animator.SetBool("coverFire2", true);
            yield return new WaitForSeconds(0.5f);
            if (!agent.deathState.isDead)
            {
                agent.FireOn();
            }
            yield return new WaitForSeconds(peakTimeChoice);
            agent.FireOff();
            agent.animator.SetBool("coverFire2", false);
        }
        calculating = false;
    }
}
