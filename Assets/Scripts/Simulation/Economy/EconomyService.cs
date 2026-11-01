using System;
using System.Collections.Generic;
using DragonsAndMachines.Core;

namespace DragonsAndMachines.Economy
{
    public interface IEconomyService
    {
        IReadOnlyDictionary<string, ResourceDefinition> Resources { get; }
        ResourceInventory Inventory { get; }
        int GetAmount(string resourceId);
        bool Has(IReadOnlyDictionary<string, int> cost);
        OperationResult TryAdd(string resourceId, int amount);
        OperationResult TrySpend(string resourceId, int amount);
        OperationResult TrySpend(IReadOnlyDictionary<string, int> cost);
    }
    public sealed class EconomyService : IEconomyService
    {
        private readonly Dictionary<string, ResourceDefinition> _resources;
        private readonly ResourceInventory _inventory = new ResourceInventory();

        public EconomyService(IEnumerable<ResourceDefinition> resources)
        {
            _resources = new Dictionary<string, ResourceDefinition>(StringComparer.Ordinal);
            foreach (var resource in resources)
                _resources[resource.Id] = resource;
        }

        public IReadOnlyDictionary<string, ResourceDefinition> Resources => _resources;

        public ResourceInventory Inventory => _inventory;

        public int GetAmount(string resourceId) => _inventory.Get(resourceId);

        public bool Has(IReadOnlyDictionary<string, int> cost)
        {
            if (cost == null)
                return true;
            foreach (var pair in cost)
            {
                if (pair.Value < 0)
                    return false;
                if (GetAmount(pair.Key) < pair.Value)
                    return false;
            }

            return true;
        }

        public OperationResult TryAdd(string resourceId, int amount)
        {
            var check = Validate(resourceId, amount);
            if (!check.Success)
                return check;
            _inventory.Set(resourceId, _inventory.Get(resourceId) + amount);
            return OperationResult.Ok();
        }

        public OperationResult TrySpend(string resourceId, int amount)
        {
            var check = Validate(resourceId, amount);
            if (!check.Success)
                return check;
            var current = _inventory.Get(resourceId);
            if (current < amount)
                return OperationResult.Fail("insufficient_resources", $"Недостаточно ресурса '{resourceId}': нужно {amount}, есть {current}.");
            _inventory.Set(resourceId, current - amount);
            return OperationResult.Ok();
        }

        public OperationResult TrySpend(IReadOnlyDictionary<string, int> cost)
        {
            if (cost == null)
                return OperationResult.Ok();
            foreach (var pair in cost)
            {
                if (pair.Value < 0)
                    return OperationResult.Fail("invalid_amount", "Стоимость не может быть отрицательной.");
                if (!_resources.ContainsKey(pair.Key))
                    return OperationResult.Fail("unknown_resource", $"Неизвестный ресурс '{pair.Key}'.");
            }

            if (!Has(cost))
                return OperationResult.Fail("insufficient_resources", "Недостаточно ресурсов для операции.");

            foreach (var pair in cost)
            {
                if (pair.Value == 0)
                    continue;
                var spend = TrySpend(pair.Key, pair.Value);
                if (!spend.Success)
                    return spend;
            }

            return OperationResult.Ok();
        }

        private OperationResult Validate(string resourceId, int amount)
        {
            if (string.IsNullOrEmpty(resourceId))
                return OperationResult.Fail("invalid_resource", "Идентификатор ресурса пуст.");
            if (!_resources.ContainsKey(resourceId))
                return OperationResult.Fail("unknown_resource", $"Неизвестный ресурс '{resourceId}'.");
            if (amount < 0)
                return OperationResult.Fail("invalid_amount", "Количество не может быть отрицательным.");
            if (amount == 0)
                return OperationResult.Fail("invalid_amount", "Количество должно быть больше нуля.");
            return OperationResult.Ok();
        }
    }
}
