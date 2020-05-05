using UnityEngine;
using UnityEngine.SceneManagement;

public class PushableObject : MonoBehaviour
{
    [SerializeField] LevelGrid levelGrid = default;
    [SerializeField] LayerMask pushableLayer = default;
    [SerializeField, Range(0.0f, 10.0f)] float maxGridSnapDistance = 1.0f;
    [SerializeField, Range(0.0f, 10.0f)] float collisionTimeBeforePush = 1.0f;
    [SerializeField, Range(1.1f, 3.0f)] float repeatCollisionLeeway = 1.3f;


    public static int PushableLayerIndex
    {
        get
        {
            if(pushableLayerIndex == int.MinValue)
            {
                Debug.LogError("Attempt to use PushableLayerIndex before it's set!");
            }
            return pushableLayerIndex;
        }
        set { pushableLayerIndex = value; }
    }
    private static int pushableLayerIndex = int.MinValue;
    
    protected GridNode currentGridNode;
    protected PushableMovement mover;
    protected GameObject pusher;
    protected Vector3 deltaToPusher;


    float timeSpentColliding = 0.0f;
    private bool weAreMoving;

    private void OnValidate()
    {
        if(pushableLayerIndex == int.MinValue)
        {
            pushableLayerIndex = Utils.GetLayerNumberFromMask(pushableLayer);
        }
    }

    protected virtual void Awake()
    {
        weAreMoving = false;
        mover = GetComponent<PushableMovement>();
    }

    private void Start()
    {
        SceneManager.MoveGameObjectToScene(gameObject,
    SceneManager.GetSceneByName("Grid Scene"));

        SnapToGrid();
    }

    private void Update()
    {
        if (weAreMoving)
        {
            weAreMoving = mover.WeAreStillMoving();
            if (weAreMoving) MovePusherWithUs();
            else StopBeingPushed();
        }
        else
        {
            #region Debug_KeyboardMovement
#if false
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                TryInitiateMoveTo(currentGridNode.LeftNeighbour);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                TryInitiateMoveTo(currentGridNode.RightNeighbour);
            if (Input.GetKeyDown(KeyCode.UpArrow))
                TryInitiateMoveTo(currentGridNode.FrontNeighbour);
            if (Input.GetKeyDown(KeyCode.DownArrow))
                TryInitiateMoveTo(currentGridNode.BackNeighbour);
#endif
#endregion
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            ProcessCollision(collision);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            timeSpentColliding = 0.0f;
            ProcessCollision(collision);
        }
    }

    private void StopBeingPushed()
    {
        timeSpentColliding = 0.0f;

        pusher.transform.position = transform.position - (deltaToPusher * repeatCollisionLeeway);
        pusher.gameObject.GetComponent<PussController>().YouStoppedPushing();
    }

    protected virtual void MovePusherWithUs()
    {
        if (pusher == null) return;

        //@TODO: Janky hack, m8. Do something more sensible/robust.
        Vector3 newPos  = (transform.position - deltaToPusher);
        newPos.y = currentGridNode.transform.position.y;
        pusher.transform.position = newPos;
    }

    private void ProcessCollision(Collision collision)
    {
        if (weAreMoving) return;
        else
        { 
            timeSpentColliding += Time.deltaTime;
            if (timeSpentColliding > collisionTimeBeforePush)
            {
                Vector3 relativePosition = transform.position - collision.gameObject.transform.position;

                float frontBack = Vector3.Dot(relativePosition, Vector3.forward);
                float leftRight = Vector3.Dot(relativePosition, Vector3.left);

                GridNode nodeAtTargetDir;
                if (Mathf.Abs(frontBack) > Mathf.Abs(leftRight))
                    nodeAtTargetDir = (frontBack > 0.0f) ? currentGridNode.FrontNeighbour : currentGridNode.BackNeighbour;
                else
                    nodeAtTargetDir = (leftRight > 0.0f) ? currentGridNode.LeftNeighbour : currentGridNode.RightNeighbour;

                if (TryInitiateMoveTo(nodeAtTargetDir))
                {
                    pusher = collision.rigidbody.gameObject;
                    deltaToPusher = (transform.position - collision.transform.position) * repeatCollisionLeeway;

                    collision.gameObject.GetComponent<PussController>().YouArePushingMe(this);
                }
            }
        }
    }

    protected virtual bool TryInitiateMoveTo(GridNode targetGridNode)
    {
        if (targetGridNode != null && targetGridNode.occupant == null && !targetGridNode.isOutOfBounds)
        {
            if (mover == null) Debug.LogError("Pushable Object has no mover script set!");
            currentGridNode.occupant = null;
            targetGridNode.occupant = this;

            currentGridNode = targetGridNode;

            mover.InitiateMoveTo(targetGridNode.gameObject.transform.position);
            weAreMoving = true;
            return true;
        }
        else
        {
            // Block can't be pushed in that direction.
            return false;
        }
    }

    protected virtual bool SnapToGrid()
    {
        Collider[] hits = Physics.OverlapSphere(
            position: transform.position,
            radius: maxGridSnapDistance,
            layerMask: levelGrid.gridLayer,
            queryTriggerInteraction: QueryTriggerInteraction.Collide);

        int hitCount = 0;
        float closestHit = float.MaxValue;
        Collider bestHit = null;
        foreach (Collider hit in hits)
        {
            hitCount++;
            float sqrDist =
                (transform.position - hit.gameObject.transform.position).sqrMagnitude;
            if (sqrDist < closestHit)
            {
                closestHit = sqrDist;
                bestHit = hit;
            }
        }

        if(bestHit != null)
        {
            currentGridNode = bestHit.transform.GetComponent<GridNode>();
            currentGridNode.occupant = this;
            /*Vector3 targetPosition = currentGridNode.hasSnow ?
                bestHit.transform.position + LevelGrid.Instance.snowDepth * Vector3.up :
                bestHit.transform.position;*/
            
            transform.position = bestHit.transform.position;
            return true;
        }
        else
        {
            Debug.Log("We can't snap, we're too far off of the grid! Hitcount: " + hitCount + ", SqrDistance: " + closestHit, this);
            return false;
        }
    }
}

