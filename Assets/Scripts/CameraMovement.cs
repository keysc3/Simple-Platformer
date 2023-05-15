using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Purpose: Implements camera movement for following the player
*
* @author Colin Keys
*/
public class CameraMovement : MonoBehaviour
{
    public Transform targetObject;
    public float xOffset = 12.0f;
    public float yOffset = 3.0f;
    public float zOffset = 1.0f;
    public float smoothness = 0.5f;

    // Called after all update functions have been called.
    void LateUpdate() {
        // If the object to be following is set.
        if(targetObject != null){
            // If the player has not finished the level (on the finish platform).
            if(!GameController.instance.isFinished){
                // Set camera to a specific x value with y and z values offset from the player transform.
                Vector3 newPosition = new Vector3(xOffset, targetObject.transform.position.y + yOffset, 
                targetObject.transform.position.z + zOffset);
                transform.position = Vector3.Slerp(transform.position, newPosition, smoothness);
            }
        }
    }
}
