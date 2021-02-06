using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public List<Vector3> interactTiles = new List<Vector3> { };
    public Animator animator;

    void Update()
    {
        foreach (Vector3 tile in interactTiles)
        {
            Vector3 check = transform.position + tile;
            if (PlayerUnit.instance.transform.position == check)
            {
                animator.SetTrigger("Popup");
                return;
            }
        }
        animator.SetTrigger("Deactivate");
    }

    private void OnDrawGizmosSelected()
    {
        if (interactTiles.Count != 0)
        {
            //Draws attack range
            foreach (var tile in interactTiles)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position + tile, Vector3.one);
            }
        }
    }
}
