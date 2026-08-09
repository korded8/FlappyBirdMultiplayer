using UnityEngine;
using Photon.Pun;

public class SegmentMovement : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private float moveSpeed;
    private double spawnTime;
    private Vector3 startPosition;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        moveSpeed = (float)data[0];
        spawnTime = (double)data[1];

        startPosition = transform.position;
    }

    private void Update()
    {
        if (moveSpeed <= 0f)
            return;

        double elapsed = PhotonNetwork.Time - spawnTime;

        transform.position =
            startPosition +
            Vector3.left * (moveSpeed * (float)elapsed);
    }
}