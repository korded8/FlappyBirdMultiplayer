using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static System.TimeZoneInfo;

public class GhostObstacle : MonoBehaviourPun
{
    [SerializeField] private float tpTime = 0.5f;
    public float moveDistance = 2f;
    private Photon.Realtime.Player player;
    private bool obstacleUsed;
    private Vector3 direction;

    private GameController gameController;
    private GameObject upArrow;
    private GameObject downArrow;

    public void AssignPlayer(Photon.Realtime.Player p)
    {
        if (!PhotonNetwork.InRoom || gameController == null || gameController.isLeavingRoom)
        {
            player = p;
            obstacleUsed = true;
            return;
        }

        Vector3 dir = new Vector3(0f, Random.value < 0.5f ? 1f : -1f);
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
        if (!PhotonNetwork.InRoom || gameController == null || gameController.isLeavingRoom) return;

        player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        direction = dir;
        obstacleUsed = false;
    }

    [System.Obsolete]
    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        upArrow = transform.Find("UpArrow")?.gameObject;
        downArrow = transform.Find("DownArrow")?.gameObject;

        if (upArrow != null) upArrow.SetActive(false);
        if (downArrow != null) downArrow.SetActive(false);
    }

    private void OnDestroy()
    {
        gameController?.GhostObstacleDestroyed(player);
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryMove();
        }

        if (gameController != null && gameController.localPlayerIsDead && player == PhotonNetwork.LocalPlayer && !obstacleUsed)
        {
            if (direction == Vector3.up)
            {
                if (upArrow != null) upArrow.SetActive(true);
                if (downArrow != null) downArrow.SetActive(false);
            }
            else
            {
                if (upArrow != null) upArrow.SetActive(false);
                if (downArrow != null) downArrow.SetActive(true);
            }
        }
        else
        {
            if (upArrow != null) upArrow.SetActive(false);
            if (downArrow != null) downArrow.SetActive(false);
        }
    }

    private void TryMove()
    {
        if (!PhotonNetwork.InRoom || gameController == null || gameController.isLeavingRoom) return;

        int myID = PhotonNetwork.LocalPlayer.ActorNumber;

        if (player != null && myID == player.ActorNumber && !obstacleUsed && gameController.localPlayerIsDead)
        {
            photonView.RPC(nameof(MoveObstacle), RpcTarget.All);
        }
    }

    [PunRPC]
    private void MoveObstacle()
    {
        if (!PhotonNetwork.InRoom || gameController == null || gameController.isLeavingRoom) return;

        StartCoroutine(MoveObstacleCoroutine());
    }

    private IEnumerator MoveObstacleCoroutine()
    {
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsed < tpTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tpTime);

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            yield return null;
        }

        elapsed = 0f;
        transform.position += direction * moveDistance;
        obstacleUsed = true;

        while (elapsed < tpTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tpTime);

            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 maxPoint = new Vector3(transform.position.x, transform.position.y + moveDistance);
        Vector3 minPoint = new Vector3(transform.position.x, transform.position.y - moveDistance);

        Gizmos.DrawSphere(maxPoint, 0.2f);
        Gizmos.DrawSphere(minPoint, 0.2f);
    }
}