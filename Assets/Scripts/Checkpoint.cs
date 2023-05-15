using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Purpose: Checkpoint script to handle a checkpoints actions.
*
* @author Colin Keys
*/
public class Checkpoint : MonoBehaviour
{   
    [SerializeField] private Material activated;
    [SerializeField] private Material deactivated;

    public bool triggered;
    public int lives;

    public delegate void OnCheckTrigger(Checkpoint newCheckpoint);
    public event OnCheckTrigger onCheckTrigger;

    // Called when the script instance is being loaded.
    void Awake(){
        triggered = false;
        lives = 4;
    }

    // Called when this scripts GameObject collides with another GameObject.
    private void OnTriggerEnter(Collider other){
        // Only activate checkpoint if the player touched it and it hasn't been activated yet.
        if(other.tag == "Player" && triggered == false){
            triggered = true;
            // Set onDeath event to call OnPlayerDeath.
            GameController.onDeath += OnPlayerDeath;
            // Trigger onCheckTrigger event when the checkpoint is activated.
            onCheckTrigger(this);
            // Set the active material.
            ChangeCheckpointMaterial(activated);
        }
    }

    /*
    *   OnPlayerDeath - Takes one life off of the current activated checkpoint if possible and destroys a life displaying GameObject.
    */
    void OnPlayerDeath(){
        // If the current checkpoint is this one and it has lives left, lose one.
        if(lives > 0 && CheckpointManager.instance.CurrentCheckpoint == this){
            lives -= 1;
            // Destroy a life displaying GameObject.
            GameObject currentLife = gameObject.transform.GetChild(lives).gameObject;
            Destroy(currentLife);
        }
    }

    /*
    *   ChangeCheckpointMaterial - Changes the checkpoints life displaying objects materials.
    *   @param material - Material to change the objects children to.
    */
    void ChangeCheckpointMaterial(Material material){
        for(int i = 0; i < transform.childCount; i++){
            gameObject.transform.GetChild(i).gameObject.GetComponent<Renderer>().material = material;
        }
    }

    /*
    *   OnCheckpointDeactivated - Deactivates checkpoint by changing the material and removing the onDeath event call to OnPlayerDeath.
    */
    public void OnCheckpointDeactivated(){
        ChangeCheckpointMaterial(deactivated);
        GameController.onDeath -= OnPlayerDeath;
    }
}
