using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	[HideInInspector] public bool torn = true;
	public static GameManager instancia = null;
	public TextAsset fTaulell;
	private BoardManager boardScript;
	private int nMonedes = 0; //<Nombre de monedes que ha agafat el jugador.
	private int totalMonedes = 0; //<Nombre total de monedes que hi ha en el nivell.
	private GameObject goSortida;

	//Quans segons han de passar per iniciar la pantalla.
	public float retardIniciPantalla = 2f;
	public float turnDelay = .1f;

	private Text txtLevel;
	private GameObject imgNivell;
	//Per evitar que es pugui moure el personatge durant el setup.
	private bool doingSetup;
	private int level = 1;

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

	//Part de l'API Unity i es crida cada vegada que es carrega l'escena.
	private void OnLevelWasLoaded(int index) {
		level++;
		InitGame();
	}

	void InitGame() {
		doingSetup = true;

		string[] linies = Regex.Split(fTaulell.text, "\n");
		//Debug.Log("Iniciem, fitxer amb "+linies.Length+" linies");
		Debug.Log("Començem a carregar!");
		boardScript.carregar(1, linies);
		Debug.Log("Carregat!");

		//Marquem que el setup ha acabat.
		imgNivell = GameObject.Find("imgNivell");
		txtLevel = GameObject.Find("lblMonedes").GetComponent<Text>();
		txtLevel.text = "Coins: " + nMonedes;

		//En un inici, la sortida està desactivada si queden monedes per agafar.
		if (totalMonedes > nMonedes){
			this.goSortida.SetActive(false);
		}

		imgNivell.SetActive(true);
		//Al cap de retardIniciPantalla segons, amaguem l'imatge del nivell.
		Invoke("AmagarLevelImatge", retardIniciPantalla);
	}

	private void AmagarLevelImatge() {
		imgNivell.SetActive(false);
		doingSetup = false;
	}

	public void GameOver(){
		enabled = false;
	}

	public bool IsDoingSetup() {
		return this.doingSetup;
	}

	public void AgafarMoneda(Collider2D moneda) {
		this.nMonedes++;
		txtLevel.text = "Coins: " + nMonedes;
		moneda.gameObject.SetActive(false);
		if (this.nMonedes == this.totalMonedes) {
			this.goSortida.SetActive(true);
		}
	}

	public void setTotalMonedes(int totalMonedes) {
		this.totalMonedes = totalMonedes;
	}

	public void setSortida(GameObject sortida) {
		this.goSortida = sortida;
	}

}
