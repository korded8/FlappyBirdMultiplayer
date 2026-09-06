using UnityEngine;
using Photon.Pun;

public class Segment : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private GameController gameController;

    [System.Obsolete]
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        gameController = FindObjectOfType<GameController>();
    }

    private void Update()
    {
        double speed = gameController.moveSpeed;

        transform.position += Vector3.left * (float)speed * Time.deltaTime;
    }
}