using System.Collections.Generic;
using Managers.Local;
using Stats;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image icon;
        [SerializeField] private Image backing;

        private ShopItemStats _itemStats;
        private bool _isSelected;
        private bool _isClone;
        private readonly List<WheelItem> _targets = new();
    
        public void OnSelected()
        {
            if (_isClone)
            {
                _targets[0].OnSelected();
                return;
            }
            
            foreach (var item in _targets)
            {
                item.OnSelectedImplementation();
            }

            OnSelectedImplementation();
        }
        
        private void OnSelectedImplementation()
        {
            scaler.OnPointerEnter(new PointerEventData(EventSystem.current));
            outline.color = SelectedColor;
            _isSelected = true;
        }

        public void OnDeselected()
        {
            if (_isClone)
            {
                _targets[0].OnDeselected();
                return;
            }
            
            foreach (var item in _targets)
            {
                item.OnDeselectedImplementation();
            }

            OnDeselectedImplementation();
        }
        
        private void OnDeselectedImplementation()
        {
            scaler.OnPointerExit(new PointerEventData(EventSystem.current));
            outline.color = DefaultColor;
            _isSelected = false;
        }

        public void OnHover()
        {
            if (_isClone)
            {
                _targets[0].OnHover();
                return;
            }
            
            foreach (var item in _targets)
            {
                item.OnHoverImplementation();
            }

            OnHoverImplementation();
        }

        private void OnHoverImplementation()
        {
            scaler.OnPointerEnter(new PointerEventData(EventSystem.current));
            if (_isSelected) return;
            outline.color = HighlightColor;
        }
        public void OnUnHover()
        {
            if (_isClone)
            {
                _targets[0].OnUnHover();
                return;
            }
            
            foreach (var item in _targets)
            {
                item.OnUnHoverImplementation();
            }

            OnUnHoverImplementation();
        }

        private void OnUnHoverImplementation()
        {
            scaler.OnPointerExit(new PointerEventData(EventSystem.current));
            if (_isSelected) return;
            outline.color = DefaultColor;
        }


        public void ListenTo(IInfiniteScrollItem clone)
        {
            _isClone = true;
            WheelItem item = ((WheelItem)clone);
            item._targets.Add(this);
            _targets.Add(item);

            _itemStats = item._itemStats;
        }

        public void SetItem(ShopItemStats stats)
        {
            _itemStats = stats;

            titleText.text = stats.name;
            icon.sprite = stats.Stats.Icon;

            ResourceManager.Instance.GetRarityInformation(stats.Stats.Rarity, out _, out Sprite rarityIcon);

            backing.sprite = rarityIcon;

        }

        public ShopItemStats GetItem()
        {
            return _itemStats;
        }
    }
}
