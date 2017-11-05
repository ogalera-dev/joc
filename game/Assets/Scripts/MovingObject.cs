using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

	public float moveTime = 0.1f;
	public LayerMask blockingLayer;

	private BoxCollider2D boxCollider;
	private Rigidbody2D rb2D;
	private float inverseMoveTime;

	protected bool Move(int yDir, int xDir, out RaycastHit2D hit){
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2(xDir, yDir);
		boxCollider.enabled = false;
		hit = Physics2D.Linecast(start, end, blockingLayer);
		if (hit.transform == null){
			StartCoroutine(SmoothMovement(end));
			return true;
		}
		return false;
	}

	protected virtual void AttemptMove<T>(int yDir, int xDir) where T: Component{
		RaycastHit2D hit;
		bool canMove = Move(xDir, yDir, out hit);
		if (hit.transform == null) {
			return;
		}
		T hitComponent = hit.transform.GetComponent<T>();
		if (!canMove && hitComponent != null){
			OnCantMove(hitComponent);
		}
	}

	// Use this for initialization
	protected virtual void Start () {
		boxCollider = GetComponent<BoxCollider2D>();
		rb2D = GetComponent<Rigidbody2D>();
		inverseMoveTime = 1f / moveTime;
	}

	protected IEnumerator SmoothMovement(Vector3 end){
		float distancia = (transform.position - end).sqrMagnitude;

		while (distancia > float.Epsilon){
			Vector3 novaPosicio = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
			rb2D.MovePosition(novaPosicio);
			distancia = (transform.position - end).sqrMagnitude;
			yield return null;
		}
	}

	protected abstract void OnCantMove<T>(T component) where T : Component;
}
