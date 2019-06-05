using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingMovementState : MovementState
{
    public override bool is_grounded { get { return false; } protected set { } }

    public FallingMovementState(CharacterControl parent) : base(parent)
    {
    }

    public override void FootGrounded()
    {
        parent.move_state = new IdleMovementState(parent);
    }

    public override void Move(InputTransform inp, Transform transform)
    {
        base.Move(inp, transform);
    }
}
