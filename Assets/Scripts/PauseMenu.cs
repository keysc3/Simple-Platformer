using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/*
* Purpose: Actives the pause menu and handles its on clicks.
*
* @author Colin Keys
*/
public class PauseMenu : MonoBehaviour
{
    public static bool isPaused;

    private EventSystem eventSystem;
    private Sacboy sacboyRef;

    // Called when the object becomes enabled and active.
    void OnEnable(){
        eventSystem = EventSystem.current;
    }

    /*
    *   StartPress - Calls for pause or resume of the game depending on the isPaused variable.
    *   @param sacboy - Sacboy instance.
    */
    public void StartPress(Sacboy sacboy){
        if(sacboyRef == null)
            sacboyRef = sacboy;
        if(!isPaused)
            Pause();
        else
            Resume();
    }

    /*
    *   Pause - Handle the game being paused.
    */
    public void Pause(){
        // Activate the pause menu and set the selected button to the first one.
        gameObject.SetActive(true);
        eventSystem.SetSelectedGameObject(gameObject.transform.GetChild(0).gameObject);
        // Disable regular player actions and enable UI actions.
        sacboyRef.Actions.Disable();
        sacboyRef.UI.Enable();
        sacboyRef.UI.Cancel.performed += Cancel;
        // Game is paused and freeze the game state.
        isPaused = true;
        Time.timeScale = 0f;
    }

    /*
    *   Pause - Handle the game being resumed.
    */
    public void Resume(){
        // Deactivate the pause menu.
        gameObject.SetActive(false);
        // Enable regular player actions and disable pause menu actions.
        sacboyRef.Actions.Enable();
        sacboyRef.UI.Disable();
        sacboyRef.UI.Cancel.performed -= Cancel;
        // Game is no longer paused and unfreeze the game state.
        isPaused = false;
        Time.timeScale = 1f;
    }

    /*
    *   Quit - Handle the game being pquit.
    */
    public void Quit(){
        sacboyRef.UI.Cancel.performed -= Cancel;
        GameController.instance.QuitLevel();
    }

    /*
    *   Restart - Handle the game being restarted.
    */
    public void Restart(){
        sacboyRef.UI.Cancel.performed -= Cancel;
        GameController.instance.RestartLevel();
    }

    /*
    *   Cancel - Cancel performed action for the UI action map. Resumes the game when the cancel button is pressed.
    */
    public void Cancel(InputAction.CallbackContext context){
        Resume();
    }
}
