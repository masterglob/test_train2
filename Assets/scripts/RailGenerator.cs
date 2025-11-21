using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[ExecuteAlways]
public class RailGenerator : MonoBehaviour
{
    public SplineContainer sourceSpline;
    public SplineContainer leftRail;
    public SplineContainer rightRail;
    public float offset = 0.7f;
    [Range(10, 200)]
    public int samplesPerSpline = 100; // Nombre d'échantillons par spline


    // Appelé depuis l'éditeur
    public void Generate()
    {
        if (sourceSpline == null || leftRail == null || rightRail == null)
        {
            Debug.LogWarning("Assign all spline containers.");
            return;
        }

        // Copie la position et rotation de la source vers les cibles
        leftRail.transform.position = sourceSpline.transform.position;
        leftRail.transform.rotation = sourceSpline.transform.rotation;
        leftRail.transform.localScale = sourceSpline.transform.localScale;

        rightRail.transform.position = sourceSpline.transform.position;
        rightRail.transform.rotation = sourceSpline.transform.rotation;
        rightRail.transform.localScale = sourceSpline.transform.localScale;

        // Efface les splines existantes dans les rails
        while (leftRail.Splines.Count > 0)
        {
            leftRail.RemoveSplineAt(0);
        }
        while (rightRail.Splines.Count > 0)
        {
            rightRail.RemoveSplineAt(0);
        }

        // Génère un rail pour chaque spline source
        for (int i = 0; i < sourceSpline.Splines.Count; i++)
        {
            GenerateOffsetRail(sourceSpline.Splines[i], leftRail, -offset);
            GenerateOffsetRail(sourceSpline.Splines[i], rightRail, offset);
        }

        // Force la mise à jour des SplineExtrude
        RefreshSplineExtrude(leftRail);
        RefreshSplineExtrude(rightRail);

        Debug.Log($"Rails generated! {sourceSpline.Splines.Count} splines processed.");
    }

    void GenerateOffsetRail(Spline sourceSplineData, SplineContainer target, float offsetValue)
    {
        if (sourceSplineData == null || sourceSplineData.Count == 0)
        {
            Debug.LogWarning("Source spline is empty or null.");
            return;
        }

        // Crée une nouvelle spline
        Spline targetSplineData = new Spline();

        // Échantillonne la spline source à intervalles réguliers
        for (int i = 0; i <= samplesPerSpline; i++)
        {
            float t = (float)i / samplesPerSpline;

            // Évalue la position et la tangente à ce point
            float3 position = sourceSplineData.EvaluatePosition(t);
            float3 tangent = sourceSplineData.EvaluateTangent(t);
            tangent = math.normalize(tangent);

            // Calcule le vecteur perpendiculaire (vers la droite)
            float3 up = new float3(0, 1, 0);
            float3 right = math.normalize(math.cross(tangent, up));

            // Applique l'offset perpendiculaire
            float3 offsetPosition = position + (right * offsetValue);

            // Crée un knot linéaire (tangentes auto)
            BezierKnot newKnot = new BezierKnot(offsetPosition);
            targetSplineData.Add(newKnot);
        }

        // Applique l'auto-smoothing pour des tangentes naturelles
        for (int i = 0; i < targetSplineData.Count; i++)
        {
            targetSplineData.SetTangentMode(i, TangentMode.AutoSmooth);
        }

        // Copie le paramètre closed
        targetSplineData.Closed = sourceSplineData.Closed;

        // Ajoute la nouvelle spline au container cible
        target.AddSpline(targetSplineData);
    }

    void RefreshSplineExtrude(SplineContainer container)
    {
        // Récupère le composant SplineExtrude
        var splineExtrude = container.GetComponent<SplineExtrude>();
        if (splineExtrude != null)
        {
            // Force la régénération en désactivant/réactivant
            splineExtrude.enabled = false;
            splineExtrude.enabled = true;

            // Ou utilise Rebuild si disponible
            splineExtrude.Rebuild();
        }
    }
}