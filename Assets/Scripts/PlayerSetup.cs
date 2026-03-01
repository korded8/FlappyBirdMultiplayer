using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSetup : MonoBehaviourPun
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Wy³¹cz fizykê od razu po spawnieniu, jeœli gra nie wystartowa³a
        GameController gc = FindObjectOfType<GameController>();
        if (gc != null && !gc.IsGameStarted())
        {
            rb.simulated = false;
        }

        // Jeœli to nie jest nasz lokalny gracz, nie sterujemy Rigidbody
        if (!photonView.IsMine)
        {
            rb.simulated = false;
        }
    }

    // Funkcja do w³¹czania fizyki po starcie gry
    public void EnablePhysics()
    {
        if (rb != null)
            rb.simulated = true;
    }
}