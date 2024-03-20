using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Executors;
public static class ExecAutoInteract
{
    static ConditionFlag[] AllowedConditions = [ConditionFlag.NormalConditions, ConditionFlag.OnFreeTrial, ConditionFlag.Mounted];
    public static void Tick()
    {
        foreach(var x in Enum.GetValues<ConditionFlag>())
        {
            if (AllowedConditions.Contains(x)) continue;
            if (Svc.Condition[x]) return;
        }

    }
}
