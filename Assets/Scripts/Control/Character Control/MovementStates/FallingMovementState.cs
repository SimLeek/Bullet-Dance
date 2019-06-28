using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class FallingMovementState : MovementState
{
    public override bool is_grounded { get { return false; } protected set { } }

    public float float_speed = 1.0f;

    public Vector3 start_velocity;

    public FallingMovementState(CharacterControl parent) : base(parent)
    {
        start_velocity = parent.velocity;
    }

    public override void FootGrounded()
    {
        parent.move_state = new IdleMovementState(parent);
    }

    public override void Move(InputTransform inp, Transform transform)
    {
        base.Move(inp, transform);
        parent.velocity = new Vector3((float)(start_velocity.x + float_speed * Mathf.Cos((float)inp.direction) * inp.magnitude), parent.velocity.y, (float)(start_velocity.z + float_speed * Mathf.Sin((float)inp.direction) * inp.magnitude));
        Debug.Log($"Move()->fall->float: {parent.velocity}");
    }

    public override void FixedUpdate()
    {
        float fallVelocity = parent.anim.GetFloat(parent.animation_jump_velocity_id);
        parent.transform.localPosition += Vector3.Normalize(Vector3.up) * fallVelocity * Time.fixedDeltaTime;
        float gravityControl = parent.anim.GetFloat(parent.animation_jump_gravity_id);
        if (gravityControl > 0)
        {
            parent.player_rb.useGravity = false;
        }
    }
}
