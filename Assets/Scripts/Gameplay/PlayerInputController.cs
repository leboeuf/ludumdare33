using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerInputController : CharacterController
{
	public Controller Controller;

    private KeyCode swimButton;
    private string horizontalAxis;
    private string verticalAxis;
    private bool keyboard;

    void Start()
    {

    }

    protected override float AngularSpeed()
    {
        if (keyboard)
            return angularSpeed / 1.0f;
        return angularSpeed;
    }

    protected override Vector3 GetMovementAxis()
	{
		Vector3 axis = new Vector3 (Input.GetAxis (horizontalAxis), Input.GetAxis (verticalAxis), 0f);

        if (keyboard)
        {
            float minLength = 0.0f;
            if (axis.sqrMagnitude < (minLength * minLength))
            {
                axis = Vector3.zero;
            }
        }
        else
        {
            float deadZone = 0.1f;
            if (Math.Abs(axis.x) < deadZone)
                axis.x = 0f;
            if (Math.Abs(axis.y) < deadZone)
                axis.y = 0f;
        }

		return axis;         
    }

    protected override bool IsSwimming()
    {
        return (Input.GetKeyDown(swimButton));
    }

    public void SetControls(Controller controls)
    {
		Controller = controls;

        keyboard = false;
        switch (controls)
        {
            case Controller.Joystick1:
                horizontalAxis = "Horizontal1";
                verticalAxis = "Vertical1";
                swimButton = KeyCode.Joystick1Button0;
                break;
            case Controller.Joystick2:
                horizontalAxis = "Horizontal2";
                verticalAxis = "Vertical2";
                swimButton = KeyCode.Joystick2Button0;
                break;
            case Controller.Joystick3:
                horizontalAxis = "Horizontal3";
                verticalAxis = "Vertical3";
                swimButton = KeyCode.Joystick3Button0;
                break;
            case Controller.Joystick4:
                horizontalAxis = "Horizontal4";
                verticalAxis = "Vertical4";
                swimButton = KeyCode.Joystick4Button0;
                break;
            case Controller.Keyboard:
                horizontalAxis = "Horizontal";
                verticalAxis = "Vertical";
                swimButton = KeyCode.Space;
                keyboard = true;
                break;
        }
    }
}
