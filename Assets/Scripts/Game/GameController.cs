using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float victoryDelay = 3f;

    private bool gameStarted = false;
    private bool victoryShown = false;

    private readonly HashSet<Photon.Realtime.Player> deadPlayersHashSet = new();
    public bool localPlayerIsDead = false;

    private int secondsUntilStart = 3;

    public bool IsGameStarted()
    {
        return gameStarted;
    }

    private void Start()
    {
        PhotonNetwork.SerializationRate = 20;

        victoryText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        if (!PhotonNetwork.IsMasterClient)
            startButton.SetActive(false);

        UpdatePlayerCount();
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
        if (PhotonNetwork.LocalPlayer.ActorNumber == winnerActorNumber)
        {
            victoryText.gameObject.SetActive(true);
            StartCoroutine(ReturnToMenuAfterWin());
        }
    }

    private IEnumerator ReturnToMenuAfterWin()
    {
        yield return new WaitForSeconds(victoryDelay);

        PhotonNetwork.LeaveRoom();
    }

    [System.Obsolete]
    public void LocalPlayerDied()
    {
        photonView.RPC(nameof(SyncDeadPlayersHashSet), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        if (FindObjectOfType<UI>() != null)
            FindObjectOfType<UI>().OnPlayerDied();
        localPlayerIsDead = true;
    }

    [PunRPC]
    private void SyncDeadPlayersHashSet(int actorNumber)
    {
        Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

        if (player != null)
        {
            deadPlayersHashSet.Add(player);
        }
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
        photonView.RPC(nameof(SyncDeadPlayersHashSet), RpcTarget.MasterClient, player.ActorNumber);
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
