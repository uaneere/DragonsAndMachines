namespace DragonsAndMachines.Buildings
{
    public sealed class BuildingInstance
    {
        public string Id { get; set; }
        public string TypeId { get; set; }
        public string CellId { get; set; }
        public string LastOperationMessage { get; set; }
        public int TicksUntilRun { get; set; }
    }
}
