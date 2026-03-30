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
        Vector3 dir = new(0, Random.value < 0.5f ? 1f : -1f);
        photonView.RPC(nameof(SyncObstacle), RpcTarget.All, p.ActorNumber, dir);
    }

    public void AssignPlayer()
    {
        player = null;
        obstacleUsed = true;
    }

    [PunRPC]
    void SyncObstacle(int actorNumber, Vector3 dir)
    {
        player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        direction = dir;
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
    }

    private void OnDestroy()
    {
        gameController.GhostObstacleDestroyed(player);
    }

    private void Update()
    {
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
