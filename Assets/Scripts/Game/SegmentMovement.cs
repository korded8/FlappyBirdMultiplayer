using UnityEngine;
using Photon.Pun;

public class SegmentMovement : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private float initialSpeed;
    private double spawnTime;
    private Vector3 startPosition;
    private SegmentController segmentController;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = photonView.InstantiationData;

        initialSpeed = (float)data[0];
        spawnTime = (double)data[1];

        startPosition = transform.position;
        segmentController = FindObjectOfType<SegmentController>();
    }

    private void Update()
    {
        double speed = segmentController.moveSpeed;

        transform.position += Vector3.left * (float)speed * Time.deltaTime;
    }
}