using DragonsAndMachines.Core;
using UnityEngine;

namespace DragonsAndMachines.Unity.Presentation
{
    public static class HexLayout
    {
        public const float Size = 1.05f;

        public static Vector3 ToWorld(HexCoord coord)
        {
            var radius = Size * 0.95f;
            var x = Mathf.Sqrt(3f) * radius * (coord.Q - coord.R * 0.5f);
            var z = 1.5f * radius * coord.R;
            return new Vector3(x, 0f, z);
        }

        public static Vector3 MapCenter(int width, int height)
        {
            var a = ToWorld(new HexCoord(0, 0));
            var b = ToWorld(new HexCoord(width - 1, height - 1));
            return (a + b) * 0.5f;
        }
    }
}
