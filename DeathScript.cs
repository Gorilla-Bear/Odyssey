using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScript : MonoBehaviour {
	private bool hasEntered;
	private double timeTracker;

	private void OnTriggerStay2D(Collider2D collision) {
		if (hasEntered) {
			if (collision.gameObject.CompareTag("Death") && (Time.time - timeTracker >= 0.01)) {
			Destroy(gameObject);
            LevelManager.instance.Respawn();
			}
		} else {
			hasEntered = true;
			timeTracker = Time.time;
		}
		
	}
}
