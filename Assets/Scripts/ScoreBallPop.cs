using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Purpose: Handles the score ball colliding with a player.
*
* @author Colin Keys
*/
public class ScoreBallPop : MonoBehaviour
{
    public int scoreAmount = 10;

    // Called when this scripts GameObject collides with another GameObject.
    private void OnTriggerEnter(Collider other){
        // If the player collided with this GameObject add score and destroy this score ball.
        if(other.tag == "Player"){
            ScoreText.instance.AddScore(scoreAmount);
            Destroy(transform.parent.gameObject);
        }
    }
}
