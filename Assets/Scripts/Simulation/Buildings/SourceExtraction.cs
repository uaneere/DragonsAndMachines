using DragonsAndMachines.Core;
using DragonsAndMachines.Economy;

namespace DragonsAndMachines.Buildings
{
    public static class SourceExtraction
    {
        public static OperationResult TryExtract(ResourceSourceState source, int requested, out int extracted)
        {
            extracted = 0;
            if (source == null)
                return OperationResult.Fail("missing_source", "На клетке нет источника.");
            if (requested <= 0)
                return OperationResult.Fail("invalid_amount", "Объём добычи должен быть положительным.");

            switch (source.Kind)
            {
                case SourceKind.Flow:
                    extracted = System.Math.Min(requested, source.FlowPower);
                    return extracted > 0
                        ? OperationResult.Ok()
                        : OperationResult.Fail("no_flow", "Потоковый источник не даёт мощности.");
                case SourceKind.Renewable:
                case SourceKind.Depletable:
                    if (source.Stock <= 0)
                        return OperationResult.Fail("depleted", "Источник исчерпан.");
                    var effective = System.Math.Max(1, (int)System.Math.Round(requested * source.Efficiency));
                    extracted = System.Math.Min(effective, source.Stock);
                    source.Stock -= extracted;
                    return OperationResult.Ok();
                default:
                    return OperationResult.Fail("unknown_source", "Неизвестный тип источника.");
            }
        }

        public static void TickRegen(ResourceSourceState source)
        {
            if (source == null || source.Kind != SourceKind.Renewable)
                return;
            if (source.RegenPerTick <= 0 || source.Stock >= source.MaxStock)
                return;
            source.Stock = System.Math.Min(source.MaxStock, source.Stock + source.RegenPerTick);
        }
    }
}
