using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class Player : MonoBehaviourPun, IPunObservable
{
    private bool jumping = false;
    private float jumpElapsed;
    private float fallElapsed;
    private float gravity = 0f;
    private float jumpForce = 4f;

    private const float MAX_GRAVITY = 5f;
    private const float ACCELERATION_TIME = 1f;
    private const float JUMP_TIME = 0.2f;
    private const float REMOTE_SMOOTHING = 12f;

    private GameController gc;
    private Vector3 networkPosition;

    private void Awake()
    {
        gc = FindAnyObjectByType<GameController>();
        networkPosition = transform.position;
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                networkPosition,
                REMOTE_SMOOTHING * Time.deltaTime);
            return;
        }

        if (gc == null) return;
        if (!gc.IsGameStarted()) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            Jump();

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

    private void Jump()
    {
        jumping = true;
        jumpElapsed = 0f;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
        }
    }
}