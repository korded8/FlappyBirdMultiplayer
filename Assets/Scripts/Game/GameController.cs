using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private float victoryDelay = 3f;

    private bool gameStarted = false;
    private bool victoryShown = false;

    private Queue<Photon.Realtime.Player> deadPlayersQueue = new Queue<Photon.Realtime.Player>();
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

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!gameStarted)
            return;

        if (victoryShown)
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

    public void LocalPlayerDied()
    {
        AddDeadPlayer(PhotonNetwork.LocalPlayer);
        FindObjectOfType<UI>()?.OnPlayerDied();
        localPlayerIsDead = true;
    }

    public void AddDeadPlayer(Photon.Realtime.Player player)
    {
        if (!deadPlayersQueue.Contains(player))
            deadPlayersQueue.Enqueue(player);
    }


    public void AssignPlayerToGhostObstacle(GhostObstacle obstacle)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Photon.Realtime.Player player = deadPlayersQueue.Count > 0 ? deadPlayersQueue.Dequeue() : null;

        if (player != null)
        {
            obstacle.AssignPlayer(player);
        }
        else
        {
            obstacle.AssignNoPlayer();
        }
    }

    public System.Collections.IEnumerator LeaveRoomCoroutine()
    {
        yield return null;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
