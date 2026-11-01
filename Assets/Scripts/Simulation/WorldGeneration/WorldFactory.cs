using System.Collections.Generic;
using DragonsAndMachines.Core;
using DragonsAndMachines.Economy;
using DragonsAndMachines.World;

namespace DragonsAndMachines.WorldGeneration
{
    public static class WorldFactory
    {
        public static WorldState FromGeneration(
            WorldGenerationResult generated,
            WorldGenerationConfig config,
            IReadOnlyDictionary<string, ResourceSourceDefinition> sources,
            EconomyService economy)
        {
            var world = new WorldState
            {
                Width = generated.Width,
                Height = generated.Height,
                Seed = generated.Seed,
                GenerationSucceeded = generated.Success,
                GenerationMessage = generated.Message,
                Economy = economy
            };

            if (!generated.Success)
                return world;

            foreach (var pair in generated.Terrains)
            {
                var cell = new HexCellState
                {
                    Id = pair.Key.CellId,
                    Coord = pair.Key,
                    Terrain = pair.Value,
                    OwnerId = null,
                    Source = CreateSource(pair.Value, sources)
                };
                world.Cells[pair.Key] = cell;
            }

            AssignStartingTerritory(world, config.StartingRadius);
            return world;
        }

        private static ResourceSourceState CreateSource(
            TerrainType terrain,
            IReadOnlyDictionary<string, ResourceSourceDefinition> sources)
        {
            string typeId = null;
            if (terrain == TerrainType.Forest)
                typeId = SourceTypeIds.Forest;
            else if (terrain == TerrainType.Mine)
                typeId = SourceTypeIds.Mine;
            else if (terrain == TerrainType.Water)
                typeId = SourceTypeIds.River;

            if (typeId == null || !sources.TryGetValue(typeId, out var definition))
                return null;

            return new ResourceSourceState
            {
                TypeId = definition.TypeId,
                ResourceId = definition.ResourceId,
                Kind = definition.Kind,
                Stock = definition.InitialStock,
                MaxStock = definition.InitialStock,
                RegenPerTick = definition.RegenPerTick,
                FlowPower = definition.FlowPower
            };
        }

        public static void AssignStartingTerritory(WorldState world, int radius)
        {
            var center = new HexCoord(world.Width / 2, world.Height / 2);
            if (!world.Cells.ContainsKey(center))
            {
                foreach (var key in world.Cells.Keys)
                {
                    center = key;
                    break;
                }
            }

            var claimed = new HashSet<HexCoord>();
            var queue = new Queue<HexCoord>();
            queue.Enqueue(center);
            claimed.Add(center);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (Distance(center, current) >= radius)
                    continue;
                foreach (var neighbor in current.Neighbors())
                {
                    if (!world.Cells.ContainsKey(neighbor) || claimed.Contains(neighbor))
                        continue;
                    if (Distance(center, neighbor) > radius)
                        continue;
                    claimed.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            foreach (var coord in claimed)
                world.Cells[coord].OwnerId = world.LocalPlayerId;
        }

        private static int Distance(HexCoord from, HexCoord to)
        {
            var visited = new HashSet<HexCoord> { from };
            var queue = new Queue<(HexCoord Coord, int Dist)>();
            queue.Enqueue((from, 0));
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (item.Coord.Equals(to))
                    return item.Dist;
                foreach (var neighbor in item.Coord.Neighbors())
                {
                    if (!visited.Add(neighbor))
                        continue;
                    queue.Enqueue((neighbor, item.Dist + 1));
                }
            }

            return int.MaxValue;
        }
    }
}
