using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class GhostObstacle : MonoBehaviourPun
{
    public float moveDistance = 2f;
    private Photon.Realtime.Player player;
    private bool obstacleUsed;
    private Vector3 direction;

    private GameController gameController;
    private GameObject upArrow;
    private GameObject downArrow;

    public void AssignPlayer(Photon.Realtime.Player p)
    {
        photonView.RPC(nameof(SyncPlayer), RpcTarget.All, p.ActorNumber);
    }

    public void AssignPlayer()
    {
        player = null;
        obstacleUsed = true;
    }

    [PunRPC]
    void SyncPlayer(int actorNumber)
    {
        player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        obstacleUsed = false;
    }

    [System.Obsolete]
    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        upArrow = transform.Find("UpArrow").gameObject;
        downArrow = transform.Find("DownArrow").gameObject;

        upArrow.SetActive(false);
        downArrow.SetActive(false);

        direction = Random.value < 0.5f ? Vector3.up : Vector3.down;
    }

    private void OnDestroy()
    {
        gameController.GhostObstacleDestroyed(player);
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryMove();
        }

        if (gameController.localPlayerIsDead && player == PhotonNetwork.LocalPlayer && !obstacleUsed)
        {
            if (direction == Vector3.up)
            {
                upArrow.SetActive(true);
                downArrow.SetActive(false);
            }
            else
            {
                upArrow.SetActive(false);
                downArrow.SetActive(true);
            }
        }
        else
        {
            upArrow.SetActive(false);
            downArrow.SetActive(false);
        }
    }

    private void TryMove()
    {
        int myID = PhotonNetwork.LocalPlayer.ActorNumber;

        if (player != null && myID == player.ActorNumber && !obstacleUsed && gameController.localPlayerIsDead)
        {
            photonView.RPC(nameof(MoveObstacle), RpcTarget.All);
        }
    }

    [PunRPC]
    void MoveObstacle()
    {
        transform.position += direction * moveDistance;
        obstacleUsed = true;
        gameController.AddDeadPlayer(player);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 maxPoint = new(transform.position.x, transform.position.y + moveDistance);
        Vector3 minPoint = new(transform.position.x, transform.position.y - moveDistance);

        Gizmos.DrawSphere(maxPoint, 0.2f);
        Gizmos.DrawSphere(minPoint, 0.2f);
    }
}
