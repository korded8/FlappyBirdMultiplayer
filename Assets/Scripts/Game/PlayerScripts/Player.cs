using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviourPun
{
    private Rigidbody2D rb;
    private float jumpForce = 3f;
    private readonly float maxRotation = 45f;

    [System.Obsolete]
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            photonView.RPC(nameof(Jump), RpcTarget.All);
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        float targetZ = Mathf.Clamp(rb.linearVelocity.y * 15f, -maxRotation, maxRotation);
        float smoothZ = Mathf.LerpAngle(rb.rotation, targetZ, Time.fixedDeltaTime * 10f);
        rb.MoveRotation(smoothZ);
    }
    public void EnablePhysics()
    {
        if (rb != null)
            rb.simulated = true;
    }

    [PunRPC]
    private void Jump()
    {
        rb.linearVelocity = Vector2.up * jumpForce;
    }
}