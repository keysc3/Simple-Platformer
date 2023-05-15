using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Purpose: Checks for the player entering the finish and activates the end level sequence.
*
* @author Colin Keys
*/
public class Finish : MonoBehaviour
{
    public GameObject  mCamera;
    public GameObject leftBarrier;
    public GameObject rightBarrier;
    public float xOffset = 8.0f;
    public float yOffset = 2.0f;
    public float zOffset = 0f;

    // Called when this scripts GameObject collides with another GameObject.
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            GameController.instance.isFinished = true;
            FinishEntered();
            GameController.instance.FinishLevel();
            
        }
    }

    /*
    * FinishedEntered - Moves the camera to a locked position on the finish and activates the blocking barriers so the player can't move away.
    */
    private void FinishEntered(){
        mCamera.transform.position = new Vector3(transform.position.x + xOffset, transform.position.y + yOffset, transform.position.z + zOffset);
        leftBarrier.SetActive(true);
        rightBarrier.SetActive(true);
    }
}
