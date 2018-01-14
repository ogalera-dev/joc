using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //<Per carregar nous nivells un cop l'usuari arriba al final.

public class Player : MovingObject {

	private Animator animator;
	private int nMonedes; //<Nombre de monedes a agafar en el nivell actual.
	private int restartLevelDelay = 1; //<Nombre de segons que han de passar per canviar de nivell un cop el jugador arriba a la sorida.

	//Es defineix el comportament del jugador al iniciar.
	protected override void Start () {
		animator = GetComponent<Animator>();
		base.Start();
	}

	// Update is called once per frame
	void Update () {
		//No podem moure el jugador fins que no ha acabat el setup del taulell.
		if (GameManager.instancia.IsDoingSetup()) {
			return;
		}
		int horizontal = 0;
		int vertical = 0;

		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER

		horizontal = (int)Input.GetAxisRaw("Horizontal");
		vertical = (int)Input.GetAxisRaw("Vertical");

		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			var posVec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			var x = Mathf.RoundToInt(posVec.x) - transform.position.x;
			var y = Mathf.RoundToInt(posVec.y) - transform.position.y;
			if (Mathf.Abs(x) > Mathf.Abs(y))
			{
				horizontal = x > 0 ? 1 : -1;
			}
			else 
			{
				vertical = y > 0 ? 1 : -1;
			}
		}

#else
		if (Input.touchCount > 0)
		{
			Touch myTouch = Input.touches[0];

			if (myTouch.phase == TouchPhase.Began)
			{
				touchOrigin = myTouch.position;
			} else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
			{
				Vector2 touchEnd = myTouch.position;
				float x = touchEnd.x - touchOrigin.x;
				float y = touchEnd.y - touchOrigin.y;
				touchOrigin.x = -1;
				if (Mathf.Abs(x) > Mathf.Abs(y))
					horizontal = x > 0 ? 1 : -1;
				else
					vertical = y > 0 ? 1 : -1;
			}
		}

#endif
		if (horizontal != 0)
			vertical = 0;

		if (horizontal != 0)
		{
			animator.SetTrigger(horizontal > 0 ? "GirDre" : "GirEsq");
		}
		else if (vertical != 0) { 
			animator.SetTrigger(vertical > 0 ? "GirDar" : "GirDav");
		}
		Debug.Log(vertical + ", " + horizontal);
		if (horizontal != 0 || vertical != 0)
			AttemptMove<Coin>(horizontal, vertical);

	}

	protected override void AttemptMove<T>(int xDir, int yDir){
		base.AttemptMove<T>(xDir, yDir);
		RaycastHit2D hit;
		if (Move(xDir, yDir, out hit)){
			//SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
		}
		CheckIfGameOver();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		//S'ha col·lisionat amb un objecte, és una moneda? és la sortida?
		if (other.tag == "Coin"){
			//És una moneda, la computem i la deshabilitem.
			GameManager.instancia.AgafarMoneda(other);
		}
		if (other.tag == "Number") {
			GameObject numero = other.gameObject;
			if (Input.GetKeyDown("space")) {
				Debug.Log("Transportar");
			}
		}
		else if (other.tag == "Exit"){
			Invoke("Restart", restartLevelDelay); //El restartLevelDelay indica quants segons s'esperarà abans de començar la següent pantalla.
			enabled = false;
		}
	}

	protected override void OnCantMove<T>(T component){
		//Es pot moure el jugador? Si de no ser que hi hagui una pared.


	}

	private void Restart() {
		//Funció que es cridarà quan el jugador arribi a la sortida per anar al següent nivell.
		SceneManager.LoadScene(0);
	}

	private void CheckIfGameOver() { 
		//Definir quan morirà el jugador (Al acabar el temps).
	}
}
