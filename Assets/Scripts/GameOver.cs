using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void EndGame()
    {
        Debug.Log("GameOver");
        Invoke("Restart", 5f);
    }

    void Restart()
    {
        GetComponent<Animator>().SetTrigger("FadeOut");
    }
}
