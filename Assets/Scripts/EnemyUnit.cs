using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    public List<Vector3> CurrentAttackRange = new List<Vector3> { Vector3.zero };

    private void OnDrawGizmos()
    {
        //Draw attack range
        foreach (var tile in CurrentAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + tile, Vector3.one);
        }
    }
}