using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Hover;

public class WheelItem : MonoBehaviour, IInfiniteScrollItem
{
    
    private static readonly Color DefaultColor = Color.black;
    private static readonly Color HighlightColor = Color.yellow;
    private static readonly Color SelectedColor = Color.red;
    
    [SerializeField] private Image outline;
    [SerializeField] private UIHoverScale scaler;
    
    
    bool isSelected = false;
    
    public void OnSelected()
    {
        outline.color = SelectedColor;
        isSelected = true;
    }

    public void OnDeselected()
    {
        outline.color = DefaultColor;
        isSelected = false;
    }

    public void OnHover()
    {
        scaler.OnPointerEnter(new PointerEventData(EventSystem.current));
        if (isSelected) return;
        outline.color = HighlightColor;
    }

    public void OnUnHover()
    {    
        scaler.OnPointerExit(new PointerEventData(EventSystem.current));
        if (isSelected) return;
        outline.color = DefaultColor;
    }
}
