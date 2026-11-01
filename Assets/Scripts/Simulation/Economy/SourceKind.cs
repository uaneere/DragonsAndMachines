namespace DragonsAndMachines.Economy
{
    public enum SourceKind
    {
        Renewable = 1,
        Depletable = 2,
        Flow = 3
    }

    public static class SourceTypeIds
    {
        public const string Forest = "forest";
        public const string Mine = "mine";
        public const string River = "river";
    }

    public sealed class ResourceSourceDefinition
    {
        public string TypeId { get; }
        public string DisplayName { get; }
        public SourceKind Kind { get; }
        public string ResourceId { get; }
        public int InitialStock { get; }
        public int RegenPerTick { get; }
        public int FlowPower { get; }

        public ResourceSourceDefinition(
            string typeId,
            string displayName,
            SourceKind kind,
            string resourceId,
            int initialStock,
            int regenPerTick,
            int flowPower)
        {
            TypeId = typeId;
            DisplayName = displayName;
            Kind = kind;
            ResourceId = resourceId;
            InitialStock = initialStock;
            RegenPerTick = regenPerTick;
            FlowPower = flowPower;
        }
    }

    public sealed class ResourceSourceState
    {
        public string TypeId { get; set; }
        public string ResourceId { get; set; }
        public SourceKind Kind { get; set; }
        public int Stock { get; set; }
        public int MaxStock { get; set; }
        public int RegenPerTick { get; set; }
        public int FlowPower { get; set; }

        public double Efficiency =>
            Kind == SourceKind.Depletable && MaxStock > 0
                ? System.Math.Max(0.15, Stock / (double)MaxStock)
                : 1.0;
    }
}
