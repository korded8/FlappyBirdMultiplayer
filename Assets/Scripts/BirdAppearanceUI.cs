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
    public TMP_Text redColorText;
    public TMP_Text greenColorText;
    public TMP_Text blueColorText;

    [Header("Appearance Scripts")]
    public BirdAppearanceData data;
    public BirdAppearance birdAppearance;

    [Header("Presets")]
    public TMP_Dropdown presetsDropdown;
    public TMP_InputField presetNameInput;
    public TMP_Text emptyPresetNameWarning;
    public TMP_Text duplicatePresetNameWarning;

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
        emptyPresetNameWarning.enabled = false;
        duplicatePresetNameWarning.enabled = false;

        redSlider.onValueChanged.AddListener(OnSliderChanged);
        greenSlider.onValueChanged.AddListener(OnSliderChanged);
        blueSlider.onValueChanged.AddListener(OnSliderChanged);

        if (data.presets.Count > 0)
        {
            currentPreset = 0;
            data.ApplyPreset(currentPreset);
        }

        UpdatePresetsDropdown();
        RefreshSlidersWithoutNotify();
        data.OnAppearanceChanged?.Invoke();
    }

    private void OnDestroy()
    {
        redSlider.onValueChanged.RemoveListener(OnSliderChanged);
        greenSlider.onValueChanged.RemoveListener(OnSliderChanged);
        blueSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    public void UpdatePresetsDropdown()
    {
        presetsDropdown.ClearOptions();
        List<string> optionsList = new List<string>();
        foreach (var p in data.presets)
            optionsList.Add(p.presetName);
        presetsDropdown.AddOptions(optionsList);
        presetsDropdown.value = currentPreset;
        presetsDropdown.RefreshShownValue();
    }

    private void RefreshSlidersWithoutNotify()
    {
        Color currentColor = data.GetColor(selectedPart, currentPreset);
        Debug.Log(currentColor);
        redSlider.SetValueWithoutNotify(currentColor.r);
        greenSlider.SetValueWithoutNotify(currentColor.g);
        blueSlider.SetValueWithoutNotify(currentColor.b);

        redColorText.text = Math.Round(redSlider.value * 255).ToString();
        greenColorText.text = Math.Round(greenSlider.value * 255).ToString();
        blueColorText.text = Math.Round(blueSlider.value * 255).ToString();
    }

    public void OnSliderChanged(float value)
    {
        Color currentColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);

        redColorText.text = Math.Round(redSlider.value * 255).ToString();
        greenColorText.text = Math.Round(greenSlider.value * 255).ToString();
        blueColorText.text = Math.Round(blueSlider.value * 255).ToString();

        data.SetColor(selectedPart, currentColor, currentPreset);
        data.OnAppearanceChanged?.Invoke();
    }

    public void SelectPart(int index)
    {
        selectedPart = (BirdPart)index;
        RefreshSlidersWithoutNotify();
    }

    public void AddPreset()
    {
        string newPresetName = presetNameInput.text.Trim();
        if (string.IsNullOrEmpty(newPresetName))
        {
            emptyPresetNameWarning.enabled = true;
            return;
        }
        emptyPresetNameWarning.enabled = false;

        foreach (var p in data.presets)
        {
            if (p.presetName == newPresetName)
            {
                duplicatePresetNameWarning.enabled = true;
                return;
            }
        }
        duplicatePresetNameWarning.enabled = false;

        Color bodyColor = data.bodyColor;
        Color wingColor = data.wingColor;
        Color featherColor = data.featherColor;

        //for (int i = 0; i < 3; i++)
        //{
        //    switch (i)
        //    {
        //        case 0:
        //            selectedPart = (BirdPart)i;
        //            RefreshSlidersWithoutNotify();
        //            bodyColor = new Color(redSlider.value, greenSlider.value, blueSlider.value, 1);
        //            break;
        //        case 1:
        //            selectedPart = (BirdPart)i;
        //            RefreshSlidersWithoutNotify();
        //            wingColor = new Color(redSlider.value, greenSlider.value, blueSlider.value, 1);
        //            break;
        //        case 2:
        //            selectedPart = (BirdPart)i;
        //            RefreshSlidersWithoutNotify();
        //            featherColor = new Color(redSlider.value, greenSlider.value, blueSlider.value, 1);
        //            break;
        //    }
        //}

        data.AddPreset(bodyColor, wingColor, featherColor, newPresetName);
        currentPreset = data.presets.Count - 1;
        data.ApplyPreset(currentPreset);

        UpdatePresetsDropdown();
        RefreshSlidersWithoutNotify();
        data.OnAppearanceChanged?.Invoke();
    }

    public void OnDropdownChanged(int index)
    {
        if (index < 0 || index >= data.presets.Count) return;

        currentPreset = index;
        data.ApplyPreset(currentPreset);
        birdAppearance.ApplyPreset(currentPreset);

        presetNameInput.text = data.presets[currentPreset].presetName;

        RefreshSlidersWithoutNotify();
        data.OnAppearanceChanged?.Invoke();
    }
}