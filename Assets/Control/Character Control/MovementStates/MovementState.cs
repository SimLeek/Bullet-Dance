using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementState
{
    public CharacterControl parent;

    public double rotateSpeed = 6.0f;

    public double anim_speed=1.5;
    public double anim_direction=0.0;


    public virtual bool is_grounded { get; protected set; }

    public MovementState(CharacterControl parent)
    {
        this.parent = parent;
    }

    virtual public void FixedUpdate()
    {
        
    }

    virtual public void Move(InputTransform inp, Transform transform)
    {
        if (inp.h > 0.2)
        {
            anim_direction = 1.0f;
        }
        else if (inp.h < -0.2)
        {
            anim_direction = -1.0f;
        }
        else
        {
            anim_direction = 0.0f;
        }

        parent.transform.Rotate(0, (float)(inp.r * rotateSpeed), 0);

    }

    virtual public void FootGrounded() {
    }

    virtual public void FootUngrounded()
    {
    }


}