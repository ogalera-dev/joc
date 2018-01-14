using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief Classe per tots els components que es mouen en el taulell
 */
public abstract class MovingObject : MonoBehaviour {

	public float moveTime = 0.1f;
	public LayerMask blockingLayer;

	private BoxCollider2D boxCollider; //<BoxCollider de l'objecte que es mou.
	private Rigidbody2D rb2D;//<RigidBody de l'objecte que es mou
	private float inverseMoveTime;

	// Use this for initialization
	protected virtual void Start()
	{
		boxCollider = GetComponent<BoxCollider2D>();
		rb2D = GetComponent<Rigidbody2D>();
		inverseMoveTime = 1f / moveTime;
	}

	/**
	 * @post Donada una x, una y i un objecte hit, diu si ens podem moure en aquesta
	 * direcció i carrega l'objecte amb el que estem topant sobre hit.
	 */
	protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
	{
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2(xDir, yDir);

		boxCollider.enabled = false;
		hit = Physics2D.Linecast(start, end, blockingLayer);
		boxCollider.enabled = true;

		if (hit.transform == null)
		{
			StartCoroutine(SmoothMovement(end));
			return true;
		}

		return false;
	}

	protected IEnumerator SmoothMovement(Vector3 end)
	{
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		while (sqrRemainingDistance > float.Epsilon)
		{
			Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
			rb2D.MovePosition(newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}
	}

	protected virtual void AttemptMove<T>(int xDir, int yDir) where T: Component{
		//Mirem si ens podem moure en la direcció especificada.
		RaycastHit2D hit;
		bool canMove = Move(xDir, yDir, out hit);

		if (hit.transform == null) {
			//No hi ha res en el camí, ens podem moure.
			return;
		}

		//Hi ha un objecte en el camí, el podem travessar?
		//Aquest objecte o bé és una moneda (podem travesar o bé és una pared (no podem travesar).
		T hitComponent = hit.transform.GetComponent<T>();
		if (!canMove && hitComponent != null){
			OnCantMove(hitComponent);
		}
	}

	/**
	 * @brief L'objecte que hereti, decidirà si es pot moure o no sobre el component
	 * especificat en el paràmetre.
	 */
	protected abstract void OnCantMove<T>(T component) where T : Component;
}
