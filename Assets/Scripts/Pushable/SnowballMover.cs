
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[SelectionBase]
class SnowballMover : PushableMovement
{
    [SerializeField] GameObject ourBall = default;
    [SerializeField, Range(0.0f, 1.0f)] float smoothIn = 0.0f;
    [SerializeField, Range(0.0f, 1.0f)] float smoothOut = 1.0f;

    Snowball ourSnowball;
    Quaternion startRotForBall, targetRotForBall;

    private int oldSize = 0;
    
    private void Awake()
    {
        ourSnowball = GetComponent<Snowball>();
    }

    public override void InitiateMoveTo(Vector3 targetPosition, Quaternion? _targetRot = null)
    {
        Debug.Assert(ourBall != null, "RollingBall is missing a reference to it's ball!", this);
        base.InitiateMoveTo(targetPosition, _targetRot);
                               
        Vector3 movementVector = (startPosition - targetPosition);
        Vector3 rotationAxis = Vector3.Cross(movementVector, gameObject.transform.up);

        startRotForBall = ourBall.transform.rotation;
        targetRotForBall = Quaternion.AngleAxis(movementVector.magnitude * Mathf.Rad2Deg, rotationAxis) * startRotForBall;
        
    }

    public override bool WeAreStillMoving()
    {
        if (base.WeAreStillMoving())
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, BezierRemappedT());

            if(ourSnowball.size != oldSize)
                transform.localScale = ScaleWithT();

            ourBall.transform.rotation = Quaternion.Lerp(startRotForBall, targetRotForBall, BezierRemappedT());
            return true;
        }
        else
        {
            //cleanup
             oldSize = ourSnowball.size;
            return false;
        }
    }
    
    private Vector3 ScaleWithT()
    {
        if (ourSnowball.size < ourSnowball.SnowballScales.Length -1)
        {
            float minScale = ourSnowball.SnowballScales[ourSnowball.size];
            float maxScale = ourSnowball.SnowballScales[ourSnowball.size + 1];

            float currentScale = Mathf.Lerp(minScale, maxScale, t) * ourSnowball.baseScale;

            return Vector3.one * currentScale;
        }
        else
        {
            return Vector3.one * ourSnowball.SnowballScales[ourSnowball.SnowballScales.Length - 1] * ourSnowball.baseScale;
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
        return Mathf.Lerp(abc,bcd,t);
    }
}
