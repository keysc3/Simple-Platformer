using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
* Purpose: Implements a singleton game controller to handle death, level loading, and level ending.
*
* @author Colin Keys
*/
public class GameController : MonoBehaviour
{

    public static GameController instance { get; private set; }
    public bool isFinished = false;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject sacboyPrefab;
    public GameObject mCamera;

    private CameraMovement cameraMovementScript;
    private PauseMenu pauseMenuScript;
    private GameOverMenu gameOverMenuScript;

    public delegate void OnDeath();
    public static event OnDeath onDeath;

    public delegate void OnGameOver();
    public static event OnGameOver onGameOver;

    // Called when the script instance is being loaded.
    void Awake(){
        pauseMenuScript = pauseMenu.GetComponent<PauseMenu>();
        gameOverMenuScript = gameOverMenu.GetComponent<GameOverMenu>();
        cameraMovementScript = mCamera.GetComponent<CameraMovement>();
        instance = this;
    }

    // Called before the first frame update.
    void Start(){
        Time.timeScale = 1f;
        PauseMenu.isPaused = false;
        SacboyController.onPause += pauseMenuScript.StartPress;
        onGameOver += gameOverMenuScript.GameOver;
    }

    /*
    *   RestartLevel - Gets the current active scene and loads it again for a restart.
    */
    public void RestartLevel(){   
        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single); 
    }
    
    /*
    *   QuitLevel - Loads the hub world scene.
    */
    public void QuitLevel(){
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
    
    /*
    *  FinishLevel - Starts the level finished sequence.
    */
    public void FinishLevel(){
        StartCoroutine(LevelCompleted());
    }

    /*
    *   LevelCompleted - Coroutine for the level finished sequence.
    */
    IEnumerator LevelCompleted(){
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    /*
    *   Death - Handles the death of the player by destroying them and creating a new player GameObject
    *   at the activated checkpoint if it has any lives, otherwise activates the game over event.
    */
    public void Death(){
        Checkpoint currentCheckpoint = CheckpointManager.instance.CurrentCheckpoint;
        // If the player is not out of lives
        if(CheckpointManager.instance.CurrentCheckpoint.lives != 0){
            // Call the onDeath event and create a new player at the checkpoints location.
            onDeath?.Invoke();
            // Determine lane to spawn player based on where the checkpoint is.
            float xSpawn = currentCheckpoint.gameObject.transform.position.x;
            if(xSpawn == -4f)
                xSpawn = -2f;
            else if (xSpawn == -1f)
                xSpawn = 0f;
            else
                xSpawn = 2f;
            GameObject sacboy = Instantiate(sacboyPrefab, new Vector3(xSpawn, currentCheckpoint.gameObject.transform.position.y + 0.2f, 
            currentCheckpoint.gameObject.transform.position.z), Quaternion.identity);
            // Set the cameras target to the new player GameObject.
            cameraMovementScript.targetObject = sacboy.transform;
        }
        else{
            // Call the onGameOver event.
            onGameOver?.Invoke();
        }
    }

    // Called when the object is destroyed or the behavior is disabled.
    private void OnDisable(){
        // Remove events.
        SacboyController.onPause -= pauseMenuScript.StartPress;
        onGameOver -= gameOverMenuScript.GameOver;
    }
}
