using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BirdAppearance : MonoBehaviour
{
    [Header("Material & Mask")]
    public Material materialInstance;
    public Texture2D maskTexture;

    [Header("Appearance Data")]
    public BirdAppearanceData data;

    [Header("Preview Settings")]

    private Animator birdAnimator;
    private SpriteRenderer spriteRenderer;
    private RuntimeAnimatorController animatorController;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        birdAnimator = GetComponent<Animator>();

        if (spriteRenderer != null && materialInstance == null)
        {
            materialInstance = new Material(spriteRenderer.sharedMaterial);
            spriteRenderer.material = materialInstance;
        }

        if (maskTexture != null)
            materialInstance.SetTexture("_MaskTex", maskTexture);

        if (data != null)
        {
            data.OnAppearanceChanged -= ApplyColors;
            data.OnAppearanceChanged += ApplyColors;
        }

        ApplyColors();
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
        if (spriteRenderer == null || materialInstance == null || data == null) return;

        materialInstance.SetColor("_RedColor", data.bodyColor);
        materialInstance.SetColor("_GreenColor", data.wingColor);
        materialInstance.SetColor("_BlueColor", data.featherColor);
    }

    public void ChangeSkin()
    {
        birdAnimator.runtimeAnimatorController = data.skins[data.currentSkinIndex].GetComponent<Animator>().runtimeAnimatorController;
        spriteRenderer.sprite = data.skins[data.currentSkinIndex].GetComponent<SpriteRenderer>().sprite;
        materialInstance = new Material(data.skins[data.currentSkinIndex].GetComponent<SpriteRenderer>().sharedMaterial);
        spriteRenderer.sharedMaterial = materialInstance;

        if (maskTexture != null)
            materialInstance.SetTexture("_MaskTex", maskTexture);

        if (data.currentSkinIndex == 0)
        {
            ApplyColors();
        }
    }
}