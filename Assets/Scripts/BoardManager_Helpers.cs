using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class BoardManager : MonoBehaviour
{
/*
██╗░░██╗███████╗██╗░░░░░██████╗░███████╗██████╗░░██████╗
██║░░██║██╔════╝██║░░░░░██╔══██╗██╔════╝██╔══██╗██╔════╝
███████║█████╗░░██║░░░░░██████╔╝█████╗░░██████╔╝╚█████╗░
██╔══██║██╔══╝░░██║░░░░░██╔═══╝░██╔══╝░░██╔══██╗░╚═══██╗
██║░░██║███████╗███████╗██║░░░░░███████╗██║░░██║██████╔╝
╚═╝░░╚═╝╚══════╝╚══════╝╚═╝░░░░░╚══════╝╚═╝░░╚═╝╚═════╝░
*/

    //Builds vector3 based on horizontal and vertical inputs
    private Vector3 GetInputVector()
    {
        return new Vector3(Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")), 0, Mathf.RoundToInt(Input.GetAxisRaw("Vertical")));
    }

    //Scans all units in the room and checks if they're moving
    private bool AnyUnitsMoving()
    {
        List<Unit> units = FindObjectsOfType<Unit>().ToList();
        foreach (var unit in units)
        {
            if (unit.moving)
            {
                return true;
            }
        }
        return false;
    }

    //Scans all units in the room and checks if any desired nodes collide
    private bool IsDuplicateDesiredNode(Node toCheck)
    {
        List<Unit> units = GameObject.FindObjectsOfType<Unit>().Where(x => x.GetDesiredNode() != null).ToList();
        foreach (var item in units)
        {
            if (toCheck.GetWorldPoint() == item.GetDesiredNode().GetWorldPoint())
            {
                return true;
            }
        }
        return false;
    }

    private void ReadForInputs()
    {
        //Primary attack key pressed
        if (Input.GetKey(KeyCode.JoystickButton2) || Input.GetMouseButton(0))
        {
            ChangeState(PlayerInput.PrimaryAttack);
            return;
        }

        //Rotation mode key pressed
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetAxisRaw("Left Trigger") != 0)
        {
            ChangeState(PlayerInput.RotateMode);
            return;
        }

        //Diagonal mode key pressed
        else if (Input.GetKey(KeyCode.JoystickButton4) || Input.GetKey(KeyCode.LeftControl))
        {
            ChangeState(PlayerInput.DiagonalMode);
            return;
        }

        //Movement key(s) pressed
        else if (GetInputVector() != Vector3.zero)
        {
            ChangeState(PlayerInput.Move);
            return;
        }

        //Rest key pressed
        else if (Input.GetKey(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.Q))
        {
            ChangeState(PlayerInput.Rest);
            return;
        }

        //Loot key pressed
        else if (Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.E))
        {
            ChangeState(PlayerInput.Loot);
            return;
        }

        //No key pressed
        else
        {
            ChangeState(PlayerInput.None);
            return;
        }
    }
}
