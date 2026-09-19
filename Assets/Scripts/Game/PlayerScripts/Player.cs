using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviourPun
{
    [Header("Settings")]
    [SerializeField] private float jumpForce = 6f;

    private Rigidbody2D rb;
    private readonly float maxRotation = 45f;

    [System.Obsolete]
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

    private void Start()
    {
        if (!photonView.IsMine)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
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

    private void Jump()
    {
        rb.linearVelocity = Vector2.up * jumpForce;
    }
}