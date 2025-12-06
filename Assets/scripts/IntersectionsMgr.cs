using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.UI;

using System.Collections.Generic;
using TMPro;

public class IntersectionsMgr : MonoBehaviour
{

    public SplineContainer splineContainer = null;
    public GameObject parentSwis;

    private SwIndicator[] allSWIs;
    public SwIndicator[] GetSWIs(){return allSWIs;}

    public TMP_Text UiErrMsg;

    private float[][] tValues;
    private int[] nbKnots; // idx=  spline;
    private Dictionary<SplineKnotIndex, SplineKnotIndex> kLinks;

    /**
     * For each Knot, an associated "SimpleSwitch" components, that 
     * only contains the Splines knots and dead ends.
     * Note that each "physical switch" exists in two instances here (one in each 
     * spline.)
     */
    private Dictionary<SplineKnotIndex, SimpleSwitch> switches;

    // For switch indicators
    public RectTransform uiPanel = null;
    public GameObject btnPrefab;
    public GameObject btnAckPrefab;
    private List<UIIndicator> uiIndicators = new List<UIIndicator>();
    private float yBtnOffset = 50f;
    private float xBtnOffset = 300f;

    // Constructeur
    public void Start()
    {
        if (splineContainer == null || parentSwis == null)
        {
            Debug.LogError("Missing inputs in IntersectionsMgr");
            return;
        }

        // Create list of Switch Indicators(SWI)
        allSWIs = parentSwis.GetComponentsInChildren<SwIndicator>();

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

        SetGlobalDirect(true);

        createUiBtns();
    }

    private void createUiBtns()
    {
        if (uiPanel == null) return;

        float currentX = 0f;
        float currentY = 0f;

        foreach (SwIndicator swi in allSWIs)
        {
            GameObject go = Instantiate(btnPrefab, uiPanel);

            Button btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSwitchButtonClicked(swi));
            }

            UIIndicator ui = go.GetComponent<UIIndicator>();
            swi.uiIndicator = ui;
            ui.SetState(swi.IsDirect());

            TMP_Text label = go.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = swi.gameObject.name;
            }

            uiIndicators.Add(ui);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(currentX, currentY);

            currentX += xBtnOffset;
            if (currentX > 400)
            {
                currentX = 0f;
                currentY -= yBtnOffset;
            }
        }

        // Ack button for errors
        if (currentX > 1f)
        {
            currentX = 0f;
            currentY -= yBtnOffset;
        }
        {
            GameObject go = Instantiate(btnAckPrefab, uiPanel);

            Button btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnAckButtonClicked()); //TODO
                // btn.interactable = false;
            }

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(currentX, currentY);
        }
    }

    // Click on Ack button
    private void OnAckButtonClicked()
    {
        ResetError();
    }

    // Clic on Switch button
    private void OnSwitchButtonClicked(SwIndicator swi)
    {
        // Attention le clic est sur le Bouton, il faut retrouver le
        // Switch qui correspond
        foreach (SimpleSwitch ss in switches.Values)
        {
            if (ss.Swi == swi)
            {
                ss.InvertDirect();
                return;
            }
        }

    }

    public void SetGlobalDirect(bool direct)
    {
        if (switches == null)
        {
            Debug.LogError("Assign a switches in IntersectionMgr.");
            return;
        }

        foreach (SimpleSwitch ss in switches.Values)
        {
            ss.SetDirect(direct);
        }
    }

    private void CreateSwitches()
    {
        int nbSplines = splineContainer.Splines.Count;
        for (int sI = 0; sI < nbSplines; sI++)
        {
            Spline spline = splineContainer.Splines[sI];
            for (int kI = sI + 1; kI < spline.Count; kI++)
            {
                BezierKnot k = spline[kI];
                if (CreateKnotLink(sI, kI, out SimpleSwitch simpleSwitch))
                {
                    simpleSwitch.Normalize();
                    // Find matching Indicator (SWI)
                    // Debug.Log($"Add switch S{sI}K{kI} => S{simpleSwitch}");
                    switches[simpleSwitch.GetDirectKnot()] = simpleSwitch;
                    switches[simpleSwitch.GetDeviateKnot()] = simpleSwitch;
                    simpleSwitch.AssignSwi(splineContainer, allSWIs);
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

    public int nextKi(int sI, int kI, bool isFwd=true)
    {
        if (isFwd)
        {
            if (nbKnots == null || sI < 0 || sI >= nbKnots.Length) return -1;
            return (kI + 1) % nbKnots[sI];
        }
        else
        {
            if (nbKnots == null || sI < 0 || sI >= nbKnots.Length) return -1;
            return (kI + nbKnots[sI] - 1) % nbKnots[sI];
        }
    }

    public bool getKnotLink(int sI, int kI, out SimpleSwitch simpleSwitch)
    {

        if (kLinks != null &&
            switches.TryGetValue(new SplineKnotIndex(sI, kI), out simpleSwitch))
        {
            return true;
        }
        simpleSwitch = new SimpleSwitch();
        return false;
    }

    private bool CreateKnotLink(int sI, int kI, out SimpleSwitch simpleSwitch)
    {
        simpleSwitch = new SimpleSwitch();
        if (kLinks == null) return false;

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
        if (switches == null || !switches.TryGetValue(new SplineKnotIndex(sI, kI), out SimpleSwitch ss))
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

        return ss.SelectSpline(!ss.IsDirect(), isFwd);
    }

    /* Return the KnotIndex Ki so that t is on Spline[SplinIndex] between kI and kI + 1 
     * This function take into account the case of closed loop.
     * Return 6& if not found
     * - subPos returns a flost [0..1]: 0 => position is on kI, 1=> on kI+1
     */
    public int GetKnotIndex(int splineIndex, float t, out float subPos)
    {
        subPos = 0.5f;
        if (tValues == null || splineIndex < 0 || splineIndex >= tValues.Length)
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
            {
                subPos = (t-t0)/(t1-t0);
                return kI;
            }
        }

        // Si t est supérieur ou égal au dernier t, on retourne le dernier index
        if (t >= GetT(splineIndex, knotCount - 1))
            return knotCount - 1;

        // Aucun intervalle trouvé
        return -1;
    }

    // Indicate the next switch
    public SimpleSwitch ShowNextSwitch(int sI, int kI, bool isFwd)
    {
        if (switches == null)  return null;

        // limit to 10 next sections
        for (int i = 0; i < 10; i++)
        {
            if (getKnotLink(sI, kI, out SimpleSwitch ss))
            {
                return ss;
            }
            kI = nextKi(sI, kI, isFwd);
        }
        return null;
    }

    public void SetError(string message)
    {
        Debug.LogError(message);
        UiErrMsg.text = message;
        UiErrMsg.enabled = true;
    }

    public void ResetError()
    {
        UiErrMsg.text = "";
        UiErrMsg.enabled = false;
    }

    public string GetError()
    {
        return UiErrMsg.text;
    }
}
