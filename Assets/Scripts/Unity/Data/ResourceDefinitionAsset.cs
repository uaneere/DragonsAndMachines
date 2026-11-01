using DragonsAndMachines.Economy;
using UnityEngine;

namespace DragonsAndMachines.Unity.Data
{
    [CreateAssetMenu(menuName = "DragonsAndMachines/Resource", fileName = "Resource")]
    public sealed class ResourceDefinitionAsset : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public string ColorHex = "#FFFFFF";

        public ResourceDefinition ToDefinition() => new ResourceDefinition(Id, DisplayName, ColorHex);
    }
}
