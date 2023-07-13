using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [SerializeField] private Button quickPlayButton;
    
    // Start is called before the first frame update
    void Start()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            quickPlayButton.interactable = true;
        };
        AuthenticationService.Instance.SignedOut += () =>
        {
            //Also make sure to send the user back, if they've gotten too far. and pull up a pop-up
            quickPlayButton.interactable = false;
        };
    }
}
