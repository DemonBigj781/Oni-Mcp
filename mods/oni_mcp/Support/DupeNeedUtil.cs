using System;
using System.Collections.Generic;
using Klei.AI;

namespace OniMcp.Support
{
    public static class DupeNeedUtil
    {
        private static readonly string[] CoreNeedIds =
        {
            "Stress",
            "Calories",
            "Stamina",
            "Bladder",
            "Breath",
            "Temperature",
            "RadiationBalance",
            "BionicInternalBattery",
            "BionicOxygenTank",
            "BionicOil",
            "BionicGunk"
        };

        public static float GetAmountValue(MinionIdentity dupe, string id)
        {
            if (dupe == null || string.IsNullOrEmpty(id))
                return 0f;

            if (string.Equals(id, "Stress", StringComparison.OrdinalIgnoreCase))
            {
                var stress = GetStressAmount(dupe);
                if (stress != null)
                    return SafeFloat(stress.value);
            }

            var amounts = dupe.GetComponent<Amounts>();
            if (amounts != null)
            {
                try
                {
                    return SafeFloat(amounts.GetValue(id));
                }
                catch { }

                foreach (var amount in amounts.ModifierList)
                {
                    if (amount?.amount == null)
                        continue;
                    if (string.Equals(amount.amount.Id, id, StringComparison.OrdinalIgnoreCase))
                        return SafeFloat(amount.value);
                }
            }

            return 0f;
        }

        private static float SafeFloat(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 0f;
            return value;
        }

        public static AmountInstance GetStressAmount(MinionIdentity dupe)
        {
            if (dupe == null)
                return null;

            try
            {
                var stressMonitor = dupe.GetSMI<StressMonitor.Instance>();
                if (stressMonitor?.stress != null)
                    return stressMonitor.stress;
            }
            catch { }

            try
            {
                return Db.Get().Amounts.Stress.Lookup(dupe.gameObject);
            }
            catch
            {
                return null;
            }
        }

        public static Dictionary<string, float> GetCoreNeedValues(MinionIdentity dupe)
        {
            var result = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in CoreNeedIds)
                result[id] = GetAmountValue(dupe, id);
            return result;
        }
    }
}
