﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AssetStore.Joystick_Pack.Scripts.Base
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public float Horizontal { get { return (snapX) ? SnapFloat(Input.x, AxisOptions.Horizontal) : Input.x; } }
        public float Vertical { get { return (snapY) ? SnapFloat(Input.y, AxisOptions.Vertical) : Input.y; } }
        public Vector2 Direction { get { return new Vector2(Horizontal, Vertical); } }


        public float DeadZone
        {
            get { return deadZone; }
            set { deadZone = Mathf.Abs(value); }
        }

        public AxisOptions AxisOptions { get { return AxisOptions; } set { axisOptions = value; } }
        public bool SnapX { get { return snapX; } set { snapX = value; } }
        public bool SnapY { get { return snapY; } set { snapY = value; } }

        [SerializeField] private float handleRange = 1;
        [SerializeField] private float deadZone = 0;
        [SerializeField] private AxisOptions axisOptions = AxisOptions.Both;
        [SerializeField] private bool snapX = false;
        [SerializeField] private bool snapY = false;

        [SerializeField] protected RectTransform background = null;
        [SerializeField] protected RectTransform handle = null;
        private RectTransform baseRect = null;

        private Canvas canvas;
        private Camera cam;

        protected Vector2 radius;
        public Vector2 Input { get; private set; }

        [SerializeField] private Image colorImg;
    

        protected virtual void Start()
        {
            radius = background.sizeDelta / 2;
            DeadZone = deadZone;
            baseRect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                Debug.LogError("The Joystick is not placed inside a canvas");

            Vector2 center = new Vector2(0.5f, 0.5f);
            background.pivot = center;
            handle.anchorMin = center;
            handle.anchorMax = center;
            handle.pivot = center;
            handle.anchoredPosition = Vector2.zero;
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            cam = null;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                cam = canvas.worldCamera;

            Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        
            Input = (eventData.position - position) / (radius * canvas.scaleFactor);
            FormatInput();
            HandleInput(Input.magnitude, Input.normalized);
            SetAnchorPosition(Input  * handleRange * radius);
        }

        public virtual void SetInput(Vector2 input)
        {
            Input = input;
            SetAnchorPosition(Input  * handleRange * radius);
        }

        protected virtual void SetAnchorPosition(Vector2 pos)
        {
            handle.anchoredPosition = pos;
        }

        public virtual void HandleInput(float magnitude, Vector2 normalised)
        {
            if (magnitude > deadZone)
            {
                if (magnitude > 1)
                    Input = normalised;
            }
            else
                Input = Vector2.zero;
        }

        private void FormatInput()
        {
            if (axisOptions == AxisOptions.Horizontal)
                Input = new Vector2(Input.x, 0f);
            else if (axisOptions == AxisOptions.Vertical)
                Input = new Vector2(0f, Input.y);
        }

        private float SnapFloat(float value, AxisOptions snapAxis)
        {
            if (value == 0)
                return value;

            if (axisOptions == AxisOptions.Both)
            {
                float angle = Vector2.Angle(Input, Vector2.up);
                if (snapAxis == AxisOptions.Horizontal)
                {
                    if (angle < 22.5f || angle > 157.5f)
                        return 0;
                    else
                        return (value > 0) ? 1 : -1;
                }
                else if (snapAxis == AxisOptions.Vertical)
                {
                    if (angle > 67.5f && angle < 112.5f)
                        return 0;
                    else
                        return (value > 0) ? 1 : -1;
                }
                return value;
            }
            else
            {
                if (value > 0)
                    return 1;
                if (value < 0)
                    return -1;
            }
            return 0;
        }



        public virtual void OnPointerUp(PointerEventData eventData)
        {
            Input = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, cam, out localPoint))
            {
                Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;
                return localPoint - (background.anchorMax * baseRect.sizeDelta) + pivotOffset;
            }
            return Vector2.zero;
        }

        private void OnEnable()
        {
            colorImg.color = Color.white;
            Input = Vector2.zero;
            SetAnchorPosition(Input  * handleRange * radius);
        }

        private void OnDisable()
        {
            colorImg.color = Color.grey;
            SetAnchorPosition(Input  * handleRange * radius);
        }
    
    }

    public enum AxisOptions { Both, Horizontal, Vertical }
}