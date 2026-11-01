using System.Collections.Generic;
using DragonsAndMachines.Core;
using DragonsAndMachines.Integration;
using DragonsAndMachines.World;
using UnityEngine;
using UnityEngine.Rendering;

namespace DragonsAndMachines.Unity.Presentation
{
    public sealed class IsoHexMapView : MonoBehaviour
    {
        private readonly Dictionary<HexCoord, HexCellView> _cells = new Dictionary<HexCoord, HexCellView>();
        private GameSession _session;
        private Mesh _hexMesh;
        private Mesh _hatchMesh;
        private Material _material;
        private Material _hatchMaterial;
        private HexCoord? _selected;
        private WorldState _builtWorld;

        public void Bind(GameSession session)
        {
            _session = session;
            _session.WorldChanged += Rebuild;
            if (_hexMesh == null)
                _hexMesh = HexMeshFactory.CreatePrism(HexLayout.Size * 0.95f, 0.28f);
            if (_hatchMesh == null)
                _hatchMesh = HexMeshFactory.CreateHatchOverlay(HexLayout.Size * 0.95f, 0.29f);
            if (_material == null)
                _material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            if (_hatchMaterial == null)
                _hatchMaterial = CreateHatchMaterial();
            Rebuild();
        }

        public void Rebuild()
        {
            if (_session?.World?.Cells == null)
                return;
            if (_builtWorld == _session.World)
            {
                RefreshMarkers();
                SetSelected(_session.SelectedCell);
                return;
            }

            foreach (Transform child in transform)
                Destroy(child.gameObject);
            _cells.Clear();
            _builtWorld = _session.World;

            foreach (var pair in _session.World.Cells)
            {
                var go = new GameObject(pair.Value.Id);
                go.transform.SetParent(transform, false);
                go.transform.position = HexLayout.ToWorld(pair.Key);
                var view = go.AddComponent<HexCellView>();
                view.Setup(pair.Key, TerrainColor(pair.Value), _hexMesh, _material);
                view.SetOwnershipHatch(
                    pair.Value.OwnerId != _session.World.LocalPlayerId,
                    _hatchMesh,
                    _hatchMaterial);
                view.SetBuilding(!string.IsNullOrEmpty(pair.Value.BuildingInstanceId), new Color(0.85f, 0.75f, 0.4f));
                _cells[pair.Key] = view;
            }

            SetSelected(_session.SelectedCell);
        }

        private void RefreshMarkers()
        {
            foreach (var pair in _cells)
            {
                if (!_session.World.TryGetCell(pair.Key, out var cell))
                    continue;
                pair.Value.SetBuilding(!string.IsNullOrEmpty(cell.BuildingInstanceId), new Color(0.85f, 0.75f, 0.4f));
                pair.Value.SetOwnershipHatch(
                    cell.OwnerId != _session.World.LocalPlayerId,
                    _hatchMesh,
                    _hatchMaterial);
            }
        }

        public void SetSelected(HexCoord? coord)
        {
            if (_selected.HasValue && _cells.TryGetValue(_selected.Value, out var previous))
                previous.SetSelected(false);
            _selected = coord;
            if (_selected.HasValue && _cells.TryGetValue(_selected.Value, out var current))
                current.SetSelected(true);
        }

        public bool TryPick(Ray ray, out HexCoord coord)
        {
            coord = default;
            if (Physics.Raycast(ray, out var hit, 500f))
            {
                var view = hit.collider.GetComponent<HexCellView>();
                if (view != null)
                {
                    coord = view.Coord;
                    return true;
                }
            }

            return false;
        }

        private static Color TerrainColor(HexCellState cell)
        {
            Color color;
            switch (cell.Terrain)
            {
                case TerrainType.Plains:
                    color = new Color(0.62f, 0.76f, 0.38f);
                    break;
                case TerrainType.Forest:
                    color = new Color(0.18f, 0.42f, 0.28f);
                    break;
                case TerrainType.Water:
                    color = new Color(0.16f, 0.38f, 0.62f);
                    break;
                case TerrainType.Mountain:
                    color = new Color(0.55f, 0.55f, 0.58f);
                    break;
                case TerrainType.Mine:
                    color = new Color(0.42f, 0.32f, 0.24f);
                    break;
                default:
                    color = Color.magenta;
                    break;
            }

            return color;
        }

        private static Material CreateHatchMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (shader == null)
                throw new System.InvalidOperationException("Не найден шейдер для штриховки закрытых клеток.");

            var material = new Material(shader);
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend"))
                material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend"))
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend"))
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite"))
                material.SetInt("_ZWrite", 0);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", new Color(0f, 0f, 0f, 0.48f));
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", new Color(0f, 0f, 0f, 0.48f));
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            return material;
        }
    }
}
