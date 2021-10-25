using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    [SerializeField] private bool Item1;
    [SerializeField] private bool Item2;
    [SerializeField] private bool Item3;
    public static LevelManager instance;

    public Transform respawnPoint;
    public GameObject playerPrefab;
    public GameObject item;
    public GameObject item2;
    public GameObject item3;


    private void Awake() {
        instance = this;
    }

    public void Respawn() {
        Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity);
        if (Item1) {
            item.SetActive(true);
        }
        if (Item2) {
            item2.SetActive(true);
        }
        if (Item3) {
            item3.SetActive(true);
        }
    }
}