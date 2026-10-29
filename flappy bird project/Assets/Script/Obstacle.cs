using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Obstacle : MonoBehaviour
{
    private float highPosY = 1f;
    private float lowPosY = -1f;

    private float holeSize = 3f;

    public Transform upObject;
    public Transform downObject;

    public float widthPadding = 3f;
    
    GameManager gameManager;

    public void Start()
    {
        gameManager = GameManager.Instance;
    }
    
    public Vector3 SetRandomPlace(Vector3 lastPosition, int obstacleCount)
    {
        float halfHoleSize = holeSize / 2f;
        upObject.localPosition = new Vector3(0, halfHoleSize);
        downObject.localPosition = new Vector3(0, -halfHoleSize);

        Vector3 placePosition = lastPosition + new Vector3(widthPadding, 0);
        placePosition.y = Random.Range(lowPosY, highPosY);
        
        transform.position = placePosition;
        
        return placePosition;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        gameManager.AddScore(1);
    }
}
