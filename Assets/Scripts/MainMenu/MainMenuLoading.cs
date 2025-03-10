using System;
using Loading;
using MainMenu.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace MainMenu
{
    public class MainMenuLoading : MonoBehaviour
    {
        
        private async void Start()
        {
            LoadingHelper.Instance.Activate();
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (ServicesInitializationException)
            {
                Debug.LogError("Failed to connect, implement UI");
                LoadingHelper.Instance.Deactivate();
                return;
            }
        }

        private void OnEnable()
        {
            
            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignedOut += OnSignedOut;
        }

        private void OnDisable()
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignedOut -= OnSignedOut;
        }

        private void OnSignedOut()
        {
            Debug.LogWarning("Player has been signed out!");
        }

        private void OnSignedIn()
        {
            Debug.LogWarning("Player has been signed in!");
            
        }
    }
}
