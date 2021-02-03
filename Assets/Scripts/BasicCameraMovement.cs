using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCameraMovement : MonoBehaviour
{
    public GameObject target;
    public Vector3 offset;
    [Range(0, 20)]
    public float smoothSpeed;

    void Update()
    {
        Vector3 startVector = transform.position;
        Vector3 destVector = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);


        if (startVector != destVector + offset)
        {
            transform.position = Vector3.Lerp(startVector, destVector + offset, smoothSpeed * Time.deltaTime);
        }
    }
}
