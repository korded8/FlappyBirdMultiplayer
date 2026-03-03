using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSetup : MonoBehaviourPun
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        GameController gc = FindObjectOfType<GameController>();
        if (gc != null && !gc.IsGameStarted())
        {
            rb.simulated = false;
        }

        if (!photonView.IsMine)
        {
            rb.simulated = false;
        }
    }

    public void EnablePhysics()
    {
        if (rb != null)
            rb.simulated = true;
    }
}