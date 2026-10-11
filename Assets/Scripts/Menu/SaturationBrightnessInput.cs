using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaturationBrightnessInput :
    MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private ColorPickerController picker;
    [SerializeField] private RawImage colorSquare;

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateSelection(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateSelection(eventData);
    }

    private void UpdateSelection(PointerEventData eventData)
    {
        RectTransform rect = colorSquare.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            return;
        }

        Rect bounds = rect.rect;

        float saturation = Mathf.Clamp01(
            (localPoint.x - bounds.xMin) / bounds.width);

        float brightness = Mathf.Clamp01(
            (localPoint.y - bounds.yMin) / bounds.height);

        picker.SetSaturationBrightness(saturation, brightness);
    }
}