using System;
using DragonsAndMachines.Buildings;
using DragonsAndMachines.Economy;
using DragonsAndMachines.Integration;
using DragonsAndMachines.World;
using UnityEngine;
using UnityEngine.UIElements;

namespace DragonsAndMachines.Unity.UI
{
    public sealed class GameHud
    {
        private readonly GameSession _session;
        private readonly VisualElement _root;
        private Label _resources;
        private Label _generation;
        private Label _cellInfo;
        private Label _buildingInfo;
        private Label _status;
        private IntegerField _seedField;
        private IntegerField _widthField;
        private IntegerField _heightField;
        private FloatField _plainsFrequency;
        private FloatField _forestFrequency;
        private FloatField _waterFrequency;
        private FloatField _mountainFrequency;
        private FloatField _mineFrequency;

        public GameHud(GameSession session, VisualElement root)
        {
            _session = session;
            _root = root;
            Build();
            _session.WorldChanged += Refresh;
            Refresh();
        }

        private void Build()
        {
            _root.style.flexGrow = 1;
            _root.pickingMode = PickingMode.Ignore;

            var top = Bar(FlexDirection.Row);
            _resources = Label(top, "", 16);
            _generation = Label(top, "", 12);
            _root.Add(top);

            var panel = Panel(Align.FlexStart);
            panel.style.width = 360;
            panel.style.maxHeight = Length.Percent(92);
            var left = new ScrollView();
            left.style.flexGrow = 1;
            panel.Add(left);

            left.Add(new Label("Генерация карты"));
            _seedField = new IntegerField("Seed") { value = _session.GenerationConfig.Seed };
            _widthField = new IntegerField("Ширина") { value = _session.GenerationConfig.Width };
            _heightField = new IntegerField("Высота") { value = _session.GenerationConfig.Height };
            left.Add(_seedField);
            left.Add(_widthField);
            left.Add(_heightField);
            _plainsFrequency = FrequencyField(left, "Частота равнин", _session.GenerationConfig.PlainsFrequency);
            _forestFrequency = FrequencyField(left, "Частота леса", _session.GenerationConfig.ForestFrequency);
            _waterFrequency = FrequencyField(left, "Частота воды", _session.GenerationConfig.WaterFrequency);
            _mountainFrequency = FrequencyField(left, "Частота гор", _session.GenerationConfig.MountainFrequency);
            _mineFrequency = FrequencyField(left, "Частота шахт", _session.GenerationConfig.MineFrequency);
            Button(left, "Сгенерировать карту", GenerateWorld);

            _cellInfo = Label(left, "Клетка не выбрана", 12);
            left.Add(new Label("Постройки"));
            foreach (var building in _session.Catalog.Buildings.Values)
            {
                var typeId = building.TypeId;
                Button(left, "Построить: " + building.DisplayName, () => Place(typeId));
            }

            _buildingInfo = Label(left, "", 12);
            _status = Label(left, "", 12);
            _root.Add(panel);
        }

        private static FloatField FrequencyField(VisualElement parent, string label, double value)
        {
            var field = new FloatField(label) { value = (float)value };
            parent.Add(field);
            return field;
        }

        private void GenerateWorld()
        {
            var config = _session.GenerationConfig;
            config.Seed = _seedField.value;
            config.Width = _widthField.value;
            config.Height = _heightField.value;
            config.PlainsFrequency = _plainsFrequency.value;
            config.ForestFrequency = _forestFrequency.value;
            config.WaterFrequency = _waterFrequency.value;
            config.MountainFrequency = _mountainFrequency.value;
            config.MineFrequency = _mineFrequency.value;

            if (config.Width <= 0 || config.Height <= 0)
            {
                SetStatus("Размеры карты должны быть положительными.");
                return;
            }

            if (!IsValidFrequency(config.PlainsFrequency) ||
                !IsValidFrequency(config.ForestFrequency) ||
                !IsValidFrequency(config.WaterFrequency) ||
                !IsValidFrequency(config.MountainFrequency) ||
                !IsValidFrequency(config.MineFrequency))
            {
                SetStatus("Частоты должны быть конечными числами больше нуля.");
                return;
            }

            var world = _session.GenerateWorld();
            SetStatus(world.GenerationMessage);
        }

