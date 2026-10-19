using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviourPun
{
    private float jumpForce = 3f;
    private float gravity = 0f;

    private const float MAX_GRAVITY = 5f;
    private const float ACCELERATION_TIME = 1f;
    private const float JUMP_TIME = 0.5f;

    private bool isFalling = true;
    private bool isFallCoroutineCalled = false;
    private Coroutine fallCoroutine;
    private Coroutine jumpCoroutine;

    private void Awake()
    {
        fallCoroutine = StartCoroutine(FallDown());
        isFallCoroutineCalled = true;
    }
    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            photonView.RPC(nameof(Movement), RpcTarget.All);
        }

        photonView.RPC(nameof(ManageFallCoroutine), RpcTarget.All, false);
    }

    [PunRPC]
    private void Movement()
    {
        photonView.RPC(nameof(ManageFallCoroutine), RpcTarget.All, true);

        if (jumpCoroutine != null)
            StopCoroutine(jumpCoroutine);

        jumpCoroutine = StartCoroutine(JumpCoroutine());
    }

    [PunRPC]
    private void ManageFallCoroutine(bool stopFalling)
    {
        if (!stopFalling)
        {
            if (isFalling && !isFallCoroutineCalled)
            {
                fallCoroutine = StartCoroutine(FallDown());
                isFallCoroutineCalled = true;
            }
        }
        else
        {
            if (jumpCoroutine != null)
            {
                StopCoroutine(fallCoroutine);
                isFallCoroutineCalled = false;
            }
        }
    }

    [PunRPC]
    private void SyncIsFalling(bool fall)
    {
        isFalling = fall;
    }

    private IEnumerator JumpCoroutine()
    {
        float elapsed = 0f;
        gravity = 0f;
        photonView.RPC(nameof(SyncIsFalling), RpcTarget.All, false);

        while (elapsed < JUMP_TIME)
        {
            elapsed += Time.deltaTime;

            transform.position += Vector3.up * jumpForce * Time.deltaTime;

            yield return null;
        }

        elapsed = 0f;
        float speed = jumpForce;

        while (speed > 0f)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / (JUMP_TIME / 2));
            speed = Mathf.Lerp(jumpForce, 0f, t);

            transform.position += Vector3.up * speed * Time.deltaTime;

            yield return null;
        }

        photonView.RPC(nameof(SyncIsFalling), RpcTarget.All, true);
    }

    private IEnumerator FallDown()
    {
        float elapsed = 0f;

        while (isFalling)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / ACCELERATION_TIME);
            gravity = Mathf.Lerp(0f, MAX_GRAVITY, t);

            transform.position += Vector3.down * gravity * Time.deltaTime;

            yield return null;
        }
    }

}
