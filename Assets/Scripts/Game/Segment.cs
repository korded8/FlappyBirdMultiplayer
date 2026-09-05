using UnityEngine;
using Photon.Pun;

public class Segment : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private SegmentController segmentController;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        segmentController = FindObjectOfType<SegmentController>();
    }

    private void Update()
    {
        double speed = segmentController.moveSpeed;

        transform.position += Vector3.left * (float)speed * Time.deltaTime;
    }
}