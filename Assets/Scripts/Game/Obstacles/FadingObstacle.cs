using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(SpriteRenderer))]
public class FadingObstacle : MonoBehaviourPun
{
    [Header("Fade settings")]
    private readonly float fadeSpeed = 1f;
    private readonly float minAlpha = 0.01f;
    private readonly float maxAlpha = 1f;
    private readonly float waitTime = 4f;

    private SpriteRenderer sr;

    private float currentAlpha;
    private float timer = 0f;

    private enum FadeState
    {
        FadingOut,
        Waiting,
        FadingIn
    }

    private FadeState state = FadeState.FadingOut;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        currentAlpha = maxAlpha;
        ApplyAlpha(currentAlpha);
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        UpdateFade();
    }

    private void UpdateFade()
    {
        float delta = fadeSpeed * Time.deltaTime;

        switch (state)
        {
            case FadeState.FadingOut:
                currentAlpha -= delta;
                if (currentAlpha <= minAlpha)
                {
                    currentAlpha = minAlpha;
                    state = FadeState.Waiting;
                    timer = waitTime;
                }
                break;

            case FadeState.Waiting:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    state = FadeState.FadingIn;
                }
                break;

            case FadeState.FadingIn:
                currentAlpha += delta;
                if (currentAlpha >= maxAlpha)
                {
                    currentAlpha = maxAlpha;
                    state = FadeState.FadingOut;
                }
                break;
        }

        photonView.RPC(nameof(SyncAlpha), RpcTarget.All, currentAlpha);
    }

    void ApplyAlpha(float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    [PunRPC]
    void SyncAlpha(float alpha)
    {
        ApplyAlpha(alpha);
    }
}
