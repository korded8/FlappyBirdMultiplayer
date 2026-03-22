using UnityEngine;
using Photon.Pun;

public class MovingObstacle : MonoBehaviourPun
{
    private float maxY;
    private float minY;
    private float moveSpeed = 3f;
    private float distance = 2f;
    private Vector3 direction;

    private void Start()
    {
        maxY = transform.position.y + distance;
        minY = transform.position.y - distance;

        direction = Random.value > 0.5f ? Vector3.up : Vector3.down;
    }
    private void Update()
    {
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(Move), RpcTarget.All);
    }

    [PunRPC]
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
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3 maxPoint = new Vector3(transform.position.x, transform.position.y + distance);
        Gizmos.DrawSphere(maxPoint, 0.2f);

        Vector3 minPoint = new Vector3(transform.position.x, transform.position.y - distance);
        Gizmos.DrawSphere(minPoint, 0.2f);

        Gizmos.DrawLine(minPoint, maxPoint);
    }
}