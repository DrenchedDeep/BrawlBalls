using Core.ActionMaps;
using MainMenu;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    [RequireComponent(typeof(Image))]
    public class ActionMapSpriteIcon : MonoBehaviour
    {
        [SerializeField] private ESpriteInputRequest request;
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
            transform.root.GetComponent<MainMenuPlayer>().OnDeviceChanged += SetActionMap;
        }

        private void OnDestroy()
        {
            transform.root.GetComponent<MainMenuPlayer>().OnDeviceChanged -= SetActionMap;
        }

        public void SetActionMap(InputSpriteActionMap map)
        {
            _image.sprite = map.GetSpriteByEnum(request);
        }
    }
}
