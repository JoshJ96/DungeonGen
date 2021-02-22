using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissapear : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        GameEvents.instance.turnPass += TurnPass;
    }

    private void TurnPass()
    {
        if (Vector3.Distance(this.transform.position, PlayerUnit.instance.transform.position) > 10.0f)
        {
            meshRenderer.enabled = false;
        }
        else
        {
            meshRenderer.enabled = true;
        }
    }
}