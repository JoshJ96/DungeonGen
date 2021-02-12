using System;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public List<Vector3> interactTiles = new List<Vector3> { };
    public Animator animator;

    private void Start()
    {
        GameEvents.instance.turnPass += TurnPass;
    }

    private void TurnPass()
    {
        foreach (Vector3 tile in interactTiles)
        {
            Vector3 check = transform.position + tile;
            if (PlayerUnit.instance.transform.position.x == check.x
                && PlayerUnit.instance.transform.position.z == check.z)
            {
                if (PlayerUnit.instance.GetDirection() == Unit.Direction.North)
                {
                    animator.SetTrigger("Popup");
                    return;
                }
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
