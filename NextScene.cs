using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour  {
    private int nextSceneToLoad;

    private void Start() {
        nextSceneToLoad = SceneManager.GetActiveScene().buildIndex + 1; 
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        SceneManager.LoadScene(nextSceneToLoad);
    }

}
