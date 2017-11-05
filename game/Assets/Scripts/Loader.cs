using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {
	public GameObject gameManager;

	void Awake () {
		if (GameManager.instancia == null){
			Instantiate(gameManager);
		}
	}
}
