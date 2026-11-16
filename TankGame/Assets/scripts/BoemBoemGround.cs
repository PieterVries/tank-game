using UnityEngine;
using UnityEngine.U2D;

public class BoemBoemGround : MonoBehaviour
{
    public SpriteShapeController ground;

    public float craterRadius = 1.5f;
    public float craterDepth = 1.0f;

    [Header("Crater Shape")]
    public int craterPoints = 5;

    private Spline spline;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (ground == null)
        {
            ground = GetComponent<SpriteShapeController>();
        }

        if (ground != null)
        {
            spline = ground.spline;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void MakeCrater(Vector2 explosionPosition)
    {
        if (ground == null)
        {
            ground = GetComponent<SpriteShapeController>();
        }

        if (ground == null)
        {
            return;
        }

        if (spline == null)
        {
            spline = ground.spline;
        }

        Vector3 localExplosion = ground.transform.InverseTransformPoint(explosionPosition);
        int pointCount = spline.GetPointCount();
        bool[] affectedPoints = new bool[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 point = spline.GetPosition(i);
            float horizontalDistance = Mathf.Abs(point.x - localExplosion.x);

            if (horizontalDistance > craterRadius)
            {
                continue;
            }

            // Ignore the lower edge of the closed ground shape.
            if (point.y < localExplosion.y - craterDepth)
            {
                continue;
            }

            float normalizedDistance = horizontalDistance / craterRadius;
            float amount = 1f - Mathf.SmoothStep(0f, 1f, normalizedDistance);
            point.y -= craterDepth * amount;
            spline.SetPosition(i, point);
            affectedPoints[i] = true;
        }

        for (int i = 0; i < pointCount; i++)
        {
            bool previousPointAffected = i > 0 && affectedPoints[i - 1];
            bool nextPointAffected = i < pointCount - 1 && affectedPoints[i + 1];

            if (affectedPoints[i] || previousPointAffected || nextPointAffected)
            {
                spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            }
        }

        ground.RefreshSpriteShape();
    }

}
