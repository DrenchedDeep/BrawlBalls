using UnityEngine;

namespace MainMenu.UI
{
    public class LoadingHelper : MonoBehaviour
    {
        private static LoadingHelper Instance { get; set; }

        private void Awake()
        {
            
            Debug.LogWarning("Find a better way to make this script, should not be a singleton", gameObject);
            
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            gameObject.SetActive(false);
        }

        public static void Activate()
        {
            Instance.gameObject.SetActive(true);
        }

        public static void Deactivate()
        {
            Instance.gameObject.SetActive(false);
        }

    }
}
