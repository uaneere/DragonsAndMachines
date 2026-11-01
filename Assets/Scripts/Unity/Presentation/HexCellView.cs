using DragonsAndMachines.Core;
using UnityEngine;

namespace DragonsAndMachines.Unity.Presentation
{
    public sealed class HexCellView : MonoBehaviour
    {
        public HexCoord Coord { get; private set; }

        private Renderer _renderer;
        private GameObject _buildingMarker;
        private GameObject _ownershipHatch;
        private Color _baseColor;

        public void Setup(HexCoord coord, Color color, Mesh mesh, Material material)
        {
            Coord = coord;
            _baseColor = color;
            _renderer = GetComponent<MeshRenderer>();
            if (_renderer == null)
                _renderer = gameObject.AddComponent<MeshRenderer>();
            var filter = GetComponent<MeshFilter>();
            if (filter == null)
                filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            _renderer.material = new Material(material) { color = color };
            var collider = GetComponent<MeshCollider>();
            if (collider == null)
                collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }

        public void SetSelected(bool selected)
        {
            if (_renderer == null)
                return;
            _renderer.material.color = selected ? Color.Lerp(_baseColor, Color.white, 0.45f) : _baseColor;
        }

        public void SetOwnershipHatch(bool visible, Mesh mesh, Material material)
        {
            if (!visible)
            {
                if (_ownershipHatch != null)
                    _ownershipHatch.SetActive(false);
                return;
            }

            if (_ownershipHatch == null)
            {
                _ownershipHatch = new GameObject("UnownedHatch");
                _ownershipHatch.transform.SetParent(transform, false);
                var filter = _ownershipHatch.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                var renderer = _ownershipHatch.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            _ownershipHatch.SetActive(true);
        }

        public void SetBuilding(bool present, Color color)
        {
            if (!present)
            {
                if (_buildingMarker != null)
                    _buildingMarker.SetActive(false);
                return;
            }

            if (_buildingMarker == null)
            {
                _buildingMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _buildingMarker.name = "Building";
                _buildingMarker.transform.SetParent(transform, false);
                _buildingMarker.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                _buildingMarker.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
                var col = _buildingMarker.GetComponent<Collider>();
                if (col != null)
                    Destroy(col);
            }

            _buildingMarker.SetActive(true);
            _buildingMarker.GetComponent<Renderer>().material.color = color;
        }
    }
}
