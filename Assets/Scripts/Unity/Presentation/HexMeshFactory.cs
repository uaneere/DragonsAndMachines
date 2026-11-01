using System.Collections.Generic;
using UnityEngine;

namespace DragonsAndMachines.Unity.Presentation
{
    public static class HexMeshFactory
    {
        public static Mesh CreatePrism(float radius, float height)
        {
            const int sides = 6;
            var vertices = new Vector3[sides * 2 + 2];
            var topCenter = sides * 2;
            var bottomCenter = sides * 2 + 1;
            vertices[topCenter] = new Vector3(0f, height, 0f);
            vertices[bottomCenter] = Vector3.zero;
            for (var i = 0; i < sides; i++)
            {
                var angle = Mathf.Deg2Rad * (60f * i + 30f);
                var x = Mathf.Cos(angle) * radius;
                var z = Mathf.Sin(angle) * radius;
                vertices[i] = new Vector3(x, height, z);
                vertices[i + sides] = new Vector3(x, 0f, z);
            }

            var triangles = new int[sides * 12];
            var t = 0;
            for (var i = 0; i < sides; i++)
            {
                var next = (i + 1) % sides;
                triangles[t++] = topCenter;
                triangles[t++] = next;
                triangles[t++] = i;
                triangles[t++] = bottomCenter;
                triangles[t++] = i + sides;
                triangles[t++] = next + sides;
                triangles[t++] = i;
                triangles[t++] = next;
                triangles[t++] = i + sides;
                triangles[t++] = next;
                triangles[t++] = next + sides;
                triangles[t++] = i + sides;
            }

            var mesh = new Mesh { name = "HexPrism" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateHatchOverlay(float radius, float height)
        {
            const float stripeWidth = 0.065f;
            const float stripeSpacing = 0.22f;
            var hex = new List<Vector2>(6);
            for (var i = 0; i < 6; i++)
            {
                var angle = Mathf.Deg2Rad * (60f * i + 30f);
                hex.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var extent = radius * Mathf.Sqrt(2f);
            for (var offset = -extent; offset < extent; offset += stripeSpacing)
            {
                var stripe = Clip(hex, offset, true);
                stripe = Clip(stripe, offset + stripeWidth, false);
                if (stripe.Count < 3)
                    continue;

                var center = Vector2.zero;
                foreach (var point in stripe)
                    center += point;
                center /= stripe.Count;
                var centerIndex = vertices.Count;
                vertices.Add(new Vector3(center.x, height, center.y));
                for (var i = 0; i < stripe.Count; i++)
                    vertices.Add(new Vector3(stripe[i].x, height, stripe[i].y));

                for (var i = 0; i < stripe.Count; i++)
                {
                    var current = centerIndex + 1 + i;
                    var next = centerIndex + 1 + (i + 1) % stripe.Count;
                    triangles.Add(centerIndex);
                    triangles.Add(next);
                    triangles.Add(current);
                }
            }

            var mesh = new Mesh { name = "HexOwnershipHatch" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static List<Vector2> Clip(List<Vector2> polygon, float boundary, bool keepGreater)
        {
            var clipped = new List<Vector2>();
            if (polygon.Count == 0)
                return clipped;

            var previous = polygon[polygon.Count - 1];
            var previousValue = previous.x + previous.y;
            var previousInside = keepGreater ? previousValue >= boundary : previousValue <= boundary;
            foreach (var current in polygon)
            {
                var currentValue = current.x + current.y;
                var currentInside = keepGreater ? currentValue >= boundary : currentValue <= boundary;
                if (currentInside != previousInside)
                {
                    var amount = (boundary - previousValue) / (currentValue - previousValue);
                    clipped.Add(Vector2.LerpUnclamped(previous, current, amount));
                }

                if (currentInside)
                    clipped.Add(current);
                previous = current;
                previousValue = currentValue;
                previousInside = currentInside;
            }

            return clipped;
        }
    }
}
