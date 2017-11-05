using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour {
	public GameObject[] goSortides;
	public GameObject[] goTerres;
	public GameObject goTerra;
	public GameObject goParetOutH;
	public GameObject goParetOutV;
	public GameObject goParetInH;
	public GameObject goParetInV;
	public GameObject goMoneda;

	private static readonly int PARED_IN_H = -2;
	private static readonly int PARED_IN_V = -1;

	private Transform boardHolder;

	private Taulell taulell;

	#region Classes
	public class BoardManagerException : Exception {		private String missatge;

		public BoardManagerException(String missatge) {			this.missatge = missatge;
		}

		public string getMissatge() {
			return missatge;
		}
	}

	public class FitxerFormatException : Exception {
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

	private class Taulell :IEnumerable<Casella>{
		public readonly int nivell;
		public readonly List<List<Casella>> taulell = null;
		public readonly int nFiles;
		public readonly int nColumnes;
		public readonly Jugador jugador;

		public Taulell(int nivell, int nFiles, int nColumnes, List<List<Casella>> taulell, Jugador jugador) {
			this.nivell = nivell;
			this.taulell = taulell;
			this.nFiles = nFiles;
			this.nColumnes = nColumnes;
			this.jugador = jugador;
		}

		public Taulell validar() {
			int nSortides = 0;			foreach (Casella casella in this) {
				if (!casella.getPosicio().valida(nFiles, nColumnes)) {
					Debug.Log("Casella " + casella.getPosicio() + ", no valida, limits " + nFiles + ", " + nColumnes);
					throw new BoardManagerException("La posicio " + casella.getPosicio() + " no és valida dins d'un taulell de " + nFiles + " x " + nColumnes);
				}
				if (casella.getElement() == Element.SORTIDA) {
					nSortides++;
				}
			}

			if (nSortides != 1) {
				Debug.Log("Nombre de sortides incorrecte, n'hi ha " + nSortides);
				throw new BoardManagerException("Nombre de sortides incorrecte, n'hi ha " + nSortides);
			}

			if (!jugador.posicio.valida(nFiles, nColumnes)) {
				Debug.Log("La posicio del jugador és incorrecte " + jugador.posicio + " en un taulell de " + nFiles + " x " + nColumnes);
				throw new BoardManagerException("La posicio del jugador és incorrecte " + jugador.posicio + " en un taulell de " + nFiles + " x " + nColumnes);
			}
			return this;
		}

		public IEnumerator<BoardManager.Casella> GetEnumerator() {
			foreach(List<Casella> caselles in this.taulell) {
				foreach(Casella casella in caselles) {
					yield return casella;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator(){
			return GetEnumerator();
		}
	}

	private class Posicio {
		public int x;
		public int y;

		public Posicio(int y, int x) {
			this.y = y;
			this.x = x;
		}

		public bool valida(int limitY, int limitX) {
			return x >= 0 && y >= 0 && x < limitX && y < limitY;
		}

		public override string ToString(){
			return "["+y+", "+x+"]";
		}
	}

	private class Casella {
		private Posicio posicio;
		private Element element;
		private Casella parella;

		public Casella(Posicio posicio, Element element, Casella parella) {
			this.posicio = posicio;
			this.element = element;
			this.parella = parella;
		}

		public void setElement(Element element) {
			this.element = element;
		}

		public Posicio getPosicio() {
			return this.posicio;				
		}

		public Element getElement() {
			return this.element;				
		}
	}

	private enum Element{
		BUIT, PARED_IN_V, PARED_IN_H, PARED_OUT_V, PARED_OUT_H, MONEDA, SORTIDA
	}

	private class Jugador {
		public readonly Posicio posicio;

		public Jugador(Posicio posicio) {
			this.posicio = posicio;
		}
	}
	#endregion

	private void setup(Taulell taulell) {
		int iRecurs = taulell.nivell % 4;
		foreach(Casella casella in taulell){
			Posicio pos = casella.getPosicio();
			GameObject objecte = null;
			GameObject terra = Random.Range(0, 3) < 2 ? this.goTerra : this.goTerres[iRecurs];
			GameObject inst = null;
			switch (casella.getElement()){
				case Element.MONEDA:
					objecte = this.goMoneda;
					break;
				case Element.PARED_IN_V:
					objecte = this.goParetInV;
					//Per qualsevol tipus de pared, posem terra normal.
					terra = this.goTerra;
					break;
				case Element.PARED_IN_H:
					objecte = this.goParetInH;
					//Per qualsevol tipus de pared, posem terra normal.
					terra = this.goTerra;
					break;
				case Element.SORTIDA:
					//objecte = this.goSortides[iRecurs];
					//La sortida no es pintarà fins que haguem obtinut els punts necessaris.
					break;
			}
			if (objecte != null) { 
				inst = Instantiate(objecte, new Vector3(pos.x, pos.y, 0F), Quaternion.identity) as GameObject;
				inst.transform.SetParent(boardHolder);
			}
			inst = Instantiate(terra, new Vector3(pos.x, pos.y, 0F), Quaternion.identity) as GameObject;
			inst.transform.SetParent(boardHolder);
		}
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

		//Aplicar la capa d'estructura
		{
			for (int i = 0; i < nFiles; i++)
			{
				//Debug.Log("Linia: " + linies[i]);
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
					f.Add(new Casella(pos, element, parella));
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
							t[i][j].setElement(Element.SORTIDA);
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

		this.taulell = new Taulell(nivell, nFiles, nColumnes, t, jugador).validar();
		this.setup(taulell);
	}
}
