using Managers.Local;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;
using Utilities.Hover;
using Utilities.Layout;

namespace MainMenu.UI
{
    [RequireComponent(typeof(PopupMenu))]
    public class DescriptionController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image backing;
        [SerializeField] private TextMeshProUGUI header;
        [SerializeField] private TextMeshProUGUI description;

        PopupMenu _popupMenu;
        private ShopItemStats _currentItem;
        private UIHoverScale _hoverScale;

        private void Awake()
        {
            _popupMenu = GetComponent<PopupMenu>();
            _hoverScale = GetComponent<UIHoverScale>();
        }

        public void OnItemGained(IInfiniteScrollItem newItem)
        {
            if (newItem is WheelItem item)
            {
                _currentItem = item.GetItem();
                DisplayCurrentItem();
                _popupMenu.Open();
            }
            else
            {
                Debug.LogError("Tried inserting an item that is not a WheelItem");
            }
        }

        public void DisplayCurrentItem()
        {
            icon.sprite = _currentItem.Stats.Icon;
            
            ResourceManager.Instance.GetRarityInformation(_currentItem.Stats.Rarity, out Color color, out Sprite sprite);
            color.a = 0.4f;
            backing.sprite = sprite;
            backing.color = color;
            
            header.text = _currentItem.Stats.name;
            description.text = _currentItem.Stats.Description;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _hoverScale.OnPointerExit(null);

            _hoverScale.Lock(true);
            _popupMenu.Close();
            
        }
    }
}
