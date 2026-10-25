using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class Player : MonoBehaviourPun
{
    private bool jumping = false;
    private float jumpElapsed;
    private float fallElapsed;
    private float gravity = 0f;
    private float jumpForce = 4f;

    private const float MAX_GRAVITY = 5f;
    private const float ACCELERATION_TIME = 1f;
    private const float JUMP_TIME = 0.2f;

    private GameController gc;

    private void Awake()
    {
        gc = FindAnyObjectByType<GameController>();
    }

    private void Update()
    {
        if (gc == null) return;
        if (!gc.IsGameStarted()) return;

        if (photonView.IsMine && Keyboard.current.spaceKey.wasPressedThisFrame)
            photonView.RPC(nameof(Jump), RpcTarget.AllViaServer);

        if (jumping)
        {
            jumpElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(jumpElapsed / JUMP_TIME);
            float speed = Mathf.Lerp(jumpForce, 0f, t);
            transform.position += Vector3.up * speed * Time.deltaTime;
            if (t == 1f) 
            { 
                jumping = false; 
                gravity = 0f; 
                fallElapsed = 0f; 
            }
        }
        else
        {
            fallElapsed += Time.deltaTime;
            gravity = Mathf.Lerp(0f, MAX_GRAVITY, Mathf.Clamp01(fallElapsed / ACCELERATION_TIME));
            transform.position += Vector3.down * gravity * Time.deltaTime;
        }
    }

    [PunRPC]
    private void Jump()
    {
        jumping = true;
        jumpElapsed = 0f;
    }
}
