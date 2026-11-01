using DragonsAndMachines.Core;
using DragonsAndMachines.Economy;
using DragonsAndMachines.World;

namespace DragonsAndMachines.Buildings
{
    public interface IBuildingBehavior
    {
        OperationResult ExecuteCycle(BuildingInstance instance, BuildingDefinition definition, HexCellState cell, IEconomyService economy);
    }

    public sealed class ExtractionBehavior : IBuildingBehavior
    {
        public OperationResult ExecuteCycle(BuildingInstance instance, BuildingDefinition definition, HexCellState cell, IEconomyService economy)
        {
            var extract = SourceExtraction.TryExtract(cell.Source, definition.ExtractAmount, out var got);
            if (!extract.Success)
                return extract;
            return economy.TryAdd(cell.Source.ResourceId, got);
        }
    }

    public sealed class RecipeBehavior : IBuildingBehavior
    {
        public OperationResult ExecuteCycle(BuildingInstance instance, BuildingDefinition definition, HexCellState cell, IEconomyService economy)
        {
            if (definition.Recipe == null)
                return OperationResult.Fail("missing_recipe", "У здания нет рецепта.");

            var inputs = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var input in definition.Recipe.Inputs)
                inputs[input.ResourceId] = inputs.TryGetValue(input.ResourceId, out var amount) ? amount + input.Amount : input.Amount;

            var spend = economy.TrySpend(inputs);
            if (!spend.Success)
                return spend;
            foreach (var output in definition.Recipe.Outputs)
            {
                var add = economy.TryAdd(output.ResourceId, output.Amount);
                if (!add.Success)
                    return add;
            }

            return OperationResult.Ok();
        }
    }

    public static class BuildingBehaviorFactory
    {
        public static IBuildingBehavior Create(BuildingDefinition definition)
        {
            if (definition.Role == BuildingRole.Processing)
                return new RecipeBehavior();
            return new ExtractionBehavior();
        }
    }
}
