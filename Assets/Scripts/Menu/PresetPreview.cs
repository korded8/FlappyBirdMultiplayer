using UnityEngine;
using UnityEngine.UI;

public class PresetPreview : MonoBehaviour
{
    public int viewedPresetIndex;
    public Image buttonImage;
    public Image renderedImage;
    public Image defaultRenderedImage;
    public Image defaultButtonImage;
    public SpriteRenderer birdSprite;
    public BirdAppearanceData data;
    public Material materialInstance;
    public Texture2D maskTexture;

    private void Awake()
    {
        if (birdSprite != null)
        {
            materialInstance = new Material(birdSprite.sharedMaterial);
        }
        renderedImage.material = materialInstance;
        materialInstance.SetTexture("_MaskTex", maskTexture);

        data.OnAppearanceChanged -= UpdatePreview;
        data.OnAppearanceChanged += UpdatePreview;

        UpdatePreview();
    }

    public void UpdatePreview()
    {
        if (renderedImage == null) return;
        if (viewedPresetIndex > data.presets.Count - 1)
        {
            renderedImage.enabled = false;
            buttonImage.sprite = defaultRenderedImage.sprite;
        }
        else
        {
            renderedImage.enabled = true;
            renderedImage.sprite = birdSprite.sprite;
            buttonImage.sprite = defaultButtonImage.sprite;
            materialInstance = new Material(birdSprite.sharedMaterial);
            renderedImage.material = materialInstance;
            ApplyColors();
        }
    }

    public void ApplyColors()
    {
        if (materialInstance == null || data == null) return;

        materialInstance.SetColor("_RedColor", data.presets[viewedPresetIndex].bodyColor);
        materialInstance.SetColor("_GreenColor", data.presets[viewedPresetIndex].wingColor);
        materialInstance.SetColor("_BlueColor", data.presets[viewedPresetIndex].featherColor);
    }
}
