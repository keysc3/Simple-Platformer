using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Purpose: Handles the player colliding with a death collider.
*
* @author Colin Keys
*/
public class SacboyDeath : MonoBehaviour
{

    // Called when this scripts GameObject collides with another GameObject where one has a non-kinematic rigidbody.
    private void OnCollisionEnter(Collision collision){
        // If player hit a death collider, kill them and call the Death function.
        if (collision.gameObject.tag == "Death"){
            Destroy(gameObject);
            GameController.instance.Death();
        }
    }
}