        private static bool IsValidFrequency(double value)
        {
            return value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private void Place(string typeId)
        {
            if (!_session.SelectedCell.HasValue)
            {
                SetStatus("Сначала выберите клетку.");
                return;
            }

            var result = _session.PlaceBuilding(typeId, _session.SelectedCell.Value);
            SetStatus(result.ToString());
        }

        private void SetStatus(string message)
        {
            _status.text = message;
            Refresh();
        }

        public void Refresh()
        {
            if (_session.World == null)
                return;

            var world = _session.World;
            _generation.text = $"Карта {world.Width}×{world.Height}  seed {world.Seed}  {world.GenerationMessage}";
            var economy = world.Economy;
            _resources.text = $"Древесина {economy.GetAmount(ResourceIds.Wood)}   Камень {economy.GetAmount(ResourceIds.Stone)}   Вода {economy.GetAmount(ResourceIds.Water)}   Доски {economy.GetAmount(ResourceIds.Planks)}";

            if (_session.SelectedCell.HasValue && world.TryGetCell(_session.SelectedCell.Value, out var cell))
                _cellInfo.text = DescribeCell(cell);
            else
                _cellInfo.text = "Клетка не выбрана. ЛКМ — выбор, WASD/стрелки — камера, колёсико — масштаб, средняя кнопка — панорама.";

            var building = SelectedBuilding();
            _buildingInfo.text = building == null ? "Здания на выбранной клетке нет." : DescribeBuilding(building);
        }

        private BuildingInstance SelectedBuilding()
        {
            if (!_session.SelectedCell.HasValue || !_session.World.TryGetCell(_session.SelectedCell.Value, out var cell))
                return null;
            if (string.IsNullOrEmpty(cell.BuildingInstanceId))
                return null;
            return _session.World.Buildings.TryGetValue(cell.BuildingInstanceId, out var building) ? building : null;
        }

        private string DescribeCell(HexCellState cell)
        {
            var source = cell.Source == null
                ? "источника нет"
                : $"{cell.Source.DisplaySafe()} запас {cell.Source.Stock}/{cell.Source.MaxStock} мощность {cell.Source.FlowPower}";
            var owner = string.IsNullOrEmpty(cell.OwnerId) ? "ничья" : cell.OwnerId;
            return $"Клетка {cell.Coord}\nМестность {TerrainName(cell.Terrain)}\nВладелец {owner}\n{source}\nЗдание {(cell.BuildingInstanceId == null ? "нет" : "построено")}";
        }

        private string DescribeBuilding(BuildingInstance building)
        {
            var definition = _session.Catalog.Buildings[building.TypeId];
            var production = definition.Role == BuildingRole.Extraction
                ? $"добыча {definition.ExtractAmount} {definition.ExtractedResourceId}"
                : "древесина → доски";
            return $"{definition.DisplayName}\nПроизводство: {production}\nСостояние: {building.LastOperationMessage ?? "ожидает запуска"}";
        }

        private static string TerrainName(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Plains: return "равнина";
                case TerrainType.Forest: return "лес";
                case TerrainType.Water: return "вода";
                case TerrainType.Mountain: return "горы";
                case TerrainType.Mine: return "шахта";
                default: return "неизвестно";
            }
        }

        private static VisualElement Bar(FlexDirection direction)
        {
            var bar = new VisualElement();
            bar.style.flexDirection = direction;
            bar.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            bar.style.paddingLeft = 8;
            bar.style.paddingRight = 8;
            bar.style.paddingTop = 6;
            bar.style.paddingBottom = 6;
            bar.pickingMode = PickingMode.Position;
            return bar;
        }

        private static VisualElement Panel(Align align)
        {
            var panel = Bar(FlexDirection.Column);
            panel.style.alignItems = align;
            panel.style.marginTop = 8;
            panel.style.marginLeft = 8;
            panel.style.backgroundColor = new Color(0f, 0f, 0f, 0.62f);
            return panel;
        }

        private static Label Label(VisualElement parent, string text, int size)
        {
            var label = new Label(text);
            label.style.color = Color.white;
            label.style.fontSize = size;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginRight = 12;
            parent.Add(label);
            return label;
        }

        private static void Button(VisualElement parent, string text, Action click)
        {
            var button = new Button(click) { text = text };
            button.style.marginRight = 6;
            button.style.marginBottom = 4;
            parent.Add(button);
        }
    }

    internal static class SourceUi
    {
        public static string DisplaySafe(this ResourceSourceState source)
        {
            return source.TypeId + " / " + source.ResourceId;
        }
    }
}
