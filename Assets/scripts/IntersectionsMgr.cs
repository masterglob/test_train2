using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.UI;

using System.Collections.Generic;

public class IntersectionsMgr : MonoBehaviour
{

    public SplineContainer splineContainer = null;
    public Slider slider;
    public SwIndicator SWI1;

    private float[][] tValues;
    private int[] nbKnots; // idx=  spline;
    private Dictionary<SplineKnotIndex, SplineKnotIndex> kLinks;

    private Dictionary<SplineKnotIndex, SimpleSwitch> switches;

    // Constructeur
    public void Start()
    {
        if (slider == null || splineContainer == null) return;

        int nbSplines = splineContainer.Splines.Count;
        tValues = new float[nbSplines][];
        nbKnots = new int[nbSplines];
        kLinks = new Dictionary<SplineKnotIndex, SplineKnotIndex>();
        switches = new Dictionary<SplineKnotIndex, SimpleSwitch>();

        // Compute table of position for each Knot
        for (int sI = 0; sI < nbSplines; sI++)
        {
            Spline spline = splineContainer.Splines[sI];
            tValues[sI] = new float[spline.Count];
            var native = new NativeSpline(spline);

            nbKnots[sI] = spline.Count;
            for (int kI = 0; kI < spline.Count; kI++)
            {
                BezierKnot k = spline[kI];
                SplineUtility.GetNearestPoint(native, k.Position, out float3 nearest, out float t);
                tValues[sI][kI] = t; 
            }
        }

        /* 
         * 1: Find all connected Knots
         * 2: 2 successive connected knots on 2 splines define a connection between 2 tracks.
         */
        // Todo : store this result!
        KnotLinkCollection links = splineContainer.KnotLinkCollection;
        for (int sI = 0; sI < nbSplines; sI++)
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
                        if (linkedKnot.Spline != sI)
                            kLinks[skI] = linkedKnot;
                    }
                    // Debug.Log($"Linked Knots from S{sI}/K{kI} : [{res}]");
                }
            }
        }

        CreateSwitches();
    }

    public void SetGlobalDirect(bool direct)
    {
        if (slider == null)
        {
            Debug.LogError("Assign a slider in KbdMgr.");
            return;
        }

        slider.value = direct ? 0 : 1;
        Debug.Log($"Switch global to {slider.value}");

        if (SWI1 != null)
            SWI1.Text = direct ? "^" : ">";
    }

    private void CreateSwitches()
    {
        int nbSplines = splineContainer.Splines.Count;
        for (int sI = 0; sI < nbSplines; sI++)
        {
            Spline spline = splineContainer.Splines[sI];
            for (int kI = 0; kI < spline.Count; kI++)
            {
                BezierKnot k = spline[kI];
                if (getKnotLink(sI, kI, out SimpleSwitch simpleSwitch))
                {
                    simpleSwitch.Normalize();
                    switches[new SplineKnotIndex(sI, kI)] = simpleSwitch;
                    Debug.Log($"Add switch S{sI}K{kI} => S{simpleSwitch}");
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

    private int nextKi(int sI, int kI)
    {
        if (sI < 0 || sI >= nbKnots.Length) return -1;
        return (kI + 1) % nbKnots[sI];
    }

    public bool getKnotLink(int sI, int kI, out SimpleSwitch simpleSwitch)
    {
        simpleSwitch = new SimpleSwitch();

        // Check if Current position if between 2 joined knots.
        int kI2 = nextKi(sI, kI);
        if (!kLinks.TryGetValue(new SplineKnotIndex(sI, kI), out SplineKnotIndex linked1)
            || !kLinks.TryGetValue(new SplineKnotIndex(sI, kI2), out SplineKnotIndex linked2)
            || linked1.Spline != linked2.Spline
            || !isNear(linked2.Spline, linked1.Knot, linked2.Knot))
            return false;

        int sI2 = linked2.Spline;
        simpleSwitch.Spline1Id = sI;
        simpleSwitch.Spline2Id = sI2;
        simpleSwitch.Spline1Knot1 = kI;
        simpleSwitch.Spline1Knot2 = kI2;
        simpleSwitch.Spline2Knot1 = linked1.Knot;
        simpleSwitch.Spline2Knot2 = linked2.Knot;
        simpleSwitch.Spline1CloseKnotId = -1;
        simpleSwitch.Spline2CloseKnotId = -1;
        if (!splineContainer.Splines[sI].Closed)
        {
            if (kI == 0 || kI == nbKnots[sI] - 1)
                simpleSwitch.Spline1CloseKnotId = kI;
            if (kI2 == 0 || kI2 == nbKnots[sI] - 1)
                simpleSwitch.Spline1CloseKnotId = kI2;
        }
        if (!splineContainer.Splines[sI2].Closed)
        {
            if (linked1.Knot == 0 || linked1.Knot == nbKnots[sI2] - 1)
                simpleSwitch.Spline2CloseKnotId = linked1.Knot;
            if (linked2.Knot == 0 || linked2.Knot == nbKnots[sI2] - 1)
                simpleSwitch.Spline2CloseKnotId = linked2.Knot;
        }
        return true;
    }

    private bool isNear(int sI, int kI1, int kI2)
    {
        // TODO if not "Closed" => do not loop!
        return ((kI1 + 1) % nbKnots[sI] == kI2) || ((kI2 + 1) % nbKnots[sI] == kI1);
    }

    /**
     * isFwd is True if the direction on sI is from kI to kI+1
     */
    public int GetNewSplineId(int sI, int kI, bool isFwd)
    {
        if (!switches.TryGetValue(new SplineKnotIndex(sI, kI), out SimpleSwitch ss))
        {
            return sI;
        }

        // isFwd is relative to sI, but ss may be in another direction!
        if (ss.Spline1Id == sI)
        {
            // Check direction
            if (ss.Spline1Knot2 == kI)
            {
                // Debug.Log("Invert Dir on S1");
                isFwd = !isFwd;
            }
        }
        else
        {
            if (ss.Spline2Knot2 == kI)
            {
                // Debug.Log("Invert Dir on S2");
                isFwd = !isFwd;
            }
        }

        // Currently on a section that has 2 possible path
        // Search for switch managing this path

        bool choice = (int)slider.value != 0; // TODO, One slider per switch!
        // Debug.Log($"Select {choice} from S{ss}, isFwd={isFwd}");
        return ss.SelectSpline(choice, isFwd);
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
