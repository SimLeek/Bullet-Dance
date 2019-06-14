using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JumpingMovementState : MovementState
{
    public override bool is_grounded { get { return false; } protected set { } }

    public float timer=2.0f;
    public float float_speed = 3.0f;

    public Vector3 start_velocity;

    public JumpingMovementState(CharacterControl parent) : base(parent)
    {
        parent.anim.SetBool(parent.animation_is_jumping_id, true);
        parent.anim.Play(parent.animation_is_jumping_id, -1, .1f);
        parent.anim.SetBool(parent.animation_is_idle_id, false);
        start_velocity = parent.velocity;
;       parent.player_rb.AddForce(parent.jump_force * Vector3.up);

    }

    public override void FootGrounded()
    {
        parent.anim.SetBool(parent.animation_is_jumping_id, false);
        parent.move_state = new IdleMovementState(parent);
    }

    public override void Move(InputTransform inp, Transform transform)
    {
        base.Move(inp, transform);

        parent.velocity = new Vector3((float)(start_velocity.x + float_speed * Mathf.Cos((float)inp.direction) * inp.magnitude), parent.player_rb.velocity.y, (float)(start_velocity.z + float_speed * Mathf.Sin((float)inp.direction) * inp.magnitude));
        Debug.Log($"Move()->float: {parent.velocity}");
    }

    public override void FixedUpdate()
    {
        //float jumpVelocity = parent.anim.GetFloat(parent.animation_jump_velocity_id);
        //parent.transform.localPosition += Vector3.Normalize(Vector3.up) * jumpVelocity * Time.fixedDeltaTime;
        //float gravityControl = parent.anim.GetFloat(parent.animation_jump_gravity_id);
        //if (gravityControl > 0)
        //{
        //    parent.player_rb.useGravity = false;
        //}
        //parent.player_rb.AddForce(parent.transform.up * 3.0f);
        //parent.player_rb.AddForce(parent.jump_velocity * Vector3.up);
        if (timer <= 0f)
        {
            parent.anim.SetBool(parent.animation_is_jumping_id, false);
            parent.move_state = new FallingMovementState(parent);
        }

        timer -= Time.fixedDeltaTime;
    }


}
