using System;
using System.Collections.Generic;
using DragonsAndMachines.Buildings;
using DragonsAndMachines.Core;
using DragonsAndMachines.Economy;

namespace DragonsAndMachines.World
{
    public sealed class WorldState
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Seed { get; set; }
        public string LocalPlayerId { get; set; } = PlayerIds.Local;
        public string GenerationMessage { get; set; }
        public bool GenerationSucceeded { get; set; }

        public Dictionary<HexCoord, HexCellState> Cells { get; } = new Dictionary<HexCoord, HexCellState>();
        public Dictionary<string, BuildingInstance> Buildings { get; } = new Dictionary<string, BuildingInstance>(StringComparer.Ordinal);
        public ResourceInventory PlayerInventory => Economy.Inventory;
        public EconomyService Economy { get; set; }

        public bool TryGetCell(HexCoord coord, out HexCellState cell) => Cells.TryGetValue(coord, out cell);

        public HexCellState GetCell(string cellId)
        {
            foreach (var pair in Cells)
            {
                if (pair.Value.Id == cellId)
                    return pair.Value;
            }

            return null;
        }
    }

    public static class PlayerIds
    {
        public const string Local = "player_1";
    }

    public interface IWorldStateProvider
    {
        WorldState Current { get; }
    }

    public sealed class LocalWorldStateProvider : IWorldStateProvider
    {
        public WorldState Current { get; set; }
    }

    public interface IPlayerSessionService
    {
        string ActivePlayerId { get; }
        string ActivePlatform { get; }
    }

    public sealed class LocalPlayerSessionService : IPlayerSessionService
    {
        public string ActivePlayerId => PlayerIds.Local;
        public string ActivePlatform => "pc";
    }
}
