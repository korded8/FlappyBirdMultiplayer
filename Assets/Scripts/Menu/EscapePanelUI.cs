using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapePanelUI : MonoBehaviour
{
    public GameObject escapePanel;

    private bool isEscapePanelActive = false;

    private void Start()
    {
        escapePanel.SetActive(false);
    }

    public void ToggleEscapePanel()
    {
        isEscapePanelActive = !isEscapePanelActive;

        escapePanel.SetActive(isEscapePanelActive);
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