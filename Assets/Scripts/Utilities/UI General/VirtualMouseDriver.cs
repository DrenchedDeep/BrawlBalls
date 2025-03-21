using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

namespace Utilities.UI_General
{
    
    [RequireComponent(typeof(VirtualMouseInput))]
    public class VirtualMouseDriver : MonoBehaviour
    {
        private VirtualMouseInput _input;
        private Rect _bounds;
        private Canvas _canvas;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _input = GetComponent<VirtualMouseInput>();
            PlayerInput root = transform.root.GetComponent<PlayerInput>();
            _canvas = GetComponent<Canvas>();
            OnDeviceChanged(root);

        }

        void OnDeviceChanged(PlayerInput input)
        {
            if (input.currentControlScheme is "Controller")
            {
                enabled = true;
                _canvas.enabled = true;
            }
            else
            {
                _canvas.enabled = false;
                enabled = false;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            //transform.localScale = Vector3.one * 1 / _canvas.localScale.x;
            Vector2 mousePosition = _input.virtualMouse.position.value;
            mousePosition.x = Mathf.Clamp(mousePosition.x, _bounds.xMin, Screen.width + _bounds.xMax);
            mousePosition.y = Mathf.Clamp(mousePosition.y, _bounds.yMin, Screen.height + _bounds.yMax);
            InputState.Change(_input.virtualMouse.position, mousePosition);
            InputState.Change(Pointer.current, mousePosition);
        }
    }
}
