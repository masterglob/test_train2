using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;

using TMPro;


public class LocomotiveMover : MonoBehaviour
{
    public SplineContainer rail; 

    private Spline currentSpline;
    public int splineId = 0;

    private SimpleSwitch currentSwitch = null;
    private SimpleSwitch previousSwitch = null;

    [SerializeField] private float speed = 0.0f;
    public TMP_Text textSpeed;
    public TMP_Text textCurrSpline = null;
    public TMP_Text textDebug = null;
    public float maxSpeed = 50f;      // vitesse max (m/s)
    public float drag = 0.5f;         // friction naturelle (m/s²)

    [Header("Acceleration")]
    public float maxAccel = 5f;       // accélération max (m/s²)

    public IntersectionsMgr interMgr = null;
    private bool isBwd = false;


    void Start()
    {

        currentSpline = null;
        if (rail != null && rail.Splines != null && rail.Splines.Count > 0)
        {
            if (splineId >= rail.Splines.Count)
            {
                Debug.LogError($"Invalid spawn Spline Id:{splineId}");
                splineId = 0;
            }
            currentSpline = rail.Splines[splineId];
        }
    }

    private void setSplineId(int id)
    {
        if (rail != null && rail.Splines != null && rail.Splines.Count > id)
        {
            splineId = id;
            currentSpline = rail.Splines[splineId];
        }
        else
        {
            Debug.LogWarning($"Invalid Spline Id:{id}");
        }
    }

    private void test(float t)
    {
        int kI = interMgr.GetKnotIndex(splineId, t, out float subPos);
        string dbgMSg = $" Position in S{splineId}/K{kI}";

        if (interMgr.getKnotLink(splineId, kI, out SimpleSwitch ss))
        {
            currentSwitch = ss;
            dbgMSg += "\n";
            dbgMSg += $" Comm S{ss.Spline1Id}/K{ss.Spline1Knot1} <=> S{ss.Spline2Id}/K{ss.Spline2Knot1}";
            dbgMSg += $"\n subPos={subPos * 100:00}%";
        }
        else
        {
            currentSwitch = null;
            // Search for next Switch (only when moving forward)
            if (speed > -0.01f)
            {
                SimpleSwitch ss2 = interMgr.ShowNextSwitch(splineId, kI, !isBwd);
                if (ss2 != null)
                {
                    dbgMSg += $"\nNext switch:{ss2.ToShortString()} ";
                }
            }
        }

        if (textDebug != null)
            textDebug.text = dbgMSg;

        if (previousSwitch == null && currentSwitch != null)
        {
            // Check consistency (No switch taken in bad direction)
            int kJ = kI;
            if (subPos > 0.5)
            {
                kJ = interMgr.nextKi(splineId, kI);
            }

            // Check if the "side" we entered the switch contains a dead end
            // (if so, there is no possible issue)
            if (ss.CanEnterSwitchAt(splineId, kJ))
            {
                // Debug.Log($"Switch Can use Spline{splineId},isPt1First={isPt1First} on {ss}");
            }
            else
            {
                interMgr.SetError($"Invalid switch cross: {ss}");
            }
        }
        previousSwitch = currentSwitch;
    }

    private void CheckSplineChange(float t, float fwdSpeed)
    {
        int kI = interMgr.GetKnotIndex(splineId, t, out float subPos);

        if (Math.Abs(speed) < 0.001) return;

        setSplineId(interMgr.GetNewSplineId(splineId, kI, fwdSpeed >= 0));

    }

    // Return true if the train "forward" is reverse from the Pline direction
    private bool IsSplneReverse(Vector3 forward)
    {
        float dotProduct = Vector3.Dot(transform.forward, forward);

        return dotProduct < 0f;
    }

    private void FixedUpdate()
    {
        if (currentSpline == null)
        {
            Debug.LogError("Assign a currentSpline in Locomotive Mover.");
            return;
        }
        if (interMgr == null)
        {
            Debug.LogError("Assign a interMgr in Locomotive Mover.");
            return;
        }

        Vector3 previousPosition = transform.position;
        updateParams();

        var native = new NativeSpline(currentSpline);
        float distance = SplineUtility.GetNearestPoint(native, transform.position, out float3 nearest, out float t);

        test(t);

        transform.position = nearest;

        Vector3 forward = Vector3.Normalize(native.EvaluateTangent(t));
        Vector3 up = native.EvaluateUpVector(t);

        var remappedForward = new Vector3(0, 0, 1);
        var remappedUp = new Vector3(0, 1, 0);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

        isBwd = IsSplneReverse(forward);
        if (isBwd)
        {
            // Si la direction est inversée, applique une rotation de 180 degrés autour de l'axe Y (ou un autre axe si nécessaire)
            axisRemapRotation *= Quaternion.Euler(0, 180f, 0);  // Inverser la direction sur l'axe Y
        }

        transform.rotation = Quaternion.LookRotation(forward, up) * axisRemapRotation;

        Vector3 velocity = (transform.position - previousPosition) / Time.deltaTime;
        float dot = Vector3.Dot(forward, velocity.normalized);
        CheckSplineChange(t, dot);

    }

    private void updateParams()
    {
        // Apply current speed
        Vector3 engineForward = transform.forward;
        float move = speed * Time.fixedDeltaTime;
        transform.Translate(Vector3.forward * move);

        speed = Mathf.Clamp(speed, -maxSpeed, maxSpeed);
        // Appliquer friction
        if (speed > 0f)
        {
            speed -= drag * Time.fixedDeltaTime;
            if (speed < 0f) speed = 0f;
        }
        else if (speed < 0f)
        {
            speed += drag * Time.fixedDeltaTime;
            if (speed > 0f) speed = 0f;
        }
        if (textSpeed != null)
            textSpeed.text = $"Speed: {speed:F2} m/s";
        if (textCurrSpline != null)
            textCurrSpline.text = $"Track: {splineId}";
    }

    public void Throttle(float power)
    {
        speed += power * maxAccel * Time.fixedDeltaTime;
    }

    public void Brake()
    {
        speed *= 0.9f;
    }
}
