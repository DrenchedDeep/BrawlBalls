using Core.ActionMaps;
using LocalMultiplayer;
using MainMenu;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    [RequireComponent(typeof(Image))]
    public class ActionMapSpriteIcon : MonoBehaviour
    {
#if !UNITY_ANDROID && !UNITY_IOS
        [SerializeField] private ESpriteInputRequest request;
        private Image _image;
        private void Awake()
        {
            _image = GetComponent<Image>();
            transform.root.GetComponent<LocalPlayer>().OnDeviceChanged += SetActionMap;
        }

        private void OnDestroy()
        {
            transform.root.GetComponent<LocalPlayer>().OnDeviceChanged -= SetActionMap;
        }

        public void SetActionMap(InputSpriteActionMap map)
        {
            _image.sprite = map.GetSpriteByEnum(request);
        }
#endif
    }
}
