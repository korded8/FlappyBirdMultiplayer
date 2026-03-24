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

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

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
}
