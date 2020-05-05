using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

[SelectionBase]
public class PussController : MonoBehaviour
{
    #region members
    Rigidbody rb = default;
    private Transform inputSpace = default;

    [SerializeField, Range(0.0f, 0.3f)] float inputDeadzone = 0.15f;

    [SerializeField, Range(0.0f, 180.0f)] float turnMoveThreshold = 25.0f;
    [SerializeField, Range(0.0f, 360.0f)] float maxTurnRate = 150.0f;
    [SerializeField, Range(0.0f, 10.0f)] float minTurnRate = 5.0f;
    [SerializeField, Range(0.0f, 1.0f)] float turnRateEasingFactor = 0.6f;

    [SerializeField, Range(0.0f, 10.0f)] float posAccel = 1.2f;
    [SerializeField, Range(0.0f, 10.0f)] float negAccel = 2.5f;
    [SerializeField, Range(0.0f, 10.0f)] float turningAccel = 1.9f;
    [SerializeField, Range(0.0f, 10.0f)] float maxSpeed = 1.5f;
    [SerializeField, Range(0.0f, 10.0f)] float jumpHeight = 2.0f;

    [SerializeField, Range(0.0f, 90.0f)] float maxGroundAngle = 65.0f;

    Animator pussAnim;
    int animJump = Animator.StringToHash("Jump");
    int animSpeed = Animator.StringToHash("MoveSpeed");
    int animRotation = Animator.StringToHash("Rotation");
    int animPushHeight = Animator.StringToHash("PushHeight");
    int animPushing = Animator.StringToHash("Pushing");

    Vector3 velocity;
    Vector3 desiredVel = default;
    Quaternion desiredRot;

    Vector3 contactNormal;

    PussControls controls;
    Vector2 rawMoveInput;
    bool directionIsAnalogue;

    float minCosineToGround;
    bool performJump;
    bool onGround;
    bool performingPush;

    #endregion 
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.blue;
        Handles.ArrowHandleCap(1, transform.position, desiredRot, desiredVel.magnitude / maxSpeed, EventType.Repaint);

        Handles.color = Color.red;
        Handles.ArrowHandleCap(2, transform.position, transform.rotation, 1.1f, EventType.Repaint);

        Handles.color = Color.green;
        Quaternion groundContactAngle = Quaternion.FromToRotation(Vector3.forward, contactNormal);
        Handles.ArrowHandleCap(2, transform.position, groundContactAngle, 0.5f, EventType.Repaint);

