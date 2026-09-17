using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviourPun
{
    [SerializeField] private PhotonTransformViewClassic photonTransformView;

    private Rigidbody2D rb;
    private GameController gc;
    private float jumpForce = 4f;
    private readonly float maxRotation = 45f;

    [System.Obsolete]
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        gc = FindObjectOfType<GameController>();
        if (gc != null && !gc.IsGameStarted())
        {
            rb.simulated = false;
        }

        if (!photonView.IsMine)
        {
            rb.simulated = false;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (gc.IsGameStarted()) rb.simulated = true;
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

        //photonTransformView.SetSynchronizedValues(
        //    rb.linearVelocity,
        //    rb.angularVelocity);
    }

    private void Jump()
    {
        rb.linearVelocity = Vector2.up * jumpForce;
    }
}