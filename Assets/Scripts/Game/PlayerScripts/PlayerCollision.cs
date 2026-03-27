using UnityEngine;
using Photon.Pun;

public class PlayerCollision : MonoBehaviourPun
{
    private bool isDead = false;

    [System.Obsolete]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("HELLO WORLD!");
        if (isDead || !photonView.IsMine) return;

        if (collision.collider.CompareTag("Obstacle"))
        {
            isDead = true;

            GameController gc = FindObjectOfType<GameController>();

            if (gc != null)
                gc.LocalPlayerDied();

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
