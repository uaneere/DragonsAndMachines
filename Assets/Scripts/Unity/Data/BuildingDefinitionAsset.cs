using System.Collections.Generic;
using DragonsAndMachines.Buildings;
using DragonsAndMachines.World;
using UnityEngine;

namespace DragonsAndMachines.Unity.Data
{
    [CreateAssetMenu(menuName = "DragonsAndMachines/Building", fileName = "Building")]
    public sealed class BuildingDefinitionAsset : ScriptableObject
    {
        public string TypeId;
        public string DisplayName;
        [TextArea] public string Description;
        public BuildingRole Role;
        public TerrainType[] AllowedTerrains;
        public string RequiredSourceTypeId;
        public string ExtractedResourceId;
        public int ExtractAmount = 1;
        public int DurationTicks = 2;

        public BuildingDefinition ToDefinition(IReadOnlyDictionary<string, int> cost, ProductionRecipe recipe)
        {
            return new BuildingDefinition(
                TypeId,
                DisplayName,
                Description,
                Role,
                AllowedTerrains,
                string.IsNullOrEmpty(RequiredSourceTypeId) ? null : RequiredSourceTypeId,
                cost,
                recipe,
                ExtractedResourceId,
                ExtractAmount);
        }
    }
}
