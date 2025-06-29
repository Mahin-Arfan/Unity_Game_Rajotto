using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class TimeManagerScript : MonoBehaviour
{
    [Range(0, 2)]
    [Tooltip("1 is Normal Speed")]
    public float slowDownFactor = 0.5f;
    public float transitionSpeed = 0.5f; // Smooth transition duration
    [Range(0.5f, 2)]
    [Tooltip("1 is Normal Speed")]
    private float slowPitch = 0.5f; // Adjust as needed

    private float normalTimeScale = 1f;
    private float normalPitch = 1f;

    private AudioMixer audioMixer;
    private GameManagerScript gameManager;

    void Start()
    {
        gameManager = GetComponent<GameManagerScript>();
        if(gameManager == null)
        {
            Debug.LogError("GameManagerScript Not Found!");
        }
        audioMixer = gameManager.audioMixer;
        if (audioMixer == null)
        {
            Debug.LogError("AudioMixer Not Found!");
        }
    }

    public void DoSlowMotion()
    {
        StopAllCoroutines();
        gameManager.audioManager.Play("SlowMotionOn");
        StartCoroutine(SmoothTimeChange(slowDownFactor, slowPitch, transitionSpeed));
    }

    public void ResetTime()
    {
        StopAllCoroutines();
        StartCoroutine(SmoothTimeChange(normalTimeScale, normalPitch, transitionSpeed));
    }

    private IEnumerator SmoothTimeChange(float targetTimeScale, float targetPitch, float duration)
    {
        float startTimeScale = Time.timeScale;
        float startPitch;
        audioMixer.GetFloat("SFXPitch", out startPitch);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            Time.timeScale = Mathf.Lerp(startTimeScale, targetTimeScale, t);
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            audioMixer.SetFloat("SFXPitch", Mathf.Lerp(startPitch, targetPitch, t));
            audioMixer.SetFloat("DialoguePitch", Mathf.Lerp(startPitch, targetPitch, t));

            yield return null;
        }

        //Ensuring final values are set
        Time.timeScale = targetTimeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        audioMixer.SetFloat("SFXPitch", targetPitch);
        audioMixer.SetFloat("DialoguePitch", targetPitch);
    }
}
