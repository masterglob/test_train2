using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using System;

using TMPro;


public class LocomotiveMover : MonoBehaviour
{
    public SplineContainer rail; 

    private Spline currentSpline;
    private int splineId = 0;

    [SerializeField] private float power = 1.0f;
    [SerializeField] private float speed = 0.0f;
    public TMP_Text textSpeed;
    public TMP_Text textCurrSpline = null;
    public TMP_Text textDebug = null;
    public Slider slider_0_1 = null;
    public float maxSpeed = 50f;      // vitesse max (m/s)
    public float drag = 0.5f;         // friction naturelle (m/s²)

    [Header("Acceleration")]
    public float maxAccel = 5f;       // accélération max (m/s²)

    private IntersectionsMgr interMgr = null;

    void Start()
    {

        currentSpline = null;
        if (rail != null && rail.Splines != null && rail.Splines.Count > 0)
        {
            splineId = 0;
            currentSpline = rail.Splines[splineId];
            interMgr = new IntersectionsMgr(rail, slider_0_1);
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
        int kI = interMgr.GetKnotIndex(splineId, t);
        textDebug.text = $" Position in S{splineId}/K{kI}";

        if (interMgr.getKnotLink(splineId, kI, out SimpleSwitch ss))
        {
            textDebug.text += "\n";
            textDebug.text += $" Comm S{ss.Spline1Id}/K{ss.Spline1Knot1} <=> S{ss.Spline2Id}/K{ss.Spline2Knot1}";
        }
    }

    private void CheckSplineChange(float t)
    {
        int kI = interMgr.GetKnotIndex(splineId, t);
        setSplineId(interMgr.GetNewSplineId(splineId, kI));

    }

    private void FixedUpdate()
    {
        if (currentSpline == null) return;

        updateParams();

        var native = new NativeSpline(currentSpline);
        float distance = SplineUtility.GetNearestPoint(native, transform.position, out float3 nearest, out float t);

        test(t);
        CheckSplineChange(t);

        transform.position = nearest;

        Vector3 forward = Vector3.Normalize(native.EvaluateTangent(t));
        Vector3 up = native.EvaluateUpVector(t);

        var remappedForward = new Vector3(0, 0, 1);
        var remappedUp = new Vector3(0, 1, 0);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

        transform.rotation = Quaternion.LookRotation(forward, up) * axisRemapRotation;

        var keyboard = Keyboard.current;
        if (keyboard.wKey.isPressed)
        {
            Throttle(power);
        }

        if (keyboard.sKey.isPressed)
        {
            Throttle(-power);
        }
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
        textSpeed.text = $"Speed: {speed:F2} m/s";
        textCurrSpline.text = $"Track: {splineId}";
    }

    private void Throttle(float power)
    {
        speed += power * maxAccel * Time.fixedDeltaTime;
    }
}
