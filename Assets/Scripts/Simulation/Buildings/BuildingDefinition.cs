using System.Collections.Generic;
using DragonsAndMachines.World;

namespace DragonsAndMachines.Buildings
{
    public sealed class ResourceAmount
    {
        public string ResourceId { get; }
        public int Amount { get; }

        public ResourceAmount(string resourceId, int amount)
        {
            ResourceId = resourceId;
            Amount = amount;
        }
    }

    public sealed class ProductionRecipe
    {
        public IReadOnlyList<ResourceAmount> Inputs { get; }
        public IReadOnlyList<ResourceAmount> Outputs { get; }
        public int DurationTicks { get; }

        public ProductionRecipe(IReadOnlyList<ResourceAmount> inputs, IReadOnlyList<ResourceAmount> outputs, int durationTicks)
        {
            Inputs = inputs;
            Outputs = outputs;
            DurationTicks = durationTicks;
        }
    }

    public enum BuildingRole
    {
        Extraction = 1,
        Processing = 2
    }

    public sealed class BuildingDefinition
    {
        public string TypeId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public BuildingRole Role { get; }
        public IReadOnlyList<TerrainType> AllowedTerrains { get; }
        public string RequiredSourceTypeId { get; }
        public IReadOnlyDictionary<string, int> BuildCost { get; }
        public ProductionRecipe Recipe { get; }
        public string ExtractedResourceId { get; }
        public int ExtractAmount { get; }

        public BuildingDefinition(
            string typeId,
            string displayName,
            string description,
            BuildingRole role,
            IReadOnlyList<TerrainType> allowedTerrains,
            string requiredSourceTypeId,
            IReadOnlyDictionary<string, int> buildCost,
            ProductionRecipe recipe,
            string extractedResourceId,
            int extractAmount)
        {
            TypeId = typeId;
            DisplayName = displayName;
            Description = description;
            Role = role;
            AllowedTerrains = allowedTerrains;
            RequiredSourceTypeId = requiredSourceTypeId;
            BuildCost = buildCost;
            Recipe = recipe;
            ExtractedResourceId = extractedResourceId;
            ExtractAmount = extractAmount;
        }
    }

    public static class BuildingTypeIds
    {
        public const string LumberCamp = "lumber_camp";
        public const string Quarry = "quarry";
        public const string WaterIntake = "water_intake";
        public const string Sawmill = "sawmill";
    }
}
