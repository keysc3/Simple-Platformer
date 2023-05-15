using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
* Purpose: Actives the game over menu and handles its on clicks.
*
* @author Colin Keys
*/
public class GameOverMenu : MonoBehaviour
{
    private EventSystem eventSystem;


    // Called when the object becomes enabled and active.
    void OnEnable(){
        eventSystem = EventSystem.current;
    }

    /*
    *   GameOver - Displays the game over menu and sets the first button to the selected button.
    */
    public void GameOver(){
        gameObject.SetActive(true);
        eventSystem.SetSelectedGameObject(gameObject.transform.GetChild(0).gameObject);
    }

    /*
    *   Retry - Restarts the level.
    */
    public void Retry(){
        GameController.instance.RestartLevel();
    }

    /*
    *   Retry - Quits the level.
    */
    public void Quit(){
        GameController.instance.QuitLevel();
    }
}
