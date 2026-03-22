using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    private string sceneToLoad = "Menu";

    public Animator logoAnimator;
    private string animationStateName = "LogoAnimation";

    private void Start()
    {
        StartCoroutine(LoadSceneAdditive());
    }

    private IEnumerator LoadSceneAdditive()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        operation.allowSceneActivation = false;

        if (logoAnimator != null)
        {
            yield return null;

            AnimatorStateInfo state = logoAnimator.GetCurrentAnimatorStateInfo(0);
            float animationLength = state.length;

            yield return new WaitForSeconds(animationLength);
        }

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        operation.allowSceneActivation = true;
        yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(sceneToLoad);
        SceneManager.SetActiveScene(loadedScene);

        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}