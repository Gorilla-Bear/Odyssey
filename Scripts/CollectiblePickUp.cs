using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectiblePickUp : MonoBehaviour {
    IEnumerator ItemTimeRespawn() {
        Debug.Log(Time.time);
        yield return new WaitForSeconds(2f);
        Debug.Log(Time.time);
    }

    private void OnTriggerEnter2D(Collider2D info) {
        if (info.tag == "Player") {
            // set time of object respawn to 4 seconds
            StartCoroutine(ItemTimeRespawn());
            // Remove Object
            //gameObject.SetActive(false);
            // Dash Reset
            NewPlayer playerNew = info.gameObject.GetComponent<NewPlayer>();
            if (playerNew != null) {
                playerNew.DashReset();
            }
        }
    } 
}
