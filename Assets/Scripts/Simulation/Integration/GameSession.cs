using System;
using DragonsAndMachines.Buildings;
using DragonsAndMachines.Core;
using DragonsAndMachines.Economy;
using DragonsAndMachines.World;
using DragonsAndMachines.WorldGeneration;

namespace DragonsAndMachines.Integration
{
    public sealed class GameSession
    {
        public PrototypeCatalog Catalog { get; }
        public LocalWorldStateProvider WorldProvider { get; } = new LocalWorldStateProvider();
        public LocalPlayerSessionService Session { get; } = new LocalPlayerSessionService();
        public IWorldGenerator Generator { get; }
        public WorldGenerationConfig GenerationConfig { get; }
        public BuildingPlacementService Placement { get; private set; }
        public HexCoord? SelectedCell { get; set; }
        public event Action WorldChanged;

        public WorldState World => WorldProvider.Current;

        public GameSession(PrototypeCatalog catalog = null, IWorldGenerator generator = null, WorldGenerationConfig config = null)
        {
            Catalog = catalog ?? PrototypeCatalog.CreateDefault();
            Generator = generator ?? new DeBroglieWorldGenerator();
            GenerationConfig = config ?? new WorldGenerationConfig();
        }

        public WorldState GenerateWorld()
        {
            var generated = Generator.Generate(GenerationConfig);
            var economy = new EconomyService(Catalog.Resources.Values);
            economy.TryAdd(ResourceIds.Wood, 40);
            economy.TryAdd(ResourceIds.Stone, 30);
            economy.TryAdd(ResourceIds.Water, 10);

            var world = WorldFactory.FromGeneration(generated, GenerationConfig, Catalog.Sources, economy);
            WorldProvider.Current = world;
            Placement = new BuildingPlacementService(world, Catalog.Buildings);
            SelectedCell = null;
            WorldChanged?.Invoke();
            return world;
        }

        public OperationResult PlaceBuilding(string buildingTypeId, HexCoord coord)
        {
            if (Placement == null)
                return OperationResult.Fail("no_world", "Мир ещё не создан.");
            var result = Placement.TryPlace(buildingTypeId, coord);
            if (result.Success)
                WorldChanged?.Invoke();

            return result;
        }

        public void Tick(int ticks = 1)
        {
            if (World == null || ticks <= 0)
                return;

            for (var i = 0; i < ticks; i++)
            {
                foreach (var cell in World.Cells.Values)
                    SourceExtraction.TickRegen(cell.Source);

                foreach (var building in World.Buildings.Values)
                {
                    var definition = Catalog.Buildings[building.TypeId];
                    var cell = World.GetCell(building.CellId);
                    if (cell == null)
                        continue;

                    var duration = definition.Recipe != null ? Math.Max(1, definition.Recipe.DurationTicks) : 1;
                    building.TicksUntilRun--;
                    if (building.TicksUntilRun > 0)
                        continue;

                    building.TicksUntilRun = duration;
                    var behavior = BuildingBehaviorFactory.Create(definition);
                    var result = behavior.ExecuteCycle(building, definition, cell, World.Economy);
                    building.LastOperationMessage = result.Success ? "производство выполнено" : result.Message;
                }
            }

            WorldChanged?.Invoke();
        }
    }
}
