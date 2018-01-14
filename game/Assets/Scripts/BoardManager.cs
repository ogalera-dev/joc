using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
	public GameObject[] goSortides;
	public GameObject[] goTerres;
	public GameObject goNumero;
	public GameObject goTerra;
	public GameObject goParetOutH;
	public GameObject goParetOutV;
	public GameObject goParetIn;
	public GameObject goMoneda;
	public GameObject goJugador;

	private static readonly int PARED_IN_H = -2;
	private static readonly int PARED_IN_V = -1;

	private Transform boardHolder;

	private Taulell taulell;

	#region Classes
	public class BoardManagerException : Exception
	{		private String missatge;

		public BoardManagerException(String missatge)
		{			this.missatge = missatge;
		}

		public string getMissatge()
		{
			return missatge;
		}
	}

	public class FitxerFormatException : Exception
	{
		private String missatge;

		public FitxerFormatException(String missatge)
		{
			this.missatge = missatge;
		}

		public string getMissatge()
		{
			return missatge;
		}
	}

	private class Taulell : IEnumerable<Casella>
	{
		public readonly int nivell;
		public readonly List<List<Casella>> taulell = null;
		public readonly int nFiles;
		public readonly int nColumnes;
		public readonly Jugador jugador;
		public readonly Sortida sortida;

		public Taulell(int nivell, int nFiles, int nColumnes, List<List<Casella>> taulell, Jugador jugador, Sortida sortida)
		{
			this.nivell = nivell;
			this.taulell = taulell;
			this.nFiles = nFiles;
			this.nColumnes = nColumnes;
			this.jugador = jugador;
			this.sortida = sortida;
		}

		public Taulell validar()
		{			foreach (Casella casella in this)
			{
				if (!casella.getPosicio().valida(nFiles, nColumnes))
				{
					Debug.Log("Casella " + casella.getPosicio() + ", no valida, limits " + nFiles + ", " + nColumnes);
					throw new BoardManagerException("La posicio " + casella.getPosicio() + " no és valida dins d'un taulell de " + nFiles + " x " + nColumnes);
				}
			}

			if (!sortida.posicio.valida(nFiles, nColumnes))
			{
				Debug.Log("La posició del la sortida és incorrecte " + sortida.posicio + " en un taulell de " + nFiles + " x " + nColumnes);
				throw new BoardManagerException("La posició de la sortida és incorrecte " + sortida.posicio+ " en un taulell de " + nFiles + " x " + nColumnes);
			}

			if (!jugador.posicio.valida(nFiles, nColumnes))
			{
				Debug.Log("La posició del jugador és incorrecte " + jugador.posicio + " en un taulell de " + nFiles + " x " + nColumnes);
				throw new BoardManagerException("La posició del jugador és incorrecte " + jugador.posicio + " en un taulell de " + nFiles + " x " + nColumnes);
			}


			return this;
		}

		public IEnumerator<BoardManager.Casella> GetEnumerator()
		{
			foreach (List<Casella> caselles in this.taulell)
			{
				foreach (Casella casella in caselles)
				{
					yield return casella;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private class Posicio
	{
		public int x;
		public int y;

		public Posicio(int y, int x)
		{
			this.y = y;
			this.x = x;
		}

		public bool valida(int limitY, int limitX)
		{
			return x >= 0 && y >= 0 && x < limitX && y < limitY;
		}

		public override string ToString()
		{
			return "[" + y + ", " + x + "]";
		}
	}

	/**
	 * @brief Cada una de les caselles del taulell.
	 */
	private class Casella
	{
		private Posicio posicio; //<Posició de la casella.
		private Element element; //<Element que conté la casella.
		private Casella parella; //<Destí del salt des de aquesta casella.
		private int numero; //<Número de la casella.

		public Casella(Posicio posicio, Element element, Casella parella, int numero)
		{
			this.posicio = posicio;
			this.element = element;
			this.parella = parella;
			this.numero = numero;
		}

		public void setElement(Element element)
		{
			this.element = element;
		}

		public Posicio getPosicio()
		{
			return this.posicio;
		}

		public Element getElement()
		{
			return this.element;
		}

		public int getNumero()
		{
			return this.numero;
		}
	}


	private enum Element
	{
		BUIT, PARED_IN_V, PARED_IN_H, PARED_OUT_V, PARED_OUT_H, MONEDA
	}

	/**
	 * @brief Representació del jugador (posicio).
	 */
	private class Jugador
	{
		public readonly Posicio posicio; //<Posició del jugador.

		public Jugador(Posicio posicio) {
			this.posicio = posicio;
		}
	}

	private class Sortida {		
		public readonly Posicio posicio; //<Posició de la sortida.

		public Sortida(Posicio posicio){
			this.posicio = posicio;
		}
	}
	#endregion

	private void setup(Taulell taulell) {
		int iRecurs = taulell.nivell % 4;
		GameObject t = new GameObject("taulell");
		t.transform.SetParent(boardHolder);
		int nMonedes = 0;
		foreach(Casella casella in taulell){
			Posicio pos = casella.getPosicio();
			GameObject objecte = null; //<Objecte a instanciar: moneda, pared, sortida...
			GameObject numero = null; //<Numero de la casella.
			GameObject terra = Random.Range(0, 3) < 2 ? this.goTerra : this.goTerres[iRecurs];

			switch (casella.getElement()){
				case Element.MONEDA:
					objecte = this.goMoneda;
					numero = this.goNumero;
					//objecte.transform.SetParent(monedes.transform);
					nMonedes++;
					break;
				case Element.PARED_IN_V:
				case Element.PARED_IN_H:
					objecte = this.goParetIn;
					//Per qualsevol tipus de pared, posem terra normal.
					terra = this.goTerra;
					break;
				case Element.BUIT:
					numero = this.goNumero;
					break;
			}
			GameObject instanciaTerra = Instantiate(terra, new Vector3(pos.x * 1.28f, pos.y * 1.28f, 0F), Quaternion.identity) as GameObject;
			instanciaTerra.transform.SetParent(t.transform);
			if (objecte != null) { 
				//Hi ha objecte en la casella.
				GameObject instanciaObjecte = Instantiate(objecte, new Vector3(pos.x*1.28f, pos.y* 1.28f, 0F), Quaternion.identity) as GameObject;
				instanciaObjecte.transform.SetParent(instanciaTerra.transform);
			}
			if (numero != null) { 
				//Hi ha número en la casella.
				GameObject instanciaNumero = Instantiate(numero, new Vector3(pos.x * 1.28f, pos.y * 1.28f, 0F), Quaternion.identity) as GameObject;
				instanciaNumero.GetComponent<UnityEngine.UI.Text>().text = ""+casella.getNumero();				//El número també és fill del terra.
				instanciaNumero.transform.SetParent(instanciaTerra.transform);
			}
		}
		//Instanciar el jugador.
		GameObject instanciaJugador = Instantiate(this.goJugador, new Vector3(taulell.jugador.posicio.x, taulell.jugador.posicio.y*0.84f, 0F), Quaternion.identity) as GameObject;
		instanciaJugador.transform.SetParent(boardHolder);

		//Instanciar la sortida.
		GameObject instanciaSortida = Instantiate(this.goSortides[iRecurs], new Vector3(taulell.sortida.posicio.x * 1.28f, taulell.sortida.posicio.y * 1.28f, 0F), Quaternion.identity) as GameObject;
		instanciaSortida.transform.SetParent(boardHolder);

		GameManager.instancia.setTotalMonedes(nMonedes);
		GameManager.instancia.setSortida(instanciaSortida);
		//Fer les parets exteriors per evitar que el jugador surti de la pantalla.
		ferVoltant(taulell).transform.SetParent(boardHolder);
	}

	private GameObject ferVoltant(Taulell taulell) {		GameObject exterior = new GameObject("BordeExterior");
		int x = -1;
		int y = -1;
		//Paret exterior verticals
		for (int i = 0; i < taulell.nFiles; i++) { 
			(Instantiate(this.goParetOutH, new Vector3(-1.28f, i * 1.28f, 0F), Quaternion.identity) as GameObject).transform.SetParent(exterior.transform);
			(Instantiate(this.goParetOutH, new Vector3(taulell.nColumnes * 1.28f, i * 1.28f, 0F), Quaternion.identity) as GameObject).transform.SetParent(exterior.transform);
		}
		//Paret exterior horizontals
		for (int i = 0; i < taulell.nColumnes; i++) { 
			(Instantiate(this.goParetOutH, new Vector3(i * 1.28f, -1.28f, 0F), Quaternion.identity) as GameObject).transform.SetParent(exterior.transform);
			(Instantiate(this.goParetOutH, new Vector3(i * 1.28f, taulell.nFiles * 1.28f, 0F), Quaternion.identity) as GameObject).transform.SetParent(exterior.transform);
		}

		//Fer els 4 extrems.
		(Instantiate(this.goParetOutH, new Vector3(-1.28f, -1.28f, 0F), Quaternion.identity) as GameObject).transform.SetParent(exterior.transform);
		(Instantiate(this.goParetOutH, new Vector3(taulell.nColumnes * 1.28f, taulell.nFiles * 1.28f, 0F), Quaternion.identity) as GameObject).transform.SetParent(exterior.transform);
		(Instantiate(this.goParetOutH, new Vector3(taulell.nColumnes * 1.28f, -1.28f, 0F), Quaternion.identity) as GameObject).transform.SetParent(exterior.transform);
		(Instantiate(this.goParetOutH, new Vector3(-1.28f, taulell.nFiles * 1.28f, 0F), Quaternion.identity) as GameObject).transform.SetParent(exterior.transform);
		return exterior;
	}

	public void carregar(int nivell, string[] linies){
		int indexLinia = 0;

		//Primera línia: mida del costat del tauler.
		int nFiles = System.Int32.Parse(linies[indexLinia].Split(';')[0]);
		int nColumnes = System.Int32.Parse(linies[indexLinia].Split(';')[1]);
		//Debug.Log("Files: " + nFiles + ", columnes: " + nColumnes);
		indexLinia++;

		//Línia en blanc.
		indexLinia++;

		//Debug.Log("Començem a llegir el taulell en la linia " + indexLinia);

		Dictionary<int, Posicio> vinculador = new Dictionary<int, Posicio>();
		List<List<Casella>> t = new List<List<Casella>>();
		Jugador jugador = null;
		Sortida sortida = null;

		//Aplicar la capa d'estructura
		{
			for (int i = 0; i < nFiles; i++)
			{
				string[] sFila = linies[indexLinia++].Split(';');
				if (sFila.Length != nColumnes) {
					throw new FitxerFormatException("La fila "+i+" de l'estructura del taulell no és valida, hauria de tenir: " + nColumnes + " columnes i en té: " + sFila.Length);
				}
				List<Casella> f = new List<Casella>(nColumnes);
				t.Add(f);
				for (int j = 0; j < nColumnes; j++)
				{
					Element element = Element.BUIT;
					Casella parella = null;
					int valor = System.Int32.Parse(sFila[j]);
					Posicio pos = new Posicio(i, j);
					if (valor > 0)
					{
						//Fem el doble enllaç de la posició.
						if (!vinculador.ContainsKey(valor))
						{
							//Debug.Log("Afegim " + pos + " per vincular amb etiqueta "+valor);
							vinculador.Add(valor, pos);
						}
						else {
							Posicio pParella = vinculador[valor];
							//Debug.Log("Anem a vincular la etiqueta " + valor + ", trobat: " + pParella);
							parella = t[pParella.y][pParella.x];
							vinculador.Remove(valor);
						}
					}
					else {
						element = valor == PARED_IN_H ? Element.PARED_IN_H: Element.PARED_IN_V;
					}
					f.Add(new Casella(pos, element, parella, valor));
				}
			}
		}

		Debug.Log("Fase 1 acabada, estructura del taulell");

		//Línia en blanc.
		indexLinia++;

		//Aplicar la capa d'objectes.
		{
			for (int i = 0; i < nFiles; i++){
				string[] sFila = linies[indexLinia++].Split(';');
				if (sFila.Length != nColumnes){
					throw new FitxerFormatException("La fila "+i+" de l'estructura dels objectes del taulell no és valida, hauria de tenir: " + nColumnes + " columnes i en té: " + sFila.Length);
				}
				for (int j = 0; j < nColumnes; j++){
					//Debug.Log("Trobat: " + sFila[j]);
					switch (sFila[j]){
						case "C":
							//Coin -> Moneda
							t[i][j].setElement(Element.MONEDA);
							break;
						case "E":
							//Exit -> Sortida
							sortida = new Sortida(new Posicio(i, j));
							break;
						case "P":
							//Player -> Jugador							jugador = new Jugador(new Posicio(i, j));
							break;
					}
				}
			}

		}

		Debug.Log("Fase 2 acabada, objectes del taulell");

		if (vinculador.Keys.Count > 0) { 
			//El taulell està mal format, cal llençar excepció.
		}

		this.setup(new Taulell(nivell, nFiles, nColumnes, t, jugador, sortida).validar());
	}
}
