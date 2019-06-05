using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterControl : MonoBehaviour
{
    [HideInInspector]
    public  MovementState move_state;
    protected GameObject player;
    protected Collider left_toe_collider;
    protected Collider right_toe_collider;

    [HideInInspector]
    public Animator anim;

    [HideInInspector]
    public Rigidbody player_rb;
    //[HideInInspector]
    //public Rigidbody left_toe_rb;
    //[HideInInspector]
    //public Rigidbody right_toe_rb;

    [HideInInspector]
    public Vector3 velocity;

    public string character_tree_address = "Character";
    public string character_left_foot_tree_address = "Left Toe";
    public string character_right_foot_tree_address = "Right Toe";
    public float foot_normal_angle = 60; // above this angle, the character will be falling/jumping

    public string animation_speed_id = "Speed";
    public float animation_speed_multiplier = 1.0f;
    public string animation_direction_id = "Direction";
    public string animation_is_jumping_id = "Jump";
    public string animation_is_running_id = "Loco";
    public string animation_is_idle_id = "Idle";
    public string animation_jump_velocity_id = "JumpVelocity";
    public string animation_jump_gravity_id = "GravityControl";

    public string jump_button = "Jump";

    bool left_foot_caught = false;
    bool right_foot_caught = false;

    [HideInInspector]
    public bool ready = false;
    private float minimumExtent;
    private float partialExtent;
    private float sqrMinimumExtent;
    private Vector3 previousPosition;
    private Collider player_collider;
    public float skinWidth = 0.1f; //probably doesn't need to be changed 
    public LayerMask layerMask = -1; //make sure we aren't in this layer 

    public void Start()
    {
        move_state = new RunningMovementState(this);

        player = GetComponent<GameObject>();
        player_collider = GetComponent<Collider>();
        player_rb = GetComponent<Rigidbody>();
        left_toe_collider = GameObject.Find(character_left_foot_tree_address).GetComponent<Collider>();
        //left_toe_rb = GameObject.Find(character_left_foot_tree_address).GetComponent<Rigidbody>();
        right_toe_collider = GameObject.Find(character_right_foot_tree_address).GetComponent<Collider>();
        //right_toe_rb = GameObject.Find(character_left_foot_tree_address).GetComponent<Rigidbody>();

        anim = GetComponent<Animator>();
        

        ready = true;

        previousPosition = player_rb.position;
        minimumExtent = Mathf.Min(Mathf.Min(player_collider.bounds.extents.x, player_collider.bounds.extents.y), player_collider.bounds.extents.z);
        partialExtent = minimumExtent * (1.0f - skinWidth);
        sqrMinimumExtent = minimumExtent * minimumExtent;

    }

    private void OnCollisionEnter(Collision collision)
    {
        bool foot_caught = false;
        foreach(ContactPoint con in collision.contacts)
        {
            if (Vector3.Angle(con.normal, Vector3.up) <= foot_normal_angle) {
                if (con.thisCollider == left_toe_collider)
                {
                    left_foot_caught = true;
                    if (right_foot_caught)
                    {
                        break;
                    }
                }
                if (con.thisCollider == right_toe_collider)
                {
                    right_foot_caught = true;
                    if (left_foot_caught)
                    {
                        break;
                    }
                }

            }
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        OnCollisionEnter(collision);
    }

    void DontGoThroughThings(Collider col, Rigidbody rb, Action on_contact = null)
    {
        {
            //have we moved more than our minimum extent? 
            Vector3 movementThisStep = rb.position - previousPosition;
            float movementSqrMagnitude = movementThisStep.sqrMagnitude;

            if (movementSqrMagnitude > sqrMinimumExtent)
            {
                float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
                RaycastHit hitInfo;

                //check for obstructions we might have missed 
                if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementMagnitude, layerMask.value))
                {
                    if (!hitInfo.collider)
                        return;

                    if (hitInfo.collider.isTrigger)
                        hitInfo.collider.SendMessage("OnTriggerEnter", col);

                    if (!hitInfo.collider.isTrigger)
                        rb.position = hitInfo.point - (movementThisStep / movementMagnitude) * partialExtent;

                    on_contact?.Invoke();


                }
            }

            previousPosition = rb.position;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!ready)
        {
            return;
        }

        DontGoThroughThings(player_collider, player_rb);
        //DontGoThroughThings(left_toe_collider, player_rb, delegate { right_foot_caught = true;});
        //DontGoThroughThings(right_toe_collider, player_rb, delegate { left_foot_caught = true; });

        InputTransform inp = new InputTransform();
        move_state.Move(inp, transform);
        anim.SetFloat(animation_speed_id, (float)move_state.anim_speed);
        anim.SetFloat(animation_direction_id, (float)move_state.anim_direction);
        anim.speed = (float)move_state.anim_speed;

        player_rb.useGravity = true;

        if (Input.GetButtonDown(jump_button) && move_state.is_grounded)
        {
            move_state = new JumpingMovementState(this);
            //GetComponent<Rigidbody>().AddForce(Vector3.up * 1.0f);
            right_foot_caught = false;
            left_foot_caught = false;
        }
        else if (right_foot_caught || left_foot_caught)
        {
            move_state.FootGrounded();
        }
        else
        {
            move_state.FootUngrounded();
        }

        move_state.FixedUpdate();
        velocity = transform.TransformDirection(velocity);
        //Debug.Log($"velocity: {velocity}");
        player_rb.velocity = new Vector3(velocity.x, player_rb.velocity.y, velocity.z);
        //Debug.Log($"pos: {transform.localPosition}");
        Debug.Log(move_state.GetType().ToString());
    }

}
