using Stats;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.UI
{
    [DefaultExecutionOrder(-100)]
    public class WheelGenerator : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private WheelItem template;
        [SerializeField] private ShopItemStats[] items; 
        
        /*
         * How do we get all of the object types....
         * We need:
         * Name
         * Icon,
         * Prefab,
         * object to replace??? << How do we know, if we're a Ball, Weapon or Ability? Do we enumerate? Type check? Have a Base class?
         * We know what the current podium is... Kinda... So when we choose a new object... We need to replace the one on the current podium
         * What happens when we want to "Swap"
         * How do we tell, which items are already being used.
         */
        
        
        private void OnEnable()
        {
            SpawnObjects();
            Destroy(this);
        }

        private void SpawnObjects()
        {
            foreach (ShopItemStats stats in items)
            {
                WheelItem item = Instantiate(template, parent);
                item.SetItem(stats);
            }
        }

       
    }
}
