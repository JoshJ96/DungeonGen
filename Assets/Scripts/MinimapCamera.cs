using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{

    Vector3 fixedTransform;
    int fixedOrthographicSize;
    public int zoomOrthographicSize;
    int startX, startY;

    Camera cam;

    public enum State
    {
        Fixed,
        Zoom
    }

    private State CurrentState = State.Zoom;

    public State currentState
    {
        private get
        {
            return CurrentState;
        }
        set
        {
            switch (value)
            {
                case State.Fixed:
                    transform.position = fixedTransform;
                    cam.orthographicSize = fixedOrthographicSize;
                    break;
                case State.Zoom:
                    cam.orthographicSize = zoomOrthographicSize;
                    transform.position = new Vector3(PlayerUnit.instance.transform.position.x, 1, PlayerUnit.instance.transform.position.z);
                    break;
                default:
                    break;
            }
            CurrentState = value;

        }
    }


    private void Awake()
    {
        cam = GetComponent<Camera>();
        FindObjectOfType<GameEvents>().pushDungeonData += PushDungeonData;
    }

    public void PushDungeonData(DungeonData data)
    {
        startX = (int) data.playerSpawnRoom.V3Center.x;
        startY = (int) data.playerSpawnRoom.V3Center.y;
        currentState = State.Zoom;
        fixedTransform = transform.position = new Vector3(data.mapWidth / 2, 1, data.mapHeight / 2);
        fixedOrthographicSize = data.mapWidth / 2;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Fixed:
                transform.position = fixedTransform;
                cam.orthographicSize = fixedOrthographicSize;
                break;
            case State.Zoom:
                Vector3 targetPos = new Vector3(PlayerUnit.instance.transform.position.x, 1, PlayerUnit.instance.transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 1.5f);
                cam.orthographicSize = zoomOrthographicSize;
                break;
            default:
                break;
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            Toggle();
        }
    }



    public void Toggle()
    {
        switch (CurrentState)
        {
            case State.Fixed:
                currentState = State.Zoom;
                break;
            case State.Zoom:
                currentState = State.Fixed;
                break;
            default:
                break;
        }
    }

}
