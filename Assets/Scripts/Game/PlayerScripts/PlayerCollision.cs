using UnityEngine;
using Photon.Pun;

public class PlayerCollision : MonoBehaviourPun
{
    private bool isDead = false;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (isDead || !photonView.IsMine) return;

        if (collider.CompareTag("Obstacle"))
        {
            isDead = true;

            GameController gc = FindAnyObjectByType<GameController>();

            if (gc != null)
                gc.LocalPlayerDied();

            PhotonNetwork.Destroy(gameObject);
        }
    }
}