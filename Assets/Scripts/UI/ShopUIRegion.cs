using UnityEngine;

namespace NarutoAutoBattle.View
{
    public static class ShopUIRegion
    {
        public const float PanelHeight = 150f;

        public static Rect PanelRect { get; private set; }

        public static void SetPanelRect(Rect rect)
        {
            PanelRect = rect;
        }

        public static bool ContainsScreenPoint(Vector2 screenPosition)
        {
            if (PanelRect.width <= 0f)
                return false;

            var guiPoint = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            return PanelRect.Contains(guiPoint);
        }
    }
}
