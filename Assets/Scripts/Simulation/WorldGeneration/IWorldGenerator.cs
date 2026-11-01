using System.Collections.Generic;
using DragonsAndMachines.Core;
using DragonsAndMachines.World;

namespace DragonsAndMachines.WorldGeneration
{
    public sealed class WorldGenerationConfig
    {
        public int Width { get; set; } = 24;
        public int Height { get; set; } = 24;
        public int Seed { get; set; } = 42;
        public double PlainsFrequency { get; set; } = 5.0;
        public double ForestFrequency { get; set; } = 2.2;
        public double WaterFrequency { get; set; } = 1.6;
        public double MountainFrequency { get; set; } = 1.3;
        public double MineFrequency { get; set; } = 0.6;
        public int MaxAttempts { get; set; } = 8;
        public int StartingRadius { get; set; } = 3;
    }

    public sealed class WorldGenerationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Seed { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<HexCoord, TerrainType> Terrains { get; } = new Dictionary<HexCoord, TerrainType>();
    }

    public interface IWorldGenerator
    {
        WorldGenerationResult Generate(WorldGenerationConfig config);
    }
}
