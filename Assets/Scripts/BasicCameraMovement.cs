using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCameraMovement : MonoBehaviour
{
    public GameObject target;
    public Vector3 offset;
    [Range(0, 20)]
    public float smoothSpeed;

    private void Start()
    {
        //transform.position = new Vector3(
        //        target.transform.position.x,
        //        transform.position.y,
        //        target.transform.position.z) + offset;
    }

    void Update()
    {
        if (FindObjectOfType<PlayerUnit>() == null)
        {
            return;
        }
        Vector3 startVector = transform.position;
        if (PlayerUnit.instance.GetDesiredNode() != null)
        {
            Vector3 destVector = new Vector3(
                PlayerUnit.instance.GetDesiredNode().worldPosition.x,
                transform.position.y,
                PlayerUnit.instance.GetDesiredNode().worldPosition.z
            );

            if (startVector != destVector + offset)
            {
                transform.position = Vector3.Lerp(startVector, destVector + offset, smoothSpeed * Time.deltaTime);
            }
        }
        else
        {
            Vector3 destVector = new Vector3(
                PlayerUnit.instance.transform.position.x,
                transform.position.y,
                PlayerUnit.instance.transform.position.z
            );

            if (startVector != destVector + offset)
            {
                transform.position = Vector3.Lerp(startVector, destVector + offset, smoothSpeed * Time.deltaTime);
            }
        }
    }
}
