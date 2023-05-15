using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Purpose: Creates or destroys a spring joint between the player and the grabbed GameObject.
*
* @author Colin Keys
*/
public class GrabJoint : MonoBehaviour
{
    [SerializeField] private float anchor = 1.0f;
    [SerializeField] private float minDistance = 0.0f;
    [SerializeField] private float maxDistance = 1.2f;
    [SerializeField] private float spring = 1000.0f;
    [SerializeField] private float damper = 10.0f;

    /*
    *   Create - Creates a spring joint between the player and the grabbed GameObject.
    *   @param grabbedPoint - Vector3 of the closest point to the players clavicle from the grabbed GameObject.
    *   @param grabbedObject - GameObject of the grabbed object.
    */
    public void Create(Vector3 grabbedPoint, GameObject grabbedObject){
        // Change grabbedPoint to point relative to grabbed object.
        grabbedPoint = grabbedObject.transform.InverseTransformPoint(grabbedPoint);
        GameObject sacboy = this.gameObject;
        if (grabbedObject.tag == "Moveable")
            grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
        // Create joint on player, connected anchor is collision position.
        // Connected body is object grabbed.
        sacboy.AddComponent<SpringJoint>();
        SpringJoint joint = sacboy.GetComponent<SpringJoint>();
        joint.enableCollision = true;
        joint.anchor = new Vector3(0.0f, anchor, 0.0f);
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = grabbedObject.GetComponent<Rigidbody>();
        joint.connectedAnchor = grabbedPoint;
        joint.spring = spring;
        joint.damper = damper;
        // Min Max distance = arm length.
        joint.minDistance = minDistance;
        joint.maxDistance  = maxDistance;
    }

    /*
    * Destroy - Destroys this GameObjects SpringJoint if one exists.
    */
    public void Destroy(){
        if(this.gameObject.GetComponent<SpringJoint>() != null)
            Destroy(this.gameObject.GetComponent<SpringJoint>());
    }
}
