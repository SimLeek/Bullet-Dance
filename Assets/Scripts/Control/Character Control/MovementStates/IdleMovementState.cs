using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleMovementState : MovementState
{
    public override bool is_grounded { get { return true; } protected set { } }

    public IdleMovementState(CharacterControl parent) : base(parent)
    {
        parent.anim.SetBool(parent.animation_is_idle_id, true);
        anim_speed = 1.0f;
        parent.velocity = Vector3.zero;
    }

    public override void FootUngrounded()
    {
        parent.move_state = new FallingMovementState(parent);
    }

    public override void Move(InputTransform inp, Transform transform)
    {
        if (inp.magnitude > 0.2)
        {
            parent.anim.SetBool(parent.animation_is_idle_id, false);
            parent.move_state = new RunningMovementState(parent);
            parent.move_state.Move(inp, transform);
        }
        base.Move(inp, transform);
    }
}
