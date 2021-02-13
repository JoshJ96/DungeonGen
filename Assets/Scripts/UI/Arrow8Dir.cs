using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow8Dir : MonoBehaviour
{
    void Update()
    {
        transform.position = new Vector3(
            PlayerUnit.instance.transform.position.x,
            -0.49f,
            PlayerUnit.instance.transform.position.z+1);    
    }
}
