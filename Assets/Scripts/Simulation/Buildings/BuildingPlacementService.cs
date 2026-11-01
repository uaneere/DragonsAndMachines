using System;
using System.Collections.Generic;
using DragonsAndMachines.Core;
using DragonsAndMachines.Economy;
using DragonsAndMachines.World;

namespace DragonsAndMachines.Buildings
{
    public sealed class BuildingPlacementService
    {
        private readonly WorldState _world;
        private readonly IReadOnlyDictionary<string, BuildingDefinition> _buildings;

        public BuildingPlacementService(WorldState world, IReadOnlyDictionary<string, BuildingDefinition> buildings)
        {
            _world = world;
            _buildings = buildings;
        }

        public bool CanPlace(BuildingDefinition definition, HexCellState cell, out string reason)
        {
            reason = null;
            if (definition == null)
            {
                reason = "Неизвестный тип здания.";
                return false;
            }

            if (cell == null)
            {
                reason = "Клетка не найдена.";
                return false;
            }

            if (!string.IsNullOrEmpty(cell.BuildingInstanceId))
            {
                reason = "На клетке уже есть здание.";
                return false;
            }

            if (cell.OwnerId != _world.LocalPlayerId)
            {
                reason = "Строить можно только на стартовой территории.";
                return false;
            }

            var terrainOk = false;
            if (definition.AllowedTerrains != null)
            {
                foreach (var terrain in definition.AllowedTerrains)
                {
                    if (terrain == cell.Terrain)
                    {
                        terrainOk = true;
                        break;
                    }
                }
            }

            if (!terrainOk)
            {
                reason = "Местность не подходит для этого здания.";
                return false;
            }

            if (!string.IsNullOrEmpty(definition.RequiredSourceTypeId))
            {
                if (cell.Source == null || cell.Source.TypeId != definition.RequiredSourceTypeId)
                {
                    reason = "Нет подходящего источника.";
                    return false;
                }
            }

            if (!_world.Economy.Has(definition.BuildCost))
            {
                reason = "Недостаточно ресурсов для строительства.";
                return false;
            }

            return true;
        }

        public OperationResult TryPlace(string buildingTypeId, HexCoord coord)
        {
            if (!_buildings.TryGetValue(buildingTypeId, out var definition))
                return OperationResult.Fail("unknown_building", "Неизвестный тип здания.");
            if (!_world.TryGetCell(coord, out var cell))
                return OperationResult.Fail("missing_cell", "Клетка не найдена.");
            if (!CanPlace(definition, cell, out var reason))
                return OperationResult.Fail("placement_rejected", reason);

            var spend = _world.Economy.TrySpend(definition.BuildCost);
            if (!spend.Success)
                return spend;

            var instance = new BuildingInstance
            {
                Id = Guid.NewGuid().ToString("N"),
                TypeId = definition.TypeId,
                CellId = cell.Id,
                TicksUntilRun = definition.Recipe != null ? Math.Max(1, definition.Recipe.DurationTicks) : 1
            };
            cell.BuildingInstanceId = instance.Id;
            _world.Buildings[instance.Id] = instance;
            return OperationResult.Ok(instance.Id);
        }
    }
}
