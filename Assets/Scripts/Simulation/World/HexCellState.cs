using DragonsAndMachines.Core;
using DragonsAndMachines.Economy;

namespace DragonsAndMachines.World
{
    public sealed class HexCellState
    {
        public string Id { get; set; }
        public HexCoord Coord { get; set; }
        public TerrainType Terrain { get; set; }
        public string OwnerId { get; set; }
        public ResourceSourceState Source { get; set; }
        public string BuildingInstanceId { get; set; }
    }
}
