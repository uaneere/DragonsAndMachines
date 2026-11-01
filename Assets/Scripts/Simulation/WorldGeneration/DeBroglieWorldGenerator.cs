using System;
using System.Collections.Generic;
using DeBroglie;
using DeBroglie.Models;
using DeBroglie.Topo;
using DragonsAndMachines.Core;
using DragonsAndMachines.World;
using DeBroglie.Wfc;

namespace DragonsAndMachines.WorldGeneration
{
    public sealed class DeBroglieWorldGenerator : IWorldGenerator
    {
        public WorldGenerationResult Generate(WorldGenerationConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.Width <= 0 || config.Height <= 0)
                return Fail(config, "Размер карты должен быть положительным.");
            if (!IsValidFrequency(config.PlainsFrequency) ||
                !IsValidFrequency(config.ForestFrequency) ||
                !IsValidFrequency(config.WaterFrequency) ||
                !IsValidFrequency(config.MountainFrequency) ||
                !IsValidFrequency(config.MineFrequency))
                return Fail(config, "Частоты местности должны быть конечными числами больше нуля.");

            Exception lastError = null;
            for (var attempt = 0; attempt < Math.Max(1, config.MaxAttempts); attempt++)
            {
                var attemptSeed = unchecked(config.Seed + attempt * 9973);
                try
                {
                    var generated = RunOnce(config, attemptSeed);
                    if (generated != null)
                    {
                        generated.Message = attempt == 0
                            ? "Карта сгенерирована."
                            : $"Карта сгенерирована после {attempt + 1} попыток.";
                        return generated;
                    }
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            return Fail(config, lastError != null
                ? $"Генерация не удалась: {lastError.Message}"
                : "WFC не смог согласовать соседство тайлов. Измените seed.");
        }

        private static WorldGenerationResult RunOnce(WorldGenerationConfig config, int seed)
        {
            const TerrainType P = TerrainType.Plains;
            const TerrainType F = TerrainType.Forest;
            const TerrainType W = TerrainType.Water;
            const TerrainType M = TerrainType.Mountain;
            const TerrainType N = TerrainType.Mine;
            var sampleData = new[]
            {
                new[] { P, P, P, F, F, F, P, P },
                new[] { P, F, F, F, P, P, M, M },
                new[] { P, F, P, P, P, M, N, M },
                new[] { W, W, P, P, P, M, M, P },
                new[] { W, W, W, P, P, P, P, P },
                new[] { P, W, P, F, F, P, P, P },
                new[] { P, P, P, F, P, P, M, P },
                new[] { P, P, P, P, P, M, N, M }
            };
            var sampleTopology = new GridTopology(DirectionSet.Hexagonal2d, sampleData[0].Length, sampleData.Length, false, false);
            var sample = TopoArray.Create(sampleData, sampleTopology);
            var model = new AdjacentModel(sample.ToTiles());
            model.SetFrequency(new Tile(P), config.PlainsFrequency);
            model.SetFrequency(new Tile(F), config.ForestFrequency);
            model.SetFrequency(new Tile(W), config.WaterFrequency);
            model.SetFrequency(new Tile(M), config.MountainFrequency);
            model.SetFrequency(new Tile(N), config.MineFrequency);

            var topology = new GridTopology(DirectionSet.Hexagonal2d, config.Width, config.Height, false, false);
            var rng = new Random(seed);
            var options = new TilePropagatorOptions
            {
                ModelConstraintAlgorithm = ModelConstraintAlgorithm.Ac4,

                BacktrackType = BacktrackType.Backjump,
                MaxBacktrackDepth = 400,
                RandomDouble = rng.NextDouble,
                IndexPickerType = IndexPickerType.HeapMinEntropy,
                TilePickerType = TilePickerType.Weighted
            };

            var propagator = new TilePropagator(model, topology, options);
            var status = propagator.Run();
            if (status != Resolution.Decided)
                return null;

            var values = propagator.ToValueArray<TerrainType>();
            var result = new WorldGenerationResult
            {
                Success = true,
                Seed = config.Seed,
                Width = config.Width,
                Height = config.Height
            };

            for (var y = 0; y < config.Height; y++)
            {
                for (var x = 0; x < config.Width; x++)
                {
                    var terrain = values.Get(x, y);
                    result.Terrains[new HexCoord(x, y)] = terrain;
                }
            }

            if (!HasTerrain(result, TerrainType.Forest) ||
                !HasTerrain(result, TerrainType.Mine) ||
                !HasTerrain(result, TerrainType.Water))
                return null;

            return result;
        }

        private static bool HasTerrain(WorldGenerationResult result, TerrainType terrain)
        {
            foreach (var pair in result.Terrains)
            {
                if (pair.Value == terrain)
                    return true;
            }

            return false;
        }

        private static bool IsValidFrequency(double value)
        {
            return value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static WorldGenerationResult Fail(WorldGenerationConfig config, string message)
        {
            return new WorldGenerationResult
            {
                Success = false,
                Message = message,
                Seed = config.Seed,
                Width = config.Width,
                Height = config.Height
            };
        }
    }
}
