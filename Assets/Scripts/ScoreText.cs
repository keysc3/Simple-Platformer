using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
* Purpose: Handles the games Score using a singleton.
*
* @author Colin Keys
*/
public class ScoreText : MonoBehaviour
{
    private TMP_Text text;
    private int score;

    public static ScoreText instance { get; private set; }

    // Called when the script instance is being loaded.
    void Awake(){
        text = GetComponent<TMP_Text>();
        instance = this;
    }

    /*
    *   AddScore - Adds score to the score total based off the given score value.
    *   @param value - int to add to the score total.
    */
    public void AddScore(int value){
        score += value;
        text.SetText(score.ToString());
    }
}
