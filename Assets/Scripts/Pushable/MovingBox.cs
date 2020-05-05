
using UnityEngine;

//Collider required for the lovely rotation at the end of the animation.


[RequireComponent(typeof(PushableObject))]
class MovingBox : PushableMovement
{
    [SerializeField, Range(0.0f, 1.3f)] float smoothIn  = 0.7f;
    [SerializeField, Range(0.0f, 1.3f)] float smoothOut = 1.05f;

    [SerializeField, Range(0.0f, 20.0f)] float bounceRotation = 5.0f;
    [SerializeField, Range(40.0f, 80.0f)] float bounceSpeed = 55.0f;
    [SerializeField, Range(0.0f, 1.0f)] float bounceTiming = 0.8f;
    [SerializeField, Range(0.0f, 1.0f)] float bounceHeight = 0.1f;
    
    private Vector3 bounceMaxHeight;
    private Quaternion extremeRot;

    public override void InitiateMoveTo(Vector3 targetPosition, Quaternion? _targetRot = null)
    {
        base.InitiateMoveTo(targetPosition, _targetRot);

        Vector3 rotationAxis = Vector3.Cross(
            startPosition - targetPosition,
            gameObject.transform.up);

        startRot = transform.rotation;
        extremeRot = Quaternion.AngleAxis(bounceRotation, rotationAxis);
        bounceMaxHeight = Vector3.up * bounceHeight;
    }

    public override bool WeAreStillMoving()
    {
        if (base.WeAreStillMoving())
        {
            float bounceT = 1.0f - bounceSpeed *
                (t - bounceTiming) * (t - bounceTiming);
            transform.rotation = Quaternion.Lerp(startRot, extremeRot, bounceT);

            transform.position = 
                Vector3.Lerp(startPosition, targetPosition, BezierRemappedT()) +
                Vector3.Lerp(Vector3.zero, bounceMaxHeight, bounceT);

            return true;
        }
        else
        {
            //We stopped moving this frame. Any cleanup to do?
            return false;
        }
    }
    float BezierRemappedT()
    {
        float invSmoothIn = 1.0f - smoothIn;
        float ab = Mathf.Lerp(0, invSmoothIn, t);
        float bc = Mathf.Lerp(invSmoothIn, smoothOut, t);
        float cd = Mathf.Lerp(smoothOut, 1, t);

        float abc = Mathf.Lerp(ab, bc, t);
        float bcd = Mathf.Lerp(bc, cd, t);
        return Mathf.Lerp(abc, bcd, t);
    }
}
