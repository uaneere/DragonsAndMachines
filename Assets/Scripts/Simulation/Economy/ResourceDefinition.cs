namespace DragonsAndMachines.Economy
{
    public sealed class ResourceDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string ColorHex { get; }

        public ResourceDefinition(string id, string displayName, string colorHex)
        {
            Id = id;
            DisplayName = displayName;
            ColorHex = colorHex;
        }
    }

    public static class ResourceIds
    {
        public const string Wood = "wood";
        public const string Stone = "stone";
        public const string Water = "water";
        public const string Planks = "planks";
    }
}
