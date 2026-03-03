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
    private Color bodyColor = Color.white;
    private Color wingColor = Color.white;
    private Color featherColor = Color.white;

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
        if (data == null) return;

        ApplyColors(data.bodyColor, data.wingColor, data.featherColor);
    }

    public void ApplyColors(Color body, Color wing, Color feather)
    {
        bodyColor = body;
        wingColor = wing;
        featherColor = feather;

        if (spriteRenderer == null || materialInstance == null) return;

        materialInstance.SetColor("_RedColor", bodyColor);
        materialInstance.SetColor("_GreenColor", wingColor);
        materialInstance.SetColor("_BlueColor", featherColor);
    }
}
