using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button quickPlayButton;
    
        void OnEnable()
        {
            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignedOut += OnSignedOut;
        }

        
        private void OnDisable()
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignedOut -= OnSignedOut;
        }
        
        private void OnSignedIn()
        {
            quickPlayButton.interactable = true;
            Debug.LogWarning("SIGNED IN");
        }

        
        private void OnSignedOut()
        {
            //Also make sure to send the user back, if they've gotten too far. and pull up a pop-up
            quickPlayButton.interactable = false;
            Debug.LogWarning("SIGNED OUT");
        }

   
        
        
    }
}
