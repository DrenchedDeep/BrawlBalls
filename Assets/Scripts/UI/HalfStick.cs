using UnityEngine;

namespace UI
{
    public class HalfStick : Joystick
    {
        protected override void Start()
        {
            base.Start();
            radius.x *= 0.66f;
            Vector2 center = new Vector2(0.5f, 0);
            background.pivot = center;
            handle.anchorMin = center;
            handle.anchorMax =  center;
            handle.pivot = center;
            handle.anchoredPosition = Vector2.zero;
        }

        protected override void SetAnchorPosition(Vector2 pos)
        {
            if (pos.y < 0) pos.y = 0;
            base.SetAnchorPosition(pos);
        }

        protected override void HandleInput(float magnitude, Vector2 normalised)
        {
            if (normalised.y < 0) normalised.y = 0;
            base.HandleInput(magnitude, normalised);
        }
    }
}
