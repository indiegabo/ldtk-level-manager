
using UnityEngine;

namespace LDtkVania.Utils
{
    public static class PolygonCollider2DExtension
    {
        /// <summary>
        /// Connects the PolygonCollider2D points in a square shape
        /// based on the size. 
        /// /// </summary>
        /// <param name="collider"></param>
        /// <param name="size"></param>
        public static void ShapeFromSize(this PolygonCollider2D collider, Vector2 size)
        {
            collider.points = new Vector2[] {
                new Vector2(size.x, size.y),
                new Vector2(0, size.y),
                new Vector2(0, 0),
                new Vector2(size.x, 0)
            };
        }
    }
}