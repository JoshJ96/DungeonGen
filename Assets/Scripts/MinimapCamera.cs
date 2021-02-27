using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{

    Vector3 fixedTransform;
    int fixedOrthographicSize;
    public int zoomOrthographicSize;

    Camera cam;

    public enum State
    {
        Fixed,
        Zoom
    }

    public State currentState = State.Zoom;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        GameEvents.instance.pushDungeonData += PushDungeonData;
    }

    public void PushDungeonData(DungeonData data)
    {
        print("hi");
        fixedTransform = transform.position = new Vector3(data.mapWidth / 2, 1, data.mapHeight / 2);
        fixedOrthographicSize = data.mapWidth / 2;
    }

    private void Update()
    {
        switch (currentState)
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
    }



}
