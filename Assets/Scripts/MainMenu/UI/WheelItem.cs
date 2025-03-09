using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Hover;
using Utilities.Layout;

namespace MainMenu.UI
{
    public class WheelItem : MonoBehaviour, IInfiniteScrollItem
    {
    
        private static readonly Color DefaultColor = Color.black;
        private static readonly Color HighlightColor = Color.yellow;
        private static readonly Color SelectedColor = Color.red;
    
        [SerializeField] private Image outline;
        [SerializeField] private UIHoverScale scaler;


        private bool _isSelected;
        private IInfiniteScrollItem _myClone;
    
        public void OnSelected()
        {
            _myClone?.OnSelected();
            outline.color = SelectedColor;
            _isSelected = true;
        }

        public void OnDeselected()
        {
            _myClone?.OnDeselected();
            outline.color = DefaultColor;
            _isSelected = false;
        }

        public void OnHover()
        {
            _myClone?.OnHover();
            scaler.OnPointerEnter(new PointerEventData(EventSystem.current));
            if (_isSelected) return;
            outline.color = HighlightColor;
        }

        public void OnUnHover()
        {    
            _myClone?.OnUnHover();
            scaler.OnPointerExit(new PointerEventData(EventSystem.current));
            if (_isSelected) return;
            outline.color = DefaultColor;
        }

        public void SetCloneReciever(IInfiniteScrollItem clone)
        {
            if (_myClone != null)
            {
                _myClone.SetCloneReciever(clone);
            }

            else
            {
                _myClone = clone;
            }
        }
    }
}
