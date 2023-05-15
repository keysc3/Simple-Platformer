using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCube : MonoBehaviour
{

    private Vector3 startPos;
    private Vector3 endPos;
    public float speed = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        startPos = new Vector3(2, 7.1f, 90);
        endPos = new Vector3(2, 7.1f, 100);
    }

    // Update is called once per frame
    void Update()
    {
        float time = Mathf.PingPong(Time.time * speed, 1);
        transform.position = Vector3.Lerp(startPos, endPos, time);
    }
}
