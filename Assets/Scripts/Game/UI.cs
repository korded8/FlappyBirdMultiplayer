using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI : MonoBehaviour
{
    public TMP_Text scoreText;
    public GameObject escapePanel;

    public SegmentController sc;
    public GameController gc;

    private bool isAlive = true;
    private bool timerStarted = false;
    private bool isEscapePanelActive = false;
    private float scoreTimer = 0;
    private int score;

    public void Start()
    {
        escapePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isAlive
                || !gc.IsGameStarted()
                || gc.isLeavingRoom
                || (PhotonNetwork.PlayerList.Length == 1 && isEscapePanelActive))
            return;

        if (!timerStarted)
        {
            timerStarted = true;
        }

        UpdateScore();
        UpdateUI();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleEscapePanel();
        }
    }

    private void UpdateScore()
    {
        if ((scoreTimer += Time.deltaTime) >= 1)
        {
            score += (int)(gc.moveSpeed - 1);
            scoreTimer = 0;
        }
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void OnPlayerDied()
    {
        if (!isAlive) return;

        isAlive = false;

        UpdateHighscore();
    }

    private void UpdateHighscore()
    {
        int highscore = PlayerPrefs.GetInt("Highscore", 0);

        if (score > highscore)
        {
            PlayerPrefs.SetInt("Highscore", score);
            PlayerPrefs.Save();
        }
    }

    public void ToggleEscapePanel()
    {
        isEscapePanelActive = !isEscapePanelActive;

        escapePanel.SetActive(isEscapePanelActive);

        if (PhotonNetwork.PlayerList.Length == 1)
        {
            if (isEscapePanelActive)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }

    public void LeaveGame()
    {
        UpdateHighscore();
        ToggleEscapePanel();
        gc.LeaveRoom();
    }
}
