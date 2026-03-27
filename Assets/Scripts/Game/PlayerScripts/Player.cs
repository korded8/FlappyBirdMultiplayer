using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviourPun
{
    [Header("Settings")]
    [SerializeField] private float jumpForce = 5f;

    private Rigidbody2D rb;
    private readonly float maxRotation = 90f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        transform.eulerAngles = new Vector3(0, 0, Math.Clamp(rb.linearVelocity.y / 5f * maxRotation, -maxRotation, maxRotation));
        if (!photonView.IsMine) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.linearVelocity = Vector2.up * jumpForce;
    }
}
