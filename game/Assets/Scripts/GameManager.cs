using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager instancia = null;
	public BoardManager boardScript;
	public TextAsset fTaulell;

	// Use this for initialization
	void Awake () {
		if (instancia == null){
			instancia = this;
		}
		else {
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);

		//Debug.Log("Awake");
		boardScript = GetComponent<BoardManager>();
		InitGame();
	}

	void InitGame() {
		string[] linies = Regex.Split(fTaulell.text, "\n");
		//Debug.Log("Iniciem, fitxer amb "+linies.Length+" linies");
		Debug.Log("Començem a carregar!");
		boardScript.carregar(1, linies);
		Debug.Log("Carregat!");
	}

	// Update is called once per frame
	void Update () {
		
	}
}
