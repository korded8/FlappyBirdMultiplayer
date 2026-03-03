using System;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
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

    private const string BodyColorPrefKey = "BirdColorBody";
    private const string WingColorPrefKey = "BirdColorWing";
    private const string FeatherColorPrefKey = "BirdColorFeather";
    private const string BodyColorNetKey = "birdBodyColor";
    private const string WingColorNetKey = "birdWingColor";
    private const string FeatherColorNetKey = "birdFeatherColor";

    private void Start()
    {
        emptyPresetNameWarning.enabled = false;
        duplicatePresetNameWarning.enabled = false;

        presetNameInput.characterLimit = 8;

        redSlider.onValueChanged.AddListener(OnSliderChanged);
        greenSlider.onValueChanged.AddListener(OnSliderChanged);
        blueSlider.onValueChanged.AddListener(OnSliderChanged);

        presetsDropdown.onValueChanged.AddListener(OnDropdownChanged);

        if (LoadColorsFromPrefs())
        {
            currentPreset = -1;
        }
        else if (data.presets.Count > 0)
        {
            currentPreset = 0;
            data.ApplyPreset(currentPreset);
        }

        UpdatePresetsDropdown();
        RefreshSlidersWithoutNotify();
        SaveAndSyncColors();
        data.OnAppearanceChanged?.Invoke();
    }

    private void OnDestroy()
    {
        redSlider.onValueChanged.RemoveListener(OnSliderChanged);
        greenSlider.onValueChanged.RemoveListener(OnSliderChanged);
        blueSlider.onValueChanged.RemoveListener(OnSliderChanged);
        presetsDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    public void UpdatePresetsDropdown()
    {
        presetsDropdown.ClearOptions();
        List<string> optionsList = new List<string>();
        foreach (var p in data.presets)
        {
            optionsList.Add(p.presetName);
        }
        optionsList.Add("Add preset");
        presetsDropdown.AddOptions(optionsList);
        int selectedIndex = Mathf.Clamp(currentPreset, 0, Mathf.Max(0, optionsList.Count - 1));
        presetsDropdown.value = selectedIndex;
        presetsDropdown.RefreshShownValue();
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
        Color currentColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);

        redColorInput.text = Math.Round(redSlider.value * 255).ToString();
        greenColorInput.text = Math.Round(greenSlider.value * 255).ToString();
        blueColorInput.text = Math.Round(blueSlider.value * 255).ToString();

        data.SetColor(selectedPart, currentColor, currentPreset);
        SaveAndSyncColors();
        data.OnAppearanceChanged?.Invoke();
    }

    public void OnRGBInputChanged()
    {
        float r = ParseInputToFloat(redColorInput.text);
        float g = ParseInputToFloat(greenColorInput.text);
        float b = ParseInputToFloat(blueColorInput.text);

        Color newColor = new Color(r, g, b, 1f);

        data.SetColor(selectedPart, newColor, currentPreset);

        redSlider.SetValueWithoutNotify(r);
        greenSlider.SetValueWithoutNotify(g);
        blueSlider.SetValueWithoutNotify(b);

        SaveAndSyncColors();
        data.OnAppearanceChanged?.Invoke();
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
            presetsDropdown.value = Mathf.Clamp(currentPreset, 0, Mathf.Max(0, data.presets.Count));
            return;
        }
        emptyPresetNameWarning.enabled = false;

        foreach (var p in data.presets)
        {
            if (p.presetName == newPresetName)
            {
                duplicatePresetNameWarning.enabled = true;
                presetsDropdown.value = Mathf.Clamp(currentPreset, 0, Mathf.Max(0, data.presets.Count));
                return;
            }
        }
        duplicatePresetNameWarning.enabled = false;

        Color bodyColor = data.bodyColor;
        Color wingColor = data.wingColor;
        Color featherColor = data.featherColor;

        data.AddPreset(bodyColor, wingColor, featherColor, newPresetName);
        currentPreset = data.presets.Count - 1;
        data.ApplyPreset(currentPreset);

        UpdatePresetsDropdown();
        RefreshSlidersWithoutNotify();
        SaveAndSyncColors();
        data.OnAppearanceChanged?.Invoke();
    }

    public void UpdatePreset()
    {
        Color bodyColor = data.bodyColor;
        Color wingColor = data.wingColor;
        Color featherColor = data.featherColor;

        data.UpdatePreset(bodyColor, wingColor, featherColor, currentPreset);
        data.ApplyPreset(currentPreset);

        RefreshSlidersWithoutNotify();
        SaveAndSyncColors();
        data.OnAppearanceChanged?.Invoke();
    }

    public void OnDropdownChanged(int index)
    {
        if (index < 0 || index > data.presets.Count || index == currentPreset) return;

        if (index == data.presets.Count)
        {
            AddPreset();
            return;
        }

        currentPreset = index;
        data.ApplyPreset(currentPreset);
        birdAppearance.ApplyColors();

        presetNameInput.text = data.presets[currentPreset].presetName;

        RefreshSlidersWithoutNotify();
        SaveAndSyncColors();
        data.OnAppearanceChanged?.Invoke();
    }

    private bool LoadColorsFromPrefs()
    {
        if (!PlayerPrefs.HasKey(BodyColorPrefKey) || !PlayerPrefs.HasKey(WingColorPrefKey) || !PlayerPrefs.HasKey(FeatherColorPrefKey))
        {
            return false;
        }

        if (!TryParseHtmlColor(PlayerPrefs.GetString(BodyColorPrefKey), out Color body)) return false;
        if (!TryParseHtmlColor(PlayerPrefs.GetString(WingColorPrefKey), out Color wing)) return false;
        if (!TryParseHtmlColor(PlayerPrefs.GetString(FeatherColorPrefKey), out Color feather)) return false;

        data.bodyColor = body;
        data.wingColor = wing;
        data.featherColor = feather;
        return true;
    }

    private void SaveAndSyncColors()
    {
        PlayerPrefs.SetString(BodyColorPrefKey, ColorUtility.ToHtmlStringRGB(data.bodyColor));
        PlayerPrefs.SetString(WingColorPrefKey, ColorUtility.ToHtmlStringRGB(data.wingColor));
        PlayerPrefs.SetString(FeatherColorPrefKey, ColorUtility.ToHtmlStringRGB(data.featherColor));
        PlayerPrefs.Save();

        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        var customProperties = new Hashtable
        {
            { BodyColorNetKey, ColorUtility.ToHtmlStringRGB(data.bodyColor) },
            { WingColorNetKey, ColorUtility.ToHtmlStringRGB(data.wingColor) },
            { FeatherColorNetKey, ColorUtility.ToHtmlStringRGB(data.featherColor) }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    private bool TryParseHtmlColor(string htmlColor, out Color color)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
        {
            color = Color.white;
            return false;
        }

        string formatted = htmlColor.StartsWith("#") ? htmlColor : $"#{htmlColor}";
        return ColorUtility.TryParseHtmlString(formatted, out color);
    }
}
