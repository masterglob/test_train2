using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class SimpleSwitch
{
    public int Spline1Id = -1;
    public int Spline2Id = -1;
    public int Spline1CloseKnotId = -1;
    public int Spline2CloseKnotId = -1;
    public int Spline1Knot1 = -1;
    public int Spline1Knot2 = -1;
    public int Spline2Knot1 = -1;
    public int Spline2Knot2 = -1;
    public SwIndicator Swi = null;

    private bool HasDirectPos = true;

    public bool IsDirect() {  return HasDirectPos; }

    public void SetDirect(bool direct)
    {
        HasDirectPos = direct;
        if (Swi != null)
        {
            Swi.SetDirect(direct);
        }
    }

    public void InvertDirect()
    {
        SetDirect(!HasDirectPos);
    }

    public int SelectSpline(bool s1, bool isFwd)
    {
        // Check if this is a closed end
        if (isFwd)
        {
            // Dead end on S1/FWD?
            if (Spline1CloseKnotId == Spline1Knot2)
            {
                // Debug.Log("Dead end S1/F");
                return Spline2Id;
            }
            // Dead end on S2/FWD?
            if (Spline2CloseKnotId == Spline2Knot2)
            {
                // Debug.Log("Dead end S2/F");
                return Spline1Id;
            }

        }
        else
        {
            // Dead end on S1/BWD?
            if (Spline1CloseKnotId == Spline1Knot1)
            {
                // Debug.Log("Dead end S1/B");
                return Spline2Id;
            }
            // Dead end on S2/BWD?
            if (Spline2CloseKnotId == Spline2Knot1)
            {
                // Debug.Log("Dead end S2/B");
                return Spline1Id;
            }
        }

        return s1 ? Spline2Id : Spline1Id;
    }

    public override string ToString()
    {
        string close1 = Spline1CloseKnotId < 0 ? "" : $"(*{Spline1CloseKnotId})";
        string close2 = Spline2CloseKnotId < 0 ? "" : $"(*{Spline2CloseKnotId})";
        return $"SimpleSwitch( " +
               $"S1={Spline1Id}/{Spline1Knot1}/{Spline1Knot2}{close1}, " +
               $"S2={Spline2Id}/{Spline2Knot1}/{Spline2Knot2}{close2})";
    }

    public bool Compare(SimpleSwitch other)
    {
        return (Spline1Id == other.Spline1Id &&
               Spline2Id == other.Spline2Id &&
               Spline1Knot1 == other.Spline1Knot1 &&
               Spline1Knot2 == other.Spline1Knot2 &&
               Spline2Knot1 == other.Spline2Knot1 &&
               Spline2Knot2 == other.Spline2Knot2)
               ||
               (Spline1Id == other.Spline2Id &&
               Spline2Id == other.Spline1Id &&
               Spline1Knot1 == other.Spline2Knot1 &&
               Spline1Knot2 == other.Spline2Knot2 &&
               Spline2Knot1 == other.Spline1Knot1 &&
               Spline2Knot2 == other.Spline1Knot2);
    }

    // Find the closest SwIndicator and assigns it
    public void AssignSwi(SplineContainer splineContainer, SwIndicator[] indicators)
    {
        if (indicators == null || indicators.Length == 0)
        {
            Swi = null;
            return;
        }

        SplineKnotIndex knotIndex = this.GetDirectKnot();
        var knot = splineContainer.Splines[knotIndex.Spline][knotIndex.Knot];
        Vector3 switchPos = splineContainer.transform.TransformPoint(knot.Position);

        float minDistance = float.MaxValue;
        SwIndicator closest = null;

        foreach (var indicator in indicators)
        {
            if (indicator == null) continue;

            float distance = Vector3.Distance(switchPos, indicator.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = indicator;
            }
        }

        Swi = closest;
        if (Swi != null)
        {
            Debug.Log(
                $"[SimpleSwitch] Assigned indicator '{Swi.name}' to switch {this}. " 
            );
        }
        else
        {
            Debug.LogWarning(
                $"[SimpleSwitch] No suitable SwIndicator found for switch at {switchPos}."
            );
        }
    }

    private void Reverse()
    {
        (Spline1Id, Spline2Id) = (Spline2Id, Spline1Id);
        (Spline1CloseKnotId, Spline2CloseKnotId) = (Spline2CloseKnotId, Spline1CloseKnotId);
        (Spline1Knot1, Spline1Knot2, Spline2Knot1, Spline2Knot2) = 
            (Spline2Knot1, Spline2Knot2, Spline1Knot1, Spline1Knot2);
    }

    public void Normalize()
    {
        if (Spline1Id > Spline2Id)
            Reverse();
    }

    public SplineKnotIndex GetDirectKnot()
    {
        if (Spline1Knot1 < Spline1Knot2)
            return new SplineKnotIndex(Spline1Id, Spline1Knot1);
        else
            return new SplineKnotIndex(Spline1Id, Spline1Knot2);
    }

    public SplineKnotIndex GetDeviateKnot()
    {
        if (Spline2Knot1 < Spline2Knot2)
            return new SplineKnotIndex(Spline2Id, Spline2Knot1);
        else
            return new SplineKnotIndex(Spline2Id, Spline2Knot2);
    }
}
