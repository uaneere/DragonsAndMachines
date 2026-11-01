using System;
using System.Collections.Generic;

namespace DragonsAndMachines.Economy
{
    public sealed class ResourceInventory
    {
        private readonly Dictionary<string, int> _amounts = new Dictionary<string, int>(StringComparer.Ordinal);

        public IReadOnlyDictionary<string, int> Amounts => _amounts;

        public int Get(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
                return 0;
            return _amounts.TryGetValue(resourceId, out var value) ? value : 0;
        }

        public void Set(string resourceId, int amount)
        {
            if (string.IsNullOrEmpty(resourceId))
                throw new ArgumentException("Идентификатор ресурса пуст.", nameof(resourceId));
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Запас не может быть отрицательным.");
            _amounts[resourceId] = amount;
        }
    }
}
