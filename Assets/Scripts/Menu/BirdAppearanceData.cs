using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "BirdAppearanceData", menuName = "Bird/BirdAppearanceData")]
public class BirdAppearanceData : ScriptableObject
{
    [Serializable]
    public struct Preset
    {
        public Color bodyColor;
        public Color wingColor;
        public Color featherColor;
    }
    [Header("Current Colors")]
    public Color bodyColor = Color.white;
    public Color wingColor = Color.white;
    public Color featherColor = Color.white;

    [Header("Presets")]
    public List<Preset> presets = new();
    public int currentPresetIndex = 0;

    [Header("Skins")]
    public List<GameObject> skins = new();
    public int currentSkinIndex;

    public Action OnAppearanceChanged;

    public void ApplyPreset(int index, bool save = true)
    {
        if (index < 0 || index >= presets.Count) return;

        currentPresetIndex = index;

        bodyColor = presets[index].bodyColor;
        wingColor = presets[index].wingColor;
        featherColor = presets[index].featherColor;

        OnAppearanceChanged?.Invoke();
        if (save) SavePresets();
    }

    public void AddPreset(Color body, Color wing, Color feather)
    {
        if (presets.Count == 6) return;
        Preset newPreset = new()
        {
            bodyColor = body,
            wingColor = wing,
            featherColor = feather
        };

        presets.Add(newPreset);
        SavePresets();
    }

    public void DeletePreset(int index)
    {
        presets.RemoveAt(index);
        currentPresetIndex = 0;
        ApplyPreset(currentPresetIndex);
    }

    public void UpdatePreset(Color body, Color wing, Color feather, int index)
    {
        Preset p = presets[index];
        p.bodyColor = body;
        p.wingColor = wing;
        p.featherColor = feather;
        presets[index] = p;

        SavePresets();
    }

    public void SetColor(ColorPickerController.BirdPart part, Color color)
    {
        switch (part)
        {
            case ColorPickerController.BirdPart.body: bodyColor = color; break;
            case ColorPickerController.BirdPart.wings: wingColor = color; break;
            case ColorPickerController.BirdPart.feather: featherColor = color; break;
        }

        OnAppearanceChanged?.Invoke();
    }
    public Color GetColor(ColorPickerController.BirdPart part)
    {
        return part switch
        {
            ColorPickerController.BirdPart.body => bodyColor,
            ColorPickerController.BirdPart.wings => wingColor,
            ColorPickerController.BirdPart.feather => featherColor,
            _ => Color.white,
        };
    }

    public void Randomize()
    {
        bodyColor = new Color(UnityEngine.Random.value, 0, 0, 1);
        wingColor = new Color(0, UnityEngine.Random.value, 0, 1);
        featherColor = new Color(0, 0, UnityEngine.Random.value, 1);

        OnAppearanceChanged?.Invoke();
    }

    [Serializable]
    private class SaveData
    {
        public List<Preset> presets;
        public int presetIndex;
    }

    private string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "BirdAppearance.json");
    }

    public void SavePresets()
    {
        SaveData data = new()
        {
            presets = presets,
            presetIndex = currentPresetIndex
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetFilePath(), json);
    }

    public void LoadPresets()
    {
        string path = GetFilePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("BirdAppearance save file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        presets = data.presets ?? new List<Preset>();
        currentPresetIndex = Mathf.Clamp(data.presetIndex, 0, presets.Count - 1);

        ApplyPreset(currentPresetIndex, false);
    }
}