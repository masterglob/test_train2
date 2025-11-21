using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

using System.Collections.Generic;

public class IntersectionsMgr
{

    private SplineContainer splineContainer = null;

    private float[][] tValues;

    // Constructeur
    public IntersectionsMgr(SplineContainer splineContainer)
    {
        this.splineContainer = splineContainer;

        tValues = new float[splineContainer.Splines.Count][];
        // Compute table of position for each Knot
        for (int sI = 0; sI < splineContainer.Splines.Count; sI++)
        {
            Spline spline = splineContainer.Splines[sI];
            tValues[sI] = new float[spline.Count];
            var native = new NativeSpline(spline);

            for (int kI = 0; kI < spline.Count; kI++)
            {
                BezierKnot k = spline[kI];
                SplineUtility.GetNearestPoint(native, k.Position, out float3 nearest, out float t);
                tValues[sI][kI] = t; 
                Debug.Log($"Linked Knots from S{sI}/K{kI} : t={t}");
            }
        }

        // Todo : store this result!
        KnotLinkCollection links = splineContainer.KnotLinkCollection;
        for (int sI = 0; sI < splineContainer.Splines.Count; sI++)
        {
            Spline spline = splineContainer.Splines[sI];

            for (int kI = 0; kI < spline.Count; kI++)
            {
                SplineKnotIndex skI = new SplineKnotIndex(sI, kI);
                IReadOnlyList<SplineKnotIndex> lst = links.GetKnotLinks(skI);

                if (lst.Count > 1)
                {
                    string res = "";
                    foreach (var linkedKnot in lst)
                    {
                        res += $"(S{linkedKnot.Spline}/K{linkedKnot.Knot}), ";
                    }
                    Debug.Log($"Linked Knots from S{sI}/K{kI} : [{res}]");
                }
            }

        }
    }

    public float GetT(int splineIndex, int knotIndex)
    {
        if (splineIndex < 0 || splineIndex >= tValues.Length)
            return -1f;

        if (knotIndex < 0 || knotIndex >= tValues[splineIndex].Length)
            return -1f;

        return tValues[splineIndex][knotIndex];
    }

    public void showLinks()
    {
        if (splineContainer == null) return;
    }

    public int GetKnotIndex(int splineIndex, float t)
    {
        if (splineIndex < 0 || splineIndex >= tValues.Length)
            return -1;

        int knotCount = tValues[splineIndex].Length;

        if (knotCount == 0)
            return -1;

        // Parcours des t pour trouver l'intervalle
        for (int kI = 0; kI < knotCount - 1; kI++)
        {
            float t0 = GetT(splineIndex, kI);
            float t1 = GetT(splineIndex, kI + 1);

            if (t >= t0 && t < t1)
                return kI;
        }

        // Si t est supérieur ou égal au dernier t, on retourne le dernier index
        if (t >= GetT(splineIndex, knotCount - 1))
            return knotCount - 1;

        // Aucun intervalle trouvé
        return -1;
    }

}
