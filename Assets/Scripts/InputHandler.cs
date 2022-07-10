using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler instance;

    public StateManager state;
    public CameraManager cam;
    public Rigidbody rig;
    public ExpandedStats stat;
    public float vertical;
    public float horizontal;
    public float moveamount;
    public Vector3 movedir;
    [HideInInspector] public Vector3 tarVel;
    [HideInInspector]public InputActionPhase firePhase;

    public Inputs inputs;
    public void Awake()
    {
        inputs = new Inputs();
        inputs.Player.Enable();
        inputs.Player.Fire.performed += Fire;

        stat = transform.GetComponentInChildren<ExpandedStats>();
        state = transform.GetComponentInChildren<StateManager>();
        rig = transform.GetComponentInChildren<Rigidbody>();
        cam = transform.GetComponentInChildren<CameraManager>();

        instance = this;
    }
    private void Update()
    {
        Move(inputs.Player.Move.ReadValue<Vector2>());
        cam.Look(inputs.Player.Look.ReadValue<Vector2>());
    }
    #region Input Methods

    void Fire(InputAction.CallbackContext context)
    {
        state.anim.CrossFade("Throw",0.1f);
    }
    void Move(Vector2 pos) 
    {
        vertical = Mathf.MoveTowards(vertical, pos.y, Time.deltaTime * 4.5f);
        horizontal = Mathf.MoveTowards(horizontal, pos.x, Time.deltaTime * 4.5f);
    }
    #endregion


    public void FixedUpdate()
    {
        GetMove();
    }
    [HideInInspector] public bool movingToWall;

    // Rigidbody part of movement, animation part is handled in StateManager
    // TO DO: CLEAN
    void GetMove()
    {
        if (stat.death)
            return;

        Vector3 camforward = cam.pivot.transform.forward;
        camforward.y = 0;
        Vector3 v = vertical * camforward;
        Vector3 h = horizontal * cam.pivot.transform.right;
        movedir = (v + h).normalized;
        if (MovingToWall())
        {
            movingToWall = true;
            vertical = Mathf.MoveTowards(vertical, 0, Time.deltaTime * 10f);
            horizontal = Mathf.MoveTowards(horizontal, 0, Time.deltaTime * 10f);
        }
        else
            movingToWall = false;
        float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
        moveamount =  Mathf.Clamp01(m);
        if (moveamount > 0)
            rig.drag = 0;
        else
            rig.drag = 4;

       #region TEST
       
        Vector3 targetdir = movedir;
        targetdir.y = 0;

        Vector3 curVel = rig.velocity;
        tarVel = moveamount * state.currentSpeed * targetdir;


        // Handling with stairs
        float frontRayOffset = .4f;
        float frontY = 0;
        Vector3 origin = state.transform.position + (targetdir * frontRayOffset);
        origin.y += .5f;
        Debug.DrawRay(origin, -Vector3.up, Color.red, .01f, false);
        if (Physics.Raycast(origin, -Vector3.up, out RaycastHit hit, 2f, ~LayerMask.GetMask("Door", "MaınCapsule", "Enemy", "Breakable", "Player")))
        {
            float y = hit.point.y;
            frontY = y - state.transform.position.y;
        }
        //

        if(moveamount > 0.1f)
        {
            rig.isKinematic = false;
            rig.drag = 0;
            if(Mathf.Abs(frontY) > 0.02f)
            {
                if(frontY > 0)
                    tarVel.y = ((frontY>0) ? frontY + 0.2f : frontY) * state.currentSpeed + ( frontY * 10);
                else
                    tarVel.y = ((frontY > 0) ? frontY + 0.2f : frontY) * state.currentSpeed;
            }
        }
        else
        {
            float abs = Mathf.Abs(frontY);
            if (abs > 0.02f)
            {
                rig.isKinematic = false;
                tarVel.y = 0;
                rig.drag = 100;
            }
            rig.isKinematic = true;
        }

        rig.velocity = Vector3.Lerp(curVel, tarVel, Time.deltaTime * 10);

        #endregion

    }

    /// <summary>
    /// if player moving to wall true else false
    /// </summary>
    public bool MovingToWall()
    {
        Vector3 camforward = cam.pivot.transform.forward;
        camforward.y = 0;
        Vector3 v = vertical * camforward;
        Vector3 h = horizontal * cam.pivot.transform.right;
        Vector3 movedir = (v + h).normalized;


        Vector3 org = state.transform.position;
        org.y += 0.5f;
        float ray = 0.5f;
        if (Physics.Raycast(org, movedir, out RaycastHit wall, ray, ~LayerMask.GetMask("Door" , "MaınCapsule" , "Enemy" , "Breakable" , "Player")))
        {
            // For guarantee
            if (wall.transform.gameObject.layer == LayerMask.NameToLayer("Stair"))
            {
                org.y += 0.8f;
                Debug.DrawRay(org, movedir * ray, Color.yellow, 10);
                if (Physics.Raycast(org, movedir, out _, ray, LayerMask.GetMask("Stair")))
                {
                    return true;
                }
                return false;
            }
            if(Vector3.Distance(org, wall.point) <= ray)
            return true;
        }
        return false;
    }
}
