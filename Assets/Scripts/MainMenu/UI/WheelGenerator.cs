using Stats;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.UI
{
    public class WheelGenerator : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private Image setBackground;
        [SerializeField] private WheelItem template;
        [SerializeField] private ShopItemStats[] items;
        
        
        private void OnEnable()
        {
            SpawnObjects();
            Destroy(this);
        }

        private void SpawnObjects()
        {
            foreach (ShopItemStats stats in items)
            {
                //WheelItem myItem = Instantiate(template, parent);
                //myItem.SetItem(stats);
            }
            
        }
    }
}
