
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class SplineTerrainProjector : MonoBehaviour
{
    public SplineContainer splineContainer;
    public Terrain terrain;

    [Header("Projection Settings")]
    public float rayStartHeight = 100f;
    public LayerMask terrainMask;

    public void ProjectAllSplines()
    {
        if (splineContainer == null || terrain == null)
        {
            Debug.LogError("Missing SplineContainer or Terrain reference.");
            return;
        }

        int splineCount = splineContainer.Splines.Count;

        for (int i = 0; i < splineCount; i++)
        {
            Spline spline = splineContainer.Splines[i];
            ProjectSplineOnTerrain(spline);
        }

        Debug.Log("Projection complete.");
    }

    private void ProjectSplineOnTerrain(Spline spline)
    {
        int knotCount = spline.Count;

        for (int i = 0; i < knotCount; i++)
        {
            BezierKnot knot = spline[i];
            Vector3 pos = knot.Position;

            // point de départ du raycast
            Vector3 rayStart = new Vector3(pos.x, pos.y + rayStartHeight, pos.z);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainMask))
            {
                Vector3 newPosition = hit.point;

                // Mettre à jour le knot
                knot.Position = newPosition;
                spline.SetKnot(i, knot);
            }
        }
    }
}
