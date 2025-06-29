using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuScript : MonoBehaviour
{
    public TextMeshProUGUI highScore;
    public Animator animator;
    public GameObject controlPic;

    void Start()
    {
        highScore.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
    }

    public void Play()
    {
        animator.SetTrigger("FadeOut");
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }

    public void Controls()
    {
        controlPic.SetActive(true);
    }
    public void ControlsBack()
    {
        controlPic.SetActive(false);
    }
}
