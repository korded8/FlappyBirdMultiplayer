using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Player : MonoBehaviourPun, IPunObservable
{
    private bool jumping = false;
    private float jumpElapsed;
    private float fallElapsed;
    private float gravity = 0f;
    private float jumpForce = 4f;
    private float speed = 0f;

    private const float MAX_GRAVITY = 5f;
    private const float ACCELERATION_TIME = 1f;
    private const float JUMP_TIME = 0.2f;
    private const float ROTATION_SPEED = 3f;
    private const float SMOOTH_SPEED = 9f;

    private const float JUMP_ANGLE = 45f;
    private const float FALL_ANGLE = -45f;

    private GameController gc;
    private SFXController sfx;

    private Vector3 networkPosition;
    private float targetAngle = FALL_ANGLE;
    private float networkSpeed;

    private void Awake()
    {
        gc = FindAnyObjectByType<GameController>();

        networkPosition = transform.position;
        networkSpeed = speed;
    }

    private void Update()
    {
        if (sfx == null)
            sfx = GetComponent<SFXController>();

        if (photonView.IsMine)
        {
            if (!gc.IsGameStarted())
                GetComponent<SpriteRenderer>().sortingOrder = 10;


            UpdateLocalPlayer();
        }
        else
        {
            if (!gc.IsGameStarted())
                GetComponent<SpriteRenderer>().sortingOrder = 0;

            UpdateRemotePlayer();
            speed = networkSpeed;
        }

        RotateLocally();
    }

    private void UpdateLocalPlayer()
    {
        if (gc == null || !gc.IsGameStarted())
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
            StartCoroutine(sfx.PlaySFX(0));
            photonView.RPC(nameof(sfx.PlaySFX_RPC), RpcTarget.Others, 1, 0.25f);
        }

        if (jumping)
        {
            jumpElapsed += Time.deltaTime;

            float t = Mathf.Clamp01(jumpElapsed / JUMP_TIME);
            speed = Mathf.Lerp(jumpForce, 0f, t);

            transform.position += Vector3.up * speed * Time.deltaTime;

            targetAngle = JUMP_ANGLE;

            if (t >= 1f)
            {
                jumping = false;
                gravity = 0f;
                fallElapsed = 0f;
            }
        }
        else
        {
            speed = 0f;
            fallElapsed += Time.deltaTime;

            gravity = Mathf.Lerp(
                0f,
                MAX_GRAVITY,
                Mathf.Clamp01(fallElapsed / ACCELERATION_TIME)
            );

            speed = gravity;

            transform.position += Vector3.down * gravity * Time.deltaTime;

            targetAngle = FALL_ANGLE;
        }
    }

    private void UpdateRemotePlayer()
    {
        Vector3 oldPosition = transform.position;

        transform.position = Vector3.Lerp(
            transform.position,
            networkPosition,
            SMOOTH_SPEED * Time.deltaTime
        );

        float verticalMovement =
            transform.position.y - oldPosition.y;

        if (verticalMovement > 0.001f)
        {
            targetAngle = JUMP_ANGLE;
        }
        else if (verticalMovement < -0.001f)
        {
            targetAngle = FALL_ANGLE;
        }
    }

    private void RotateLocally()
    {
        Quaternion targetRotation = Quaternion.Euler(
            0f,
            0f,
            targetAngle
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            ROTATION_SPEED * speed * Time.deltaTime
        );
    }

    private void Jump()
    {
        jumping = true;
        jumpElapsed = 0f;
        targetAngle = JUMP_ANGLE;
    }

    public void OnPhotonSerializeView(
        PhotonStream stream,
        PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(speed);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkSpeed = (float)stream.ReceiveNext();
        }
    }
}
