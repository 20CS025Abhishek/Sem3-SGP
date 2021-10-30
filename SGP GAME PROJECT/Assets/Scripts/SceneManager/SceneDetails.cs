﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{

    [SerializeField] List<SceneDetails> connectedScenes;

    public bool IsLoaded{get; private set; }
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.tag == "Player")
        {
            Debug.Log($"Entered {gameObject.name}");
            
            LoadScene();
            //To set the current scene in GameController Script
            GameController.Instance.SetCurrentScene(this);

            //Loading all connected Scenes to prevent the black screen
            foreach(var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            //Unloading the scenes that are no longer connected
            if(GameController.Instance.PrevScene != null)
            {
                var previouslyLoadedScenes = GameController.Instance.PrevScene.connectedScenes;
                foreach (var scene in previouslyLoadedScenes)
                {
                    if(!connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnloadScene();
                    }
                }
            }
        }
    }

    //Dynamically loads different Scene in additive format
    public void LoadScene()
    {
        if(!IsLoaded)
            {
                SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
                IsLoaded = true;
            }
    }

     public void UnloadScene()
    {
        if(IsLoaded)
            {
                SceneManager.UnloadSceneAsync(gameObject.name);
                IsLoaded = false;
            }
    }
}
