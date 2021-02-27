using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCameraMovement : MonoBehaviour
{
    [Range(0, 20)]
    public float smoothSpeed;

    public GameObject target;
    public Vector3 offset;

    private void Start()
    {
        GameEvents.instance.pushDungeonData += PushDungeonData;

        transform.position = new Vector3(
            target.transform.position.x,
            transform.position.y,
            target.transform.position.z);
    }

    private void PushDungeonData(DungeonData data)
    {
        Vector3 targetPosition = data.playerSpawnRoom.V3Center;
        transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
    }

    void Update()
    {
        if (target == null) return;

        Vector3 destVector = new Vector3(
            target.transform.position.x,
            transform.position.y,
            target.transform.position.z);

        if (transform.position != destVector + offset)
        {
            transform.position = Vector3.Lerp(transform.position, destVector + offset, smoothSpeed * Time.deltaTime);
        }
    }
}