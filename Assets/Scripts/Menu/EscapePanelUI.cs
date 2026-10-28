using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapePanelUI : MonoBehaviour
{
    public GameObject escapePanel;

    private void Start()
    {
        escapePanel.SetActive(false);
    }

    public void ToggleEscapePanel()
    {
        escapePanel.SetActive(!escapePanel.activeInHierarchy);
    }

    public void RepoButton()
    {
        Application.OpenURL("https://github.com/korded8/FlappyBirdMultiplayer/tree/maybe-this-will-work");
    }

    public void CredtisButton()
    {
        SceneManager.LoadScene("Credits");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}