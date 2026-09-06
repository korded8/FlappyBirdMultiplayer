using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BirdAppearanceUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;
    
    [Header("Color texts")]
    [SerializeField] private TMP_InputField redColorInput;
    [SerializeField] private TMP_InputField greenColorInput;
    [SerializeField] private TMP_InputField blueColorInput;
     
    [Header("Appearance Scripts")]
    [SerializeField] private BirdAppearanceData data;
    [SerializeField] private BirdAppearance birdAppearance;
     
    [Header("Presets")]
    [SerializeField] private TMP_Text deletingClassicPresetWarning;
    [SerializeField] private TMP_Text classicEditWarning;
    [SerializeField] private Button randomizeButton;
    [SerializeField] private float transitionDuration = 1.5f;

    public enum BirdPart
    {
        body,
        wings,
        feather
    }
    private BirdPart selectedPart = BirdPart.body;
    private int currentPreset = 0;

    [Header("Highscore text")]
    [SerializeField] private TMP_Text highscoreText;

    private void Start()
    {
        ShowWarning(null);

        redSlider.onValueChanged.AddListener(OnSliderChanged);
        greenSlider.onValueChanged.AddListener(OnSliderChanged);
        blueSlider.onValueChanged.AddListener(OnSliderChanged);

        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        highscoreText.text = "Highscore: " + highscore.ToString();

        StartCoroutine(nameof(TransitionFirstColor));

        data.LoadPresets();
        currentPreset = data.currentPresetIndex;
        birdAppearance.ApplySkin(data.currentSkinIndex);

        RefreshSlidersWithoutNotify();
        data.OnAppearanceChanged?.Invoke();
    }

    private IEnumerator TransitionFirstColor()
    {
        randomizeButton.GetComponent<Image>().color = RandomizeColor();
        yield return new WaitForSeconds(transitionDuration);
        StartCoroutine(nameof(CycleColors));
    }

    private IEnumerator CycleColors()
    {
        while (true)
        {
            Color startColor = randomizeButton.GetComponent<Image>().color;
            Color targetColor = RandomizeColor();

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / transitionDuration);
                t = Mathf.SmoothStep(0f, 1f, t);

                randomizeButton.GetComponent<Image>().color = Color.Lerp(startColor, targetColor, t);

                yield return null;
            }
        }
    }

    private void OnDestroy()
    {
        redSlider.onValueChanged.RemoveListener(OnSliderChanged);
        greenSlider.onValueChanged.RemoveListener(OnSliderChanged);
        blueSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void RefreshSlidersWithoutNotify()
    {
        Color currentColor = data.GetColor(selectedPart);

        redSlider.SetValueWithoutNotify(currentColor.r);
        greenSlider.SetValueWithoutNotify(currentColor.g);
        blueSlider.SetValueWithoutNotify(currentColor.b);

        redColorInput.SetTextWithoutNotify(Math.Round(redSlider.value * 255).ToString());
        greenColorInput.SetTextWithoutNotify(Math.Round(greenSlider.value * 255).ToString());
        blueColorInput.SetTextWithoutNotify(Math.Round(blueSlider.value * 255).ToString());
    }

    public void OnSliderChanged(float value)
    {
        Color currentColor = new(redSlider.value, greenSlider.value, blueSlider.value);

        redColorInput.text = Math.Round(redSlider.value * 255).ToString();
        greenColorInput.text = Math.Round(greenSlider.value * 255).ToString();
        blueColorInput.text = Math.Round(blueSlider.value * 255).ToString();

        data.SetColor(selectedPart, currentColor);
        data.OnAppearanceChanged?.Invoke();
    }

    public void OnRGBInputChanged()
    {
        float r = ParseInputToFloat(redColorInput.text);
        float g = ParseInputToFloat(greenColorInput.text);
        float b = ParseInputToFloat(blueColorInput.text);

        Color newColor = new(r, g, b, 1f);

        data.SetColor(selectedPart, newColor);

        redSlider.SetValueWithoutNotify(r);
        greenSlider.SetValueWithoutNotify(g);
        blueSlider.SetValueWithoutNotify(b);
    }

    private float ParseInputToFloat(string text)
    {
        if (int.TryParse(text, out int result))
        {
            result = Mathf.Clamp(result, 0, 255);
            return result / 255f;
        }

        return 0f;
    }

    public void RandomizeColors()
    {
        Randomize(BirdPart.body);
        Randomize(BirdPart.wings);
        Randomize(BirdPart.feather);

        RefreshSlidersWithoutNotify();
    }

    private Color RandomizeColor()
    {
        float r = UnityEngine.Random.Range(0, 1f);
        float g = UnityEngine.Random.Range(0, 1f);
        float b = UnityEngine.Random.Range(0, 1f);

        Color newColor = new Color(r, g, b);
        return newColor;
    }

    public void Randomize(BirdPart birdPart)
    {
        Color newColor = RandomizeColor();
        data.SetColor(birdPart, newColor);
    }

    public void SelectPart(int index)
    {
        selectedPart = (BirdPart)index;
        RefreshSlidersWithoutNotify();
    }

    public void SelectSkin(int index)
    {
        data.currentSkinIndex = index;
        birdAppearance.ApplySkin(index);
    }

    public void AddPreset()
    {
        Color bodyColor = data.bodyColor;
        Color wingColor = data.wingColor;
        Color featherColor = data.featherColor;

        data.AddPreset(bodyColor, wingColor, featherColor);
        currentPreset = data.presets.Count - 1;
        data.ApplyPreset(currentPreset);

        RefreshSlidersWithoutNotify();
        data.OnAppearanceChanged?.Invoke();
    }

    public void UpdatePreset()
    {
        if (currentPreset == 0)
        {
            ShowWarning(classicEditWarning);
            return;
        }

        Color bodyColor = data.bodyColor;
        Color wingColor = data.wingColor;
        Color featherColor = data.featherColor;

        data.UpdatePreset(bodyColor, wingColor, featherColor, currentPreset);
        data.ApplyPreset(currentPreset);

        RefreshSlidersWithoutNotify();
        data.OnAppearanceChanged?.Invoke();
    }

    public void DeletePreset()
    {
        if (currentPreset == 0)
        {
            ShowWarning(deletingClassicPresetWarning);
            return;
        }
        else
        {
            ShowWarning(null);
        }

        data.DeletePreset(currentPreset);
        currentPreset = 0;
        
        RefreshSlidersWithoutNotify();
        data.OnAppearanceChanged?.Invoke();
    }

    public void SelectPreset(int index)
    {
        if (index < 0 || index == currentPreset) return;

        ShowWarning(null);

        if (index > data.presets.Count - 1)
        {
            AddPreset();
            return;
        }

        currentPreset = index;
        data.ApplyPreset(currentPreset);
        birdAppearance.ApplyColors();


        RefreshSlidersWithoutNotify();
        data.OnAppearanceChanged?.Invoke();
    }

    private void ShowWarning(TMP_Text warning)
    {
        deletingClassicPresetWarning.enabled = false;
        classicEditWarning.enabled = false;

        if (warning != null) warning.enabled = true;
    }
}
