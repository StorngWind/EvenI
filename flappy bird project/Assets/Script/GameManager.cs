using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager gameManager;

    public static GameManager Instance
    {
        get { return gameManager; }
    }
    
    UIManager uiManager;

    public UIManager UIManager
    {
        get { return uiManager; }
    }

    private int currentScore = 0;
    
    private void Awake()
    {
        gameManager = this;
        FindOfType<UIManager>(out uiManager); 
    }

    public void FindOfType<T>(out T component) where T : Component
    {
        component = FindObjectOfType<T>();
        if (component == null)
        {
            Debug.LogError("No component of type " + typeof(T) + " found");
        }
    }

    private void Start()
    {
        uiManager.UpdateScore(0);
    }

    public void GameOver()
    {
        uiManager.SetRestart();
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddScore(int score)
    {
        currentScore += score;
        uiManager.UpdateScore(currentScore);
    }
    
}
