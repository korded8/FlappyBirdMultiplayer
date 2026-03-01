using UnityEngine;
using Photon.Pun;

public class PlayerCollision : MonoBehaviourPun
{
    private bool isDead = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;
        if (!photonView.IsMine) return;

        if (collision.CompareTag("Obstacle"))
        {
            isDead = true;

            // informujemy GameController
            GameController gc = FindObjectOfType<GameController>();
            if (gc != null)
            {
                gc.LocalPlayerDied();
            }

            // niszczymy gracza
            PhotonNetwork.Destroy(gameObject);
        }
    }
}