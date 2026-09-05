using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BirdAppearanceUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    [Header("Color texts")]
    public TMP_InputField redColorInput;
    public TMP_InputField greenColorInput;
    public TMP_InputField blueColorInput;

    [Header("Appearance Scripts")]
    public BirdAppearanceData data;
    public BirdAppearance birdAppearance;

    [Header("Presets")]
    //public TMP_Dropdown presetsDropdown;
    //public TMP_InputField presetNameInput;
    //public TMP_Text emptyPresetNameWarning;
    //public TMP_Text duplicatePresetNameWarning;
    public TMP_Text deletingClassicPresetWarning;
    public TMP_Text classicEditWarning;

    [Header("Highscore text")]
    public TMP_Text highscoreText;

    [Header("Skins")]
    private int selectedSkinIndex;

    public enum BirdPart
    {
        body,
        wings,
        feather
    }
    private BirdPart selectedPart = BirdPart.body;
    private int currentPreset = 0;

    private void Start()
    {
        ShowWarning(null);

        //presetNameInput.characterLimit = 8;

        redSlider.onValueChanged.AddListener(OnSliderChanged);
        greenSlider.onValueChanged.AddListener(OnSliderChanged);
        blueSlider.onValueChanged.AddListener(OnSliderChanged);

        //presetsDropdown.onValueChanged.AddListener(OnDropdownChanged);

        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        highscoreText.text = "Highscore: " + highscore.ToString();

        data.LoadPresets();
        currentPreset = data.currentPresetIndex;
        birdAppearance.ApplySkin(data.currentSkinIndex);

        RefreshSlidersWithoutNotify();
        data.OnAppearanceChanged?.Invoke();
    }

    private void OnDestroy()
    {
        redSlider.onValueChanged.RemoveListener(OnSliderChanged);
        greenSlider.onValueChanged.RemoveListener(OnSliderChanged);
        blueSlider.onValueChanged.RemoveListener(OnSliderChanged);
        //presetsDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
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

    public void Randomize(BirdPart birdPart)
    {
        float r = UnityEngine.Random.Range(0, 1f);
        float g = UnityEngine.Random.Range(0, 1f);
        float b = UnityEngine.Random.Range(0, 1f);
        Color newColor = new(r, g, b);

        data.SetColor(birdPart, newColor);
    }

    public void SelectPart(int index)
    {
        selectedPart = (BirdPart)index;
        RefreshSlidersWithoutNotify();
    }

    public void SelectSkin(int index)
    {
        selectedSkinIndex = index;
        data.currentSkinIndex = index;
        birdAppearance.ApplySkin(index);
    }

    public void AddPreset()
    {
        //string newPresetName = presetNameInput.text.Trim();

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
