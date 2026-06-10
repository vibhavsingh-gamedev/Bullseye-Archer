using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFaderController : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;

    void Start()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float timer = fadeDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            canvasGroup.alpha = timer / fadeDuration;
            yield return null;
        }
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeOut(string sceneName)
    {
        canvasGroup.blocksRaycasts = true;
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = timer / fadeDuration;
            yield return null;
        }
        canvasGroup.alpha = 1;
        SceneManager.LoadScene(sceneName);
    }
}