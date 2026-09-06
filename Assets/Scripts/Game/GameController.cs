using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private TMP_Text lossText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float victoryDelay = 3f;
    [SerializeField] private GameObject background;

    private bool gameStarted = false;
    private bool victoryShown = false;
    private double gameStartTime = 0;
    public float maxSpeed = 2f;
    private Color currentBackgroundColor;
    private int transisionTime = 3;

    private readonly HashSet<Photon.Realtime.Player> deadPlayersHashSet = new();
    public bool localPlayerIsDead = false;

    private int secondsUntilStart = 3;

    public enum DifficultyLevel
    {
        Easy, Accelerated, Medium, Hard
    }
    public DifficultyLevel currDiffLvl = DifficultyLevel.Easy;
    private DifficultyLevel previousDiffLvl;

    public bool IsGameStarted()
    {
        return gameStarted;
    }

    private void Start()
    {
        PhotonNetwork.SerializationRate = 15;

        victoryText.gameObject.SetActive(false);
        lossText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        currentBackgroundColor = new Color(0.1882353f, 0.4901961f, 0.9333334f);
        Camera.main.backgroundColor = currentBackgroundColor;

        if (!PhotonNetwork.IsMasterClient)
            startButton.SetActive(false);
        else
        {
            currDiffLvl = DifficultyLevel.Easy;
            maxSpeed = 2f;
        }

            UpdatePlayerCount();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !gameStarted) return;

        double elapsed = PhotonNetwork.Time - gameStartTime;

        if (elapsed < 30)
        {
            currDiffLvl = DifficultyLevel.Easy;
            maxSpeed = 2f;
        }
        else if (elapsed < 60)
        {
            currDiffLvl = DifficultyLevel.Accelerated;
            maxSpeed = 3f;
        }
        else if (elapsed < 90)
        {
            currDiffLvl = DifficultyLevel.Medium;
            maxSpeed = 3.5f;
        }
        else
        {
            currDiffLvl = DifficultyLevel.Hard;
            maxSpeed = 4f;
        }

        if (currDiffLvl != previousDiffLvl)
        {
            previousDiffLvl = currDiffLvl;

            switch (currDiffLvl)
            {
                case DifficultyLevel.Accelerated:
                    StartCoroutine(TransitionColor(
                        new Color(0.09803922f, 0.09803922f, 0.723f)
                    ));
                    break;

                case DifficultyLevel.Medium:
                    StartCoroutine(TransitionColor(
                        new Color(0.735849f, 0.3328245f, 0f)
                    ));
                    break;

                case DifficultyLevel.Hard:
                    StartCoroutine(TransitionColor(
                        new Color(0.04313726f, 0.04313726f, 0.04313726f)
                    ));
                    break;
            }
        }
    }

    IEnumerator TransitionColor(Color nextColor)
    {
        float elapsed = 0f;

        while (elapsed < transisionTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / transisionTime);

            Camera.main.backgroundColor = Color.Lerp(
                currentBackgroundColor,
                nextColor,
                t
            );

            yield return null;
        }

        currentBackgroundColor = nextColor;
    }

    [PunRPC]
    public void SetCountdownRPC(int secondsUntilStart)
    {
        countdownText.gameObject.SetActive(true);
        if (secondsUntilStart > 0)
            countdownText.text = secondsUntilStart.ToString();
        else
            countdownText.text = "GO!";
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        gameStartTime = PhotonNetwork.Time;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        startButton.SetActive(false);
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        startButton.SetActive(false);

        while (secondsUntilStart >= 0)
        {
            photonView.RPC(nameof(SetCountdownRPC), RpcTarget.All, secondsUntilStart);
            secondsUntilStart--;

            WaitForSeconds wait = new(1);
            yield return wait;
        }

        gameStarted = true;

        countdownText.gameObject.SetActive(false);
        photonView.RPC(nameof(OnGameStartedRPC), RpcTarget.All);
        yield return null;
    }

    [PunRPC]
    private void OnGameStartedRPC()
    {
        EnablePhysics();
        countdownText.enabled = false;
    }

    private void EnablePhysics()
    {
        Rigidbody2D[] bodies = FindObjectsByType<Rigidbody2D>();
        foreach (var rb in bodies)
            rb.simulated = true;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerCount();

        if (!PhotonNetwork.IsMasterClient || !gameStarted || victoryShown)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            victoryShown = true;
            int winnerActorNumber = -1;

            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                winnerActorNumber = player.ActorNumber;
                break;
            }

            photonView.RPC(
                nameof(ShowVictoryRPC),
                RpcTarget.AllBuffered,
                winnerActorNumber
            );
        }
    }

    private void UpdatePlayerCount()
    {
        playersText.text = $"Gracze: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    [PunRPC]
    private void ShowVictoryRPC(int winnerActorNumber)
    {
        victoryShown = true;

        if (PhotonNetwork.LocalPlayer.ActorNumber == winnerActorNumber)
            victoryText.gameObject.SetActive(true);
        else
            lossText.gameObject.SetActive(true);

        StartCoroutine(ReturnToMenuAfterWin());
    }

    private IEnumerator ReturnToMenuAfterWin()
    {
        yield return new WaitForSeconds(victoryDelay);

        //PhotonNetwork.LeaveRoom();
    }

    [System.Obsolete]
    public void LocalPlayerDied()
    {
        photonView.RPC(nameof(UpdateDeadPlayersRPC), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        if (FindObjectOfType<UI>() != null)
            FindObjectOfType<UI>().OnPlayerDied();
        localPlayerIsDead = true;
    }

    [PunRPC]
    private void UpdateDeadPlayersRPC(int actorNumber)
    {
        Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        if (player != null)
            deadPlayersHashSet.Add(player);

        CheckForWinners();
    }

    private void CheckForWinners()
    {
        if (victoryShown) return;

        Photon.Realtime.Player winner = null;
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!deadPlayersHashSet.Contains(player))
            {
                if (winner != null) return; // at least 2 players standing
                winner = player;
            }
        }

        photonView.RPC(
            nameof(ShowVictoryRPC),
            RpcTarget.AllBuffered,
            winner != null ? winner.ActorNumber : -1
        );
    }

    public void AssignPlayerToGhostObstacle(GhostObstacle obstacle)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Photon.Realtime.Player player = deadPlayersHashSet.FirstOrDefault();

        if (player != null)
        {
            deadPlayersHashSet.Remove(player);
            obstacle.AssignPlayer(player);
        }
        else
            obstacle.AssignPlayer();
    }

    public void GhostObstacleDestroyed(Photon.Realtime.Player player)
    {
        if (player == null) return;
        photonView.RPC(nameof(UpdateDeadPlayersRPC), RpcTarget.MasterClient, player.ActorNumber);
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}