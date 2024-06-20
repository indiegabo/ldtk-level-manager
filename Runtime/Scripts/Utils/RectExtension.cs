using UnityEngine;

namespace LDtkLevelManager.Utils
{
    public static class RectExtension
    {
        public static void Expand(this ref Rect rect, Rect target)
        {
            rect.xMin = Mathf.Min(rect.min.x, target.min.x);
            rect.yMin = Mathf.Min(rect.min.y, target.min.y);
            rect.xMax = Mathf.Max(rect.max.x, target.max.x);
            rect.yMax = Mathf.Max(rect.max.y, target.max.y);
        }
    }
}