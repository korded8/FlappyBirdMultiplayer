using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public SpriteRenderer birdRenderer;
    public Texture2D maskTexture;
    public BirdAppearanceData data;
    private Rigidbody2D rb;
    Material materialInstance;
    private Animator playerAnimator;

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

        playerAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        materialInstance = birdRenderer.material;
        ApplySkin(photonView.Owner);
        ApplyColors(photonView.Owner);
    }

    private void ApplyColors(Photon.Realtime.Player player)
    {
        if (player.CustomProperties.TryGetValue("colors", out object obj))
        {
            byte[] data = (byte[])obj;

            Color body = new Color32(data[0], data[1], data[2], 255);
            Color wing = new Color32(data[3], data[4], data[5], 255);
            Color feather = new Color32(data[6], data[7], data[8], 255);

            materialInstance.SetColor("_RedColor", body);
            materialInstance.SetColor("_GreenColor", wing);
            materialInstance.SetColor("_BlueColor", feather);
        }
    }

    private void ApplySkin(Photon.Realtime.Player player)
    {
        if (player.CustomProperties.TryGetValue("skinIndex", out object obj))
        {
            int skin = (int)obj;
            int skinIndex = skin;
            playerAnimator.runtimeAnimatorController = data.skins[skinIndex].GetComponent<Animator>().runtimeAnimatorController;
            birdRenderer.sprite = data.skins[skinIndex].GetComponent<SpriteRenderer>().sprite;
            materialInstance = new Material(data.skins[skinIndex].GetComponent<SpriteRenderer>().sharedMaterial);
            birdRenderer.sharedMaterial = materialInstance;
            if (maskTexture != null)
                materialInstance.SetTexture("_MaskTex", maskTexture);
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == photonView.Owner && changedProps.ContainsKey("skinIndex"))
        {
            ApplySkin(targetPlayer);
        }
        if (targetPlayer == photonView.Owner && changedProps.ContainsKey("colors"))
        {
            ApplyColors(targetPlayer);
        }
    }

    public void EnablePhysics()
    {
        if (rb != null)
            rb.simulated = true;
    }
}
