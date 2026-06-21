using System.Text;
using NarutoAutoBattle.Units;

namespace NarutoAutoBattle.View
{
    public static class UnitTooltipFormatter
    {
        public static string FromUnit(Unit unit, int sellValue)
        {
            if (unit?.Data == null)
                return string.Empty;

            return FromData(unit.Data, unit.StarLevel, sellValue);
        }

        public static string FromData(UnitData data, int starLevel = 1, int? sellValue = null)
        {
            if (data == null)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"{data.displayName}  ★{starLevel}");
            sb.AppendLine($"Cost: {data.cost}g{(sellValue.HasValue ? $"  |  Sell: {sellValue.Value}g" : "")}");
            sb.AppendLine($"HP: {ScaleStat(data.baseHealth, starLevel)}  ATK: {ScaleStat(data.baseAttack, starLevel)}");
            sb.AppendLine($"Range: {data.attackRange:0.#}  Speed: {data.attackSpeed:0.#}  Move: {data.moveSpeed:0.#}");

            if (!string.IsNullOrEmpty(data.jutsuName))
                sb.AppendLine($"Jutsu: {data.jutsuName} ({data.jutsuCooldown:0.#}s cd)");

            if (data.clans != null && data.clans.Length > 0)
            {
                sb.Append("Clans: ");
                for (int i = 0; i < data.clans.Length; i++)
                {
                    if (data.clans[i] == null)
                        continue;
                    if (i > 0)
                        sb.Append(", ");
                    sb.Append(data.clans[i].displayName);
                }
            }

            return sb.ToString().TrimEnd();
        }

        static int ScaleStat(int baseStat, int starLevel)
        {
            float multiplier = UnityEngine.Mathf.Pow(1.8f, starLevel - 1);
            return UnityEngine.Mathf.RoundToInt(baseStat * multiplier);
        }
    }
}
