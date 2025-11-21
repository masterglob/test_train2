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

    public int SelectSpline(bool s1)
    {
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
}
