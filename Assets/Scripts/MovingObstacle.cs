using UnityEngine;
using Photon.Pun;

public class MovingObstacle : MonoBehaviourPun
{
    private float maxY;
    private float minY;
    private float moveSpeed = 3f;
    private Vector3 direction;

    private void Start()
    {
        maxY = transform.position.y + 3f;
        minY = transform.position.y - 3f;
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
}