using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class BoardManager : MonoBehaviour {
	public GameObject sortides;
	public GameObject jugador;
	public GameObject paretOutH;
	public GameObject paretOutV;
	public GameObject paretIn;
	public GameObject[] monedes;

	private Transform boardHolder;
	private List<List<Vector2>> taulell = null;


	public int nFiles;
	public int nColumnes;

	private void iniciar()
	{
		if (taulell == null)
		{
			taulell = new List<List<Vector2>>(nFiles);
		}
		taulell.Clear();
		/*for (int i = 1; i < nFiles - 1; i++){
			taulell.
		}*/
	}

	public void carregar(string fitxer)
	{

		using (StreamReader stream = new StreamReader(fitxer, Encoding.Default))
		{
			string linia = null;
			while ((linia = stream.ReadLine()) != null)
			{

			}
		}
	}
}
