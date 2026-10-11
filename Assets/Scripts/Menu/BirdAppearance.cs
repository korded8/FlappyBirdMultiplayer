using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class BirdAppearance : MonoBehaviourPunCallbacks
{
    [Header("Material & Mask")]
    public Material materialInstance;
    public Texture2D maskTexture;

    [Header("Appearance Data")]
    public BirdAppearanceData data;

    [Header("Preview Settings")]

    private Animator birdAnimator;
    private SpriteRenderer spriteRenderer;
    private Image image;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        birdAnimator = GetComponent<Animator>();

        if (spriteRenderer == null)
        {
            image = GetComponent<Image>();
        }

        if (spriteRenderer != null && materialInstance == null)
        {
            materialInstance = new Material(spriteRenderer.sharedMaterial);
            spriteRenderer.material = materialInstance;
        }
        else if (spriteRenderer == null)
        {
            materialInstance = new Material(image.material);
            image.material = materialInstance;
        }

        if (maskTexture != null)
            materialInstance.SetTexture("_MaskTex", maskTexture);

        if (data != null)
        {
            data.OnAppearanceChanged -= ApplyColors;
            data.OnAppearanceChanged += ApplyColors;
        }

        if (PhotonNetwork.InRoom)
        {
            SynchronizeSkin(photonView.Owner);
            SynchronizeColors(photonView.Owner);
        }
        else
        {
            ApplyColors();
        }
    }

    private void OnEnable()
    {
        if (data != null)
        {
            data.OnAppearanceChanged -= ApplyColors;
            data.OnAppearanceChanged += ApplyColors;
        }
    }

    private void OnDisable()
    {
        if (data != null)
            data.OnAppearanceChanged -= ApplyColors;
    }

    public void ApplyColors()
    {
        if ((spriteRenderer == null && image == null) || materialInstance == null || data == null) return;

        materialInstance.SetColor("_RedColor", data.bodyColor);
        materialInstance.SetColor("_GreenColor", data.wingColor);
        materialInstance.SetColor("_BlueColor", data.featherColor);
    }

    public void ApplySkin(int index, bool applyColors = true)
    {
        birdAnimator.runtimeAnimatorController = data.skins[index].GetComponent<Animator>().runtimeAnimatorController;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = data.skins[index].GetComponent<SpriteRenderer>().sprite;
            materialInstance = new Material(data.skins[index].GetComponent<SpriteRenderer>().sharedMaterial);
            spriteRenderer.sharedMaterial = materialInstance;
        }
        else
        {
            image.sprite = data.skins[index].GetComponent<SpriteRenderer>().sprite;
            materialInstance = new Material(data.skins[index].GetComponent<SpriteRenderer>().sharedMaterial);
            image.material = materialInstance;
        }

        if (maskTexture != null)
            materialInstance.SetTexture("_MaskTex", maskTexture);

        if (data.currentSkinIndex == 0)
        {
            ApplyColors();
        }
    }

    private void SynchronizeColors(Photon.Realtime.Player player)
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

    private void SynchronizeSkin(Photon.Realtime.Player player)
    {
        if (player.CustomProperties.TryGetValue("skinIndex", out object obj))
        {
            int skinIndex = (int)obj;
            ApplySkin(skinIndex, false);
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == photonView.Owner && changedProps.ContainsKey("skinIndex"))
        {
            SynchronizeSkin(targetPlayer);
        }
        if (targetPlayer == photonView.Owner && changedProps.ContainsKey("colors"))
        {
            SynchronizeColors(targetPlayer);
        }
    }
}