        Gizmos.color = onGround ? Color.red : Color.blue;
        Gizmos.DrawSphere(transform.position, 0.08f);
    }

    private void OnValidate()
    {
        minCosineToGround = Mathf.Cos(maxGroundAngle);
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pussAnim = GetComponent<Animator>();

        inputSpace = Camera.main.transform;

        controls = new PussControls();
        controls.Gameplay.Movement.performed += Movement_performed;
        controls.Gameplay.JumpClimb.performed += JumpClimb_performed;

        onGround = true;
    }

    private void JumpClimb_performed(InputAction.CallbackContext obj)
    {
        performJump = onGround ? true : false;
    }

    private void Movement_performed(InputAction.CallbackContext obj)
    {
        bool inputWasAnalogue = obj.control.ToString().StartsWith("Stick");
        Vector2 newVector = obj.ReadValue<Vector2>();

        if(inputWasAnalogue)
        {
            if(directionIsAnalogue)
            {
                rawMoveInput = newVector;
            }
            else
            {
                // Should we switch to analogue mode?
                if (newVector.SqrMagnitude() > inputDeadzone * inputDeadzone)
                {
                    //Switch Input Modes
                    directionIsAnalogue = true;
                    rawMoveInput = newVector;
                }
            }
        }
        else
        {
            rawMoveInput = newVector;
            directionIsAnalogue = false;
        }
    }

    bool SnapToGround()
    {
        if (!Physics.Raycast(transform.position, Vector3.down,
            out RaycastHit hit, 1.0f))//, mask))
        {
            return false;
        }
        else
        {
            contactNormal = hit.normal;
            return true;
        }
    }

    public void YouArePushingMe(PushableObject pushableObject)
    {
        pussAnim.SetBool(animPushing, true);
        performingPush = true;
        int pushHeight = 2;

        if (pushableObject is Snowball)
        {
            Snowball snowball = pushableObject as Snowball;
            float snowballSize = snowball.GetSizeInMeters();
            if (snowballSize <= 1.4f)
                pushHeight = 1;
            else if (snowballSize <= 1.8f)
                pushHeight = 2;
            else
                pushHeight = 3;
        }

        pussAnim.SetInteger(animPushHeight, pushHeight);
    }

    public void YouStoppedPushing()
    {
        performingPush = false;
        pussAnim.SetBool(animPushing, false);
        onGround = true;
        SnapToGround();
    }

    private void FixedUpdate()
    {
        if (performingPush) return;

        velocity = rb.velocity;
        if (rawMoveInput.magnitude >= inputDeadzone)
        {
            Vector3 moveInputinCameraSpace = inputSpace.TransformDirection(rawMoveInput.x, 0.0f, rawMoveInput.y); ;
            Vector3 moveInputOnContactSurface = Vector3.ProjectOnPlane(moveInputinCameraSpace, contactNormal);
            if (!directionIsAnalogue)
            {
                Vector3 moveInputCardinalCamera = Vector3.zero;
                // atan2 ranges from -pi to +pi, we care about +-pi/4, +-2pi/4, +- 3pi/4, pi and 0.
                // divide by 4pi and we get [-4,4)
                float atan2 = Mathf.Atan2(moveInputOnContactSurface.x, moveInputOnContactSurface.z);
                int direction = Mathf.RoundToInt(atan2 / Mathf.PI * 4);

                float sqrt2 = Mathf.Sqrt(2.0f);
                switch(direction)
                {
                    case -3:
                        moveInputCardinalCamera = new Vector3(-sqrt2, 0.0f, -sqrt2);
                        break;
                    case -2:
                        moveInputCardinalCamera = new Vector3(-1.0f, 0.0f, 0.0f);
                        break;
                    case -1:
                        moveInputCardinalCamera = new Vector3(-sqrt2, 0.0f, sqrt2);
                        break;
                    case 0:
                        moveInputCardinalCamera = new Vector3(0.0f, 0.0f, 1.0f);
                        break;
                    case 1:
                        moveInputCardinalCamera = new Vector3(sqrt2, 0.0f, sqrt2);
                        break;
                    case 2:
                        moveInputCardinalCamera = new Vector3(1.0f, 0.0f, 0.0f);
                        break;
                    case 3:
                        moveInputCardinalCamera = new Vector3(sqrt2, 0.0f, -sqrt2);
                        break;
                    case 4:
                    case -4:
                        moveInputCardinalCamera = new Vector3(0.0f, 0.0f, -1.0f);
                        break;
                    default:
                        Debug.LogError("This can't happen, bro.");
                        break;
                }
                
                moveInputOnContactSurface = Vector3.ProjectOnPlane(moveInputCardinalCamera, contactNormal);
            }

            desiredVel = moveInputOnContactSurface.normalized * rawMoveInput.magnitude * maxSpeed;

            desiredRot = Quaternion.LookRotation(moveInputOnContactSurface, contactNormal);

        }
        else
        {
            desiredVel = Vector3.zero;

            Vector3 projectOntoPlane = contactNormal * Vector3.Dot(transform.forward, contactNormal);
            desiredRot = Quaternion.LookRotation(transform.forward - projectOntoPlane, contactNormal);
        }
        

        bool weAreMovingForward;
        if (velocity.sqrMagnitude < 0.1)
            weAreMovingForward = true;
        else
            weAreMovingForward = Vector3.Dot(velocity, desiredVel) > 0.0f;

        float turnRate = maxTurnRate;
        float curAccel;

        float turnDelta = Quaternion.Angle(desiredRot, transform.rotation);
        if (turnDelta < turnMoveThreshold)
        {
            turnRate = Mathf.Max(maxTurnRate * turnRateEasingFactor, minTurnRate);
            curAccel = weAreMovingForward ? posAccel : negAccel;
        }
        else
        {
            desiredVel = Vector3.zero;
            curAccel = turningAccel;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, turnRate * Time.deltaTime);

        float speedDeltaForFrame = curAccel * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVel.x, speedDeltaForFrame);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVel.z, speedDeltaForFrame);
        
        if(performJump)
        {
            performJump = false;
            velocity.y += Mathf.Sqrt(-2.0f * Physics.gravity.y * jumpHeight);
            pussAnim.SetTrigger(animJump);
        }

        //@TODO: JANKY way to stop Puss from sliding. Do something WAY BETTER.
        rb.useGravity = !onGround;
        rb.velocity = velocity;

        float animTurnrate = turnDelta / maxTurnRate;
        pussAnim.SetLayerWeight(1, 0.5f * animTurnrate);
        pussAnim.SetFloat(animRotation, turnDelta/maxTurnRate);
        pussAnim.SetFloat(animSpeed, new Vector2(velocity.x, velocity.z).magnitude);
    }

    private void OnCollisionStay(Collision collision)
    {
        ProcessCollisions(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.contactCount == 0)
            onGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        ProcessCollisions(collision);
        if(onGround)
        {
            pussAnim.ResetTrigger(animJump);
        }
    }
    
    private void ProcessCollisions(Collision collision)
    {
        contactNormal = Vector3.zero;
        foreach (var contact in collision.contacts)
        {
            float cos = Vector3.Dot(contact.normal, Vector3.up);
            if (cos > minCosineToGround)
            contactNormal += contact.normal;
        }

        if (contactNormal == Vector3.zero)
        {
            //We're in the air
            contactNormal = Vector3.up;
        }
        else
        {
            contactNormal.Normalize();
            onGround = true;
        }

    }

    private void OnEnable()
    {
        if (controls != null)
            controls.Enable();
    }
    private void OnDisable()
    {
        if(controls != null)
            controls.Disable();
    }
}
