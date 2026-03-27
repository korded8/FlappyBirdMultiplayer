using UnityEngine;
using Photon.Pun;

public class PlayerCollision : MonoBehaviourPun
{
    private bool isDead = false;

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (isDead || !photonView.IsMine) return;

        if (collider.CompareTag("Obstacle"))
        {
            isDead = true;

            GameController gc = FindObjectOfType<GameController>();

            if (gc != null)
                gc.LocalPlayerDied();

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
