using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.UI
{
    public class PlayerCard : MonoBehaviour
    {
    
        [SerializeField] private Image backgroundImg;
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private TextMeshProUGUI playerLevel;
        [SerializeField] private Vector2 childDimensions;
        [SerializeField] private Vector2 parentDimensions;
        
        
        private static readonly Color LocalPlayerColor = new (0,0.7f,0);
        private static readonly Color OtherPlayerColor = new (0.5f,0.5f,0.5f);
        public void UpdatePlayer(string pName, string pLevel, bool isLocalPlayer, bool isChild)
        {
            gameObject.SetActive(true);
            //Get the name and level...
            playerName.text = pName;
            playerLevel.text = pLevel;

            backgroundImg.color = isLocalPlayer ? LocalPlayerColor : OtherPlayerColor;
            
            ((RectTransform) transform).sizeDelta= isChild?childDimensions:parentDimensions;
        }
        
        public void RemovePlayer()
        {
            gameObject.SetActive(false);
        }
    }
}
