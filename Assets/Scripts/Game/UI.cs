using Photon.Pun;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    public TMP_Text scoreText;
    public GameObject escapePanel;

    public SegmentController sc;
    public GameController gc;

    private double startTime;
    private double pausedTime = 0;
    private double pauseStartTime = 0;

    private bool isAlive = true;
    private bool timerStarted = false;
    private bool isEscapePanelActive = false;
    private bool isLeavingGame = false;

    public void Start()
    {
        escapePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isAlive) return;
        if (!gc.IsGameStarted()) return;
        if (isLeavingGame) return;
        if (PhotonNetwork.PlayerList.Length == 1 && isEscapePanelActive) return;
        if (!timerStarted)
        {
            startTime = PhotonNetwork.Time;
            timerStarted = true;
        }

        UpdateUI();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleEscapePanel();
        }
    }

    void UpdateUI()
    {
        int score = GetScore();
        scoreText.text = "Score: " + score.ToString();
    }

    int GetScore()
    {
        double elapsedTime = PhotonNetwork.Time - startTime - pausedTime;
        return Mathf.FloorToInt((float)elapsedTime) * (int)sc.moveSpeed;
    }

    public void OnPlayerDied()
    {
        if (!isAlive) return;

        isAlive = false;

        SetFinalScore();
    }

    private void SetFinalScore()
    {
        int finalScore = GetScore();

        int highscore = PlayerPrefs.GetInt("Highscore", 0);

        if (finalScore > highscore)
        {
            PlayerPrefs.SetInt("Highscore", finalScore);
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
                pauseStartTime = PhotonNetwork.Time;
            }
            else
            {
                Time.timeScale = 1f;
                pausedTime += PhotonNetwork.Time - pauseStartTime;
            }
        }
    }

    public void LeaveGame()
    {
        SetFinalScore();
        isLeavingGame = true;
        ToggleEscapePanel();
        StartCoroutine(gc.LeaveRoomCoroutine());
    }
}