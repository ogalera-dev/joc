using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {
	public int valor = 1;

	private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Iniciar () {
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public int agafar() {
		gameObject.SetActive(false);
		return valor;
	}
}
