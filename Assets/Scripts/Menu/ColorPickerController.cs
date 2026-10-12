using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerController : MonoBehaviour
{
    [Header("Color Picker UI")]
    [SerializeField] private RawImage saturationBrightnessImage;
    [SerializeField] private Slider hueSlider;
    [SerializeField] private Image hueSliderBackground;
    [SerializeField] private RectTransform selector;

    [Header("Color Output")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text hexLabel;

    [Header("Texture Settings")]
    [SerializeField, Range(32, 512)]
    private int textureSize = 128;

    private Texture2D sbTexture;
    private Texture2D hueTexture;
    private Sprite hueSprite;
    [SerializeField] private BirdAppearanceData data;
    [SerializeField] private BirdAppearanceUI birdAppearanceUI;

    [Header("Toggle buttons")]
    [SerializeField] private Image bodyToggle;
    [SerializeField] private Image wingsToggle;
    [SerializeField] private Image featherToggle;

    private float hue;
    private float saturation = 1f;
    private float brightness = 1f;

    public Color SelectedColor { get; private set; }

    public enum BirdPart
    {
        body,
        wings,
        feather
    }

    private BirdPart selectedPart = BirdPart.body;

    private void Awake()
    {
        sbTexture = new Texture2D(
            textureSize, textureSize, TextureFormat.RGB24, false);

        sbTexture.wrapMode = TextureWrapMode.Clamp;
        sbTexture.filterMode = FilterMode.Bilinear;

        saturationBrightnessImage.texture = sbTexture;

        CreateHueGradient();

        hueSlider.minValue = 0f;
        hueSlider.maxValue = 1f;
        hueSlider.wholeNumbers = false;

        hueSlider.onValueChanged.AddListener(OnHueChanged);
        hue = hueSlider.value;

        SelectPart(0);

        birdAppearanceUI.OnCurrentPresetChanged += OnPresetChanged;
        OnPresetChanged();
    }

    private void CreateHueGradient()
    {
        int width = 256;

        hueTexture = new Texture2D(
            width, 1, TextureFormat.RGB24, false);

        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < width; x++)
        {
            float h = x / (float)(width - 1);
            hueTexture.SetPixel(x, 0, Color.HSVToRGB(h, 1f, 1f));
        }

        hueTexture.Apply();

        hueSprite = Sprite.Create(
            hueTexture,
            new Rect(0, 0, width, 1),
            new Vector2(0.5f, 0.5f));

        if (hueSliderBackground != null)
        {
            hueSliderBackground.sprite = hueSprite;
            hueSliderBackground.type = Image.Type.Simple;
            hueSliderBackground.color = Color.white;
        }
    }

    private void OnHueChanged(float value)
    {
        hue = value;
        hueSlider.handleRect.GetComponent<Image>().color = Color.HSVToRGB(value, 1f, 1f);
        UpdateColorSquare();
        UpdateSelectedColor();
    }

    private void UpdateColorSquare()
    {
        for (int y = 0; y < textureSize; y++)
        {
            float v = y / (float)(textureSize - 1);

            for (int x = 0; x < textureSize; x++)
            {
                float s = x / (float)(textureSize - 1);

                Color color = Color.HSVToRGB(hue, s, v);
                sbTexture.SetPixel(x, y, color);
            }
        }

        sbTexture.Apply();
    }

    public void SetSaturationBrightness(float s, float v)
    {
        saturation = Mathf.Clamp01(s);
        brightness = Mathf.Clamp01(v);

        UpdateSelectorPosition();
        UpdateSelectedColor();
    }

    private void UpdateSelectorPosition()
    {
        RectTransform rect = saturationBrightnessImage.rectTransform;

        selector.anchoredPosition = new Vector2(
            (saturation - 0.5f) * rect.rect.width,
            (brightness - 0.5f) * rect.rect.height
        );
    }

    private void UpdateSelectedColor(bool picked = true)
    {
        if (!picked)
        {
            Color color;
            switch (selectedPart)
            {
                case BirdPart.body:
                    color = new Color(data.bodyColor.r, data.bodyColor.g, data.bodyColor.b);
                    Color.RGBToHSV(color, out hue, out saturation, out brightness);
                    break;
                case BirdPart.wings:
                    color = new Color(data.wingColor.r, data.wingColor.g, data.wingColor.b);
                    Color.RGBToHSV(color, out hue, out saturation, out brightness);
                    break;
                case BirdPart.feather:
                    color = new Color(data.featherColor.r, data.featherColor.g, data.featherColor.b);
                    Color.RGBToHSV(color, out hue, out saturation, out brightness);
                    break;
            }
        }

        SelectedColor = Color.HSVToRGB(
        hue, saturation, brightness);

        if (targetImage != null)
            targetImage.color = SelectedColor;

        if (previewImage != null)
            previewImage.color = SelectedColor;

        if (selector != null)
            selector.GetComponent<Image>().color = SelectedColor;

        if (picked)
        {
            data.SetColor(selectedPart, SelectedColor);
            data.OnAppearanceChanged?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (hueSlider != null)
            hueSlider.onValueChanged.RemoveListener(OnHueChanged);

        if (sbTexture != null)
            Destroy(sbTexture);

        if (hueSprite != null)
            Destroy(hueSprite);

        if (hueTexture != null)
            Destroy(hueTexture);

        birdAppearanceUI.OnCurrentPresetChanged -= OnPresetChanged;
    }

    public void SelectPart(int index)
    {
        selectedPart = (BirdPart)index;

        bodyToggle.color = new Color(0.4f, 0.4f, 0.4f);
        wingsToggle.color = new Color(0.4f, 0.4f, 0.4f);
        featherToggle.color = new Color(0.4f, 0.4f, 0.4f);

        switch (index)
        {
            case 0:
                bodyToggle.color = new Color(0.5333333333f, 0.5333333333f, 0.5333333333f);
                break;
            case 1:
                wingsToggle.color = new Color(0.5333333333f, 0.5333333333f, 0.5333333333f);
                break;
            case 2:
                featherToggle.color = new Color(0.5333333333f, 0.5333333333f, 0.5333333333f);
                break;
        }

        OnPresetChanged();
    }

    private void OnPresetChanged()
    {
        UpdateSelectedColor(false);
        hueSlider.value = hue;
        UpdateColorSquare();
        UpdateSelectorPosition();
    }
}