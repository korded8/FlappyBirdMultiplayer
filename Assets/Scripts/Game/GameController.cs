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
    [SerializeField] private float victoryDelay = 3f;

    private bool gameStarted = false;
    private bool victoryShown = false;

    private readonly HashSet<Photon.Realtime.Player> deadPlayersHashSet = new();
    public bool localPlayerIsDead = false;

    public bool IsGameStarted()
    {
        return gameStarted;
    }

    private void Start()
    {
        PhotonNetwork.SerializationRate = 60;

        victoryText.gameObject.SetActive(false);

        if (!PhotonNetwork.IsMasterClient)
            startButton.SetActive(false);

        UpdatePlayerCount();
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        gameStarted = true;
        startButton.SetActive(false);

        photonView.RPC(nameof(EnablePhysicsRPC), RpcTarget.All);
    }

    [PunRPC]
    [System.Obsolete]
    private void EnablePhysicsRPC()
    {
        Rigidbody2D[] bodies = FindObjectsOfType<Rigidbody2D>();
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
                "ShowVictoryRPC",
                RpcTarget.AllBuffered,
                winnerActorNumber
            );
        }
    }

    private void UpdatePlayerCount()
    {
        playersText.text = $"Gracze: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    private void CheckVictory()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 1 && PhotonNetwork.PlayerList.Length > 1)
        {
            victoryShown = true;
            PhotonView pv = players[0].GetComponent<PhotonView>();
            pv.RPC(nameof(ShowVictoryRPC), pv.Owner);
        }
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

    private void ReturnToMenu()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
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

    public IEnumerator LeaveRoomCoroutine()
    {
        yield return null;

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}