using UnityEngine;


[RequireComponent(typeof(PushableObject))]
abstract public class PushableMovement : MonoBehaviour
{
    protected Vector3 startPosition, targetPosition = default;
    protected Quaternion startRot, targetRot;
    [SerializeField, Range(0.1f, 3.0f)] protected float timeToMove = 1.5f;

    protected float t = default;

    public virtual void InitiateMoveTo(Vector3 targetPosition,
        Quaternion? _targetRot = null)
    {
        Quaternion targetRot = Quaternion.identity;
        if (_targetRot != null) targetRot = _targetRot.Value;

        startPosition = transform.position;
        this.targetPosition = targetPosition;

        startRot = transform.rotation;
        this.targetRot = targetRot;
        t = 0;
    }

    public virtual bool WeAreStillMoving()
    {
        IncrementTime();
        if (t >= 1.0f)
        {
            transform.position = targetPosition;
            transform.rotation = targetRot;
            return false;
        }
        else
        {
            return true;
        }
    }

    private void IncrementTime()
    {
        t += (Time.deltaTime / timeToMove);
    }
}
