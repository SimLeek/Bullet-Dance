using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RunningMovementState : MovementState
{
    public override bool is_grounded { get { return true; } protected set { } }

    public double runSpeed = 6f;  // A good 100m sprint speed in meters per second
    public double sprintSpeed = 10.0f;  // Usain Bolt's sprinting speed in meters per second.

    public double rotateDieOff = 0.1f;

    public double sprintStateEndInput = 0.5f;

    private bool sprintState = false;

    private DoubleTapDetect doubleTapDetector = new DoubleTapDetect();
    public float timer=0.2f;

    public RunningMovementState(CharacterControl parent) : base(parent)
    {
        if (parent.ready)
        {
            parent.anim.SetBool(parent.animation_is_running_id, true);
        }
        
    }

    public override void Move(InputTransform inp, Transform transform)
    {
        base.Move(inp, transform);

        if (doubleTapDetector.DetectDirectional(inp))
        {
            sprintState = true;
        }
        if (Math.Abs(inp.magnitude) < doubleTapDetector.doubleTapCloseness)
        {
            sprintState = false;
        }

        if (sprintState)
        {
            parent.velocity = new Vector3((float)(sprintSpeed * Math.Cos(inp.direction) * inp.magnitude), 0, (float)(sprintSpeed * Math.Sin(inp.direction) * inp.magnitude));
            Debug.Log($"Move()->sprint: {parent.velocity}");
        }
        else
        {
            parent.velocity = new Vector3((float)(runSpeed * Math.Cos(inp.direction) * inp.magnitude), 0, (float)(runSpeed * Math.Sin(inp.direction) * inp.magnitude));
            Debug.Log($"Move()->run: {parent.velocity}");
        }
        anim_speed = parent.velocity.magnitude/3.0f;
    }

    public override void FixedUpdate()
    {
        //Debug.Log("FixedUpdate()");
        base.FixedUpdate();

        if(parent.velocity.magnitude< 0.2)
        {
            if(timer <= 0f)
            {
                //Debug.Log("FixedUpdate()->exit to idle");
                parent.anim.SetBool(parent.animation_is_running_id, false);
                parent.move_state = new IdleMovementState(parent);
            }
            else
            {
                //Debug.Log("FixedUpdate()->decrease timer");
                timer -= Time.fixedDeltaTime;
            }
            
        }
        else
        {
            //Debug.Log("FixedUpdate()->reset timer");
            timer = 0.2f;
        }

    }

    public override void FootUngrounded()
    {
        parent.anim.SetBool(parent.animation_is_running_id, false);
        parent.move_state = new FallingMovementState(parent);
    }

}