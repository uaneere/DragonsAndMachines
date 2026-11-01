using System.Collections.Generic;
using DragonsAndMachines.Buildings;
using DragonsAndMachines.Economy;
using DragonsAndMachines.World;

namespace DragonsAndMachines.Integration
{
    public sealed class PrototypeCatalog
    {
        public IReadOnlyDictionary<string, ResourceDefinition> Resources { get; }
        public IReadOnlyDictionary<string, ResourceSourceDefinition> Sources { get; }
        public IReadOnlyDictionary<string, BuildingDefinition> Buildings { get; }

        public PrototypeCatalog(
            IEnumerable<ResourceDefinition> resources,
            IEnumerable<ResourceSourceDefinition> sources,
            IEnumerable<BuildingDefinition> buildings)
        {
            Resources = Index(resources, r => r.Id);
            Sources = Index(sources, s => s.TypeId);
            Buildings = Index(buildings, b => b.TypeId);
        }

        public static PrototypeCatalog CreateDefault()
        {
            var resources = new[]
            {
                new ResourceDefinition(ResourceIds.Wood, "Древесина", "#8B5A2B"),
                new ResourceDefinition(ResourceIds.Stone, "Камень", "#9A9A9A"),
                new ResourceDefinition(ResourceIds.Water, "Вода", "#3A7CA5"),
                new ResourceDefinition(ResourceIds.Planks, "Доски", "#C4A574")
            };

            var sources = new[]
            {
                new ResourceSourceDefinition(SourceTypeIds.Forest, "Лес", SourceKind.Renewable, ResourceIds.Wood, 80, 2, 0),
                new ResourceSourceDefinition(SourceTypeIds.Mine, "Шахта", SourceKind.Depletable, ResourceIds.Stone, 160, 0, 0),
                new ResourceSourceDefinition(SourceTypeIds.River, "Река", SourceKind.Flow, ResourceIds.Water, 0, 0, 5)
            };

            var woodCost = Cost((ResourceIds.Wood, 8), (ResourceIds.Stone, 4));
            var buildings = new[]
            {
                new BuildingDefinition(
                    BuildingTypeIds.LumberCamp,
                    "Лесозаготовка",
                    "Добывает древесину из лесного источника.",
                    BuildingRole.Extraction,
                    new[] { TerrainType.Forest },
                    SourceTypeIds.Forest,
                    woodCost,
                    new ProductionRecipe(new ResourceAmount[0], new[] { new ResourceAmount(ResourceIds.Wood, 4) }, 2),
                    ResourceIds.Wood,
                    4),
                new BuildingDefinition(
                    BuildingTypeIds.Quarry,
                    "Каменоломня",
                    "Добывает камень из шахты на карте.",
                    BuildingRole.Extraction,
                    new[] { TerrainType.Mine },
                    SourceTypeIds.Mine,
                    Cost((ResourceIds.Wood, 6), (ResourceIds.Stone, 2)),
                    new ProductionRecipe(new ResourceAmount[0], new[] { new ResourceAmount(ResourceIds.Stone, 3) }, 2),
                    ResourceIds.Stone,
                    3),
                new BuildingDefinition(
                    BuildingTypeIds.WaterIntake,
                    "Водозабор",
                    "Забирает воду из реки.",
                    BuildingRole.Extraction,
                    new[] { TerrainType.Water },
                    SourceTypeIds.River,
                    Cost((ResourceIds.Wood, 5), (ResourceIds.Stone, 5)),
                    new ProductionRecipe(new ResourceAmount[0], new[] { new ResourceAmount(ResourceIds.Water, 5) }, 2),
                    ResourceIds.Water,
                    5),
                new BuildingDefinition(
                    BuildingTypeIds.Sawmill,
                    "Лесопилка",
                    "Перерабатывает древесину в доски.",
                    BuildingRole.Processing,
                    new[] { TerrainType.Plains, TerrainType.Forest, TerrainType.Mountain },
                    null,
                    Cost((ResourceIds.Wood, 10), (ResourceIds.Stone, 6)),
                    new ProductionRecipe(
                        new[] { new ResourceAmount(ResourceIds.Wood, 2) },
                        new[] { new ResourceAmount(ResourceIds.Planks, 1) },
                        3),
                    ResourceIds.Planks,
                    1)
            };

            return new PrototypeCatalog(resources, sources, buildings);
        }

        private static Dictionary<string, int> Cost(params (string id, int amount)[] items)
        {
            var dict = new Dictionary<string, int>();
            foreach (var item in items)
                dict[item.id] = item.amount;
            return dict;
        }

        private static Dictionary<string, T> Index<T>(IEnumerable<T> items, System.Func<T, string> key)
        {
            var dict = new Dictionary<string, T>();
            foreach (var item in items)
                dict[key(item)] = item;
            return dict;
        }
    }
}
