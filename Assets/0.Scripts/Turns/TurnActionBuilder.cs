using System.Collections.Generic;
using UnityEngine;

public static class TurnActionBuilder
{
    public static TurnActionInfo[] BuildActionArray(this IEnumerable<TurnActionInfo> progress)
    {
        List<TurnActionInfo> result = new ();

        try
        {
            foreach (TurnActionInfo currentAction in progress)
            {
                if (currentAction is null) continue;
                currentAction.GoNext();
                result.Add(currentAction);
            }
        }
        finally
        {
            for (int i = result.Count - 1; i >= 0; --i)
            {
                result[i].GoPrev();
            }
        }

        return result.ToArray();
    }


    
}
