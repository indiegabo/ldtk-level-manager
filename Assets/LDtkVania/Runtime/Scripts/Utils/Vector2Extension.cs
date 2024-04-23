using UnityEngine;

namespace LDtkVania
{
    public static class Vector2Extension
    {
        /// <summary>
        /// Alters the Vector2 values to signs defining -1, 0 or 1.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector2 Sign(this Vector2 vector)
        {
            Vector2 signed = new Vector2();

            if (vector.x != 0)
                signed.x = vector.x > 0 ? 1 : -1;

            if (vector.y != 0)
                signed.y = vector.y > 0 ? 1 : -1;

            return signed;
        }

        /// <summary>
        /// Centralizes the vector vertices in a Unit. E.g. 16.987654 becomes 16.5. E.g. 1.1254 becomes 1.5.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector2 CenterInUnit(this Vector2 vector)
        {
            return new Vector2(Mathf.FloorToInt(vector.x) + 0.5f, Mathf.FloorToInt(vector.y) + 0.5f);
        }

        /// <summary>
        /// Rotates the Vector2 by a given amount of degrees
        /// </summary>
        /// <param name="v"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }
    }
}