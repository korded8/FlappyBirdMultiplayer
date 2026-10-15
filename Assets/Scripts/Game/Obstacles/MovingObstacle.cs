using UnityEngine;
using Photon.Pun;

public class MovingObstacle : MonoBehaviourPun
{
    private float maxY;
    private float minY;
    private readonly float moveSpeed = 3f;
    private readonly float distance = 2f;
    private Vector3 direction;

    private void Start()
    {
        maxY = transform.position.y + distance;
        minY = transform.position.y - distance;

        if(PhotonNetwork.IsMasterClient)
        {
            direction = Random.value > 0.5f ? Vector3.up : Vector3.down;
            photonView.RPC(nameof(SyncDirection), RpcTarget.All, direction);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        Move();
    }

    private void Move()
    {
        if (transform.position.y > maxY)
        {
            direction = Vector3.down;
        }
        else if (transform.position.y < minY)
        {
            direction = Vector3.up;
        }

        transform.position += moveSpeed * Time.deltaTime * direction;
    }

    [PunRPC]
    private void SyncDirection(Vector3 dir)
    {
        direction = dir;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3 maxPoint = new(transform.position.x, transform.position.y + distance);
        Gizmos.DrawSphere(maxPoint, 0.2f);

        Vector3 minPoint = new(transform.position.x, transform.position.y - distance);
        Gizmos.DrawSphere(minPoint, 0.2f);

        Gizmos.DrawLine(minPoint, maxPoint);
    }
}
