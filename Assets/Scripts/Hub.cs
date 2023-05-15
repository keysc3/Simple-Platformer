using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
* Purpose: Hub world to allow for quitting and entering level
*
* @author Colin Keys
*/
public class Hub : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            SceneManager.LoadScene(0, LoadSceneMode.Single); 
        }
    }
}
