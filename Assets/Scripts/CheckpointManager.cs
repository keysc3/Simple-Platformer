using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Purpose: Implements a singleton checkpoint manager to handle the checkpoints.
*
* @author Colin Keys
*/
public class CheckpointManager : MonoBehaviour
{
    private List<Checkpoint> checkPoints = new List<Checkpoint>();

    public int currentIndex = 0;
    // Return the current checkpoint if there are any in the checkpoint manager.
    public Checkpoint CurrentCheckpoint {
        get{
            if(checkPoints.Count == 0)
                return null;
            else
                return checkPoints[currentIndex];
        }
    }
    // Create singleton
    public static CheckpointManager instance { get; private set;}

    // Called when the script instance is being loaded.
    void Awake(){
        instance = this;
        // Add each checkpoint child of this object to the checkpoints list and set the onCheckTrigger event.
        for (int i = 0; i < transform.childCount; i++){
            Checkpoint checkpoint = transform.GetChild(i).GetComponent<Checkpoint>();
            checkpoint.onCheckTrigger += OnCheckpointTriggered;
            checkPoints.Add(checkpoint);
        }
    }

    /*
    *   OnCheckpointTriggered - Changed the current checkpoint index to whichever one was triggered and deactivates the currently activated one.
    *   @param newCheckpoint - Checkpoint that has been activated.
    */
    public void OnCheckpointTriggered(Checkpoint newCheckpoint){
        // Set the current index to the last index and set new current index
        // Setting the last checkpoint index allows for checkpoint activation to be done in any index order instead of ascending.
        int lastIndex = currentIndex;
        currentIndex = checkPoints.IndexOf(newCheckpoint);
        // First checkpoint activated will always be 0 since the player spawns on it.
        // Deactivate the last checkpoint.
        if(currentIndex != 0){
            checkPoints[lastIndex].OnCheckpointDeactivated();
        }
    }

}
