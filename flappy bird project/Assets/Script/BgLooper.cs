using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BgLooper : MonoBehaviour
{
    public int NumBgCount = 3;
    public float scaleMultiplier = 1.5f;
    
    public float PipeMax = 0.0f;
    public float PipeMin = 0.0f;

    public int obestacleCount = 0;
    public Vector3 obstacleLastPosition  = Vector3.zero;
    void Start()
    {
        Obstacle[] obstacles = GameObject.FindObjectsOfType<Obstacle>();
        obstacleLastPosition = obstacles[0].transform.position;
        obestacleCount = obstacles.Length;
        
        for(int i = 0; i < obestacleCount; i++)
        {
            obstacleLastPosition = obstacles[i].SetRandomPlace(obstacleLastPosition, obestacleCount);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Triggered: " + collision.name);

        if (collision.CompareTag("BackGround"))
        {
            float widthOfBgObject = ((BoxCollider2D)collision).size.x;
            Vector3 pos = collision.transform.position;

            pos.x += widthOfBgObject * NumBgCount * scaleMultiplier;
            collision.transform.position = pos;
        }

        else if (collision.tag == "Obstacle")
        {
            Obstacle obstacle = collision.GetComponent<Obstacle>();
            obstacleLastPosition = obstacle.SetRandomPlace(obstacleLastPosition, obestacleCount);
        }

    

    }
}