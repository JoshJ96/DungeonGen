using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow8Dir : MonoBehaviour
{
    public MeshRenderer
        Arrow_North,
        Arrow_Northeast,
        Arrow_East,
        Arrow_Southeast,
        Arrow_South,
        Arrow_Southwest,
        Arrow_West,
        Arrow_Northwest;

    public Material
        arrowPointerMaterial,
        arrowPointerHighlightMaterial;

    private void Update()
    {
        Arrow_North.material = arrowPointerMaterial;
        Arrow_Northeast.material = arrowPointerMaterial;
        Arrow_East.material = arrowPointerMaterial;
        Arrow_Southeast.material = arrowPointerMaterial;
        Arrow_South.material = arrowPointerMaterial;
        Arrow_Southwest.material = arrowPointerMaterial;
        Arrow_West.material = arrowPointerMaterial;
        Arrow_Northwest.material = arrowPointerMaterial;
    }

    void LateUpdate()
    {
        switch (PlayerUnit.instance.facingDirection)
        {
            case Unit.Direction.North:
                Arrow_North.material = arrowPointerHighlightMaterial;
                break;
            case Unit.Direction.Northeast:
                Arrow_Northeast.material = arrowPointerHighlightMaterial;
                break;
            case Unit.Direction.East:
                Arrow_East.material = arrowPointerHighlightMaterial;
                break;
            case Unit.Direction.Southeast:
                Arrow_Southeast.material = arrowPointerHighlightMaterial;
                break;
            case Unit.Direction.South:
                Arrow_South.material = arrowPointerHighlightMaterial;
                break;
            case Unit.Direction.Southwest:
                Arrow_Southwest.material = arrowPointerHighlightMaterial;
                break;
            case Unit.Direction.West:
                Arrow_West.material = arrowPointerHighlightMaterial;
                break;
            case Unit.Direction.Northwest:
                Arrow_Northwest.material = arrowPointerHighlightMaterial;
                break;
            default:
                break;
        }


        transform.position = new Vector3(
            PlayerUnit.instance.transform.position.x,
            -0.49f,
            PlayerUnit.instance.transform.position.z+1);
    }
}
