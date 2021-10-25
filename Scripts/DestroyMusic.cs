using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMusic : MonoBehaviour   {
    void Awake() {
        GameObject A = GameObject.FindWithTag("BackgroundMusic");
        Destroy(A);
    }
    
}
