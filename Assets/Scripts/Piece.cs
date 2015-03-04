using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[Serializable]
public class Piece : MonoBehaviour
{
	public enum Type { Circle = 0, Cross, MAX, None = -1 };

	static Board board;
	static Game game;

	public int x, y; // piece position on the board
	public Type pieceType = Type.None;

	public bool isDragged = false;

	public Vector3 origin { get; private set; }
	public Vector3 lastOrigin { get; private set; }
	Vector3 velocity = Vector3.zero;
	Vector2 touchDelta;

	// Use this for initialization
	void Start()
	{
		if (board == null) board = FindObjectOfType<Board>();
		if (game == null) game = FindObjectOfType<Game>();

		BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
		col.size = new Vector2(board.boardSize.z, board.boardSize.z);

		transform.Translate(0, 0, -1);
		origin = transform.position;
		gameObject.layer = LayerMask.NameToLayer("Board Piece");
	}

	// Update is called once per frame
	void Update()
	{
		if (!isDragged)
		{
			if (transform.position != origin)
				transform.position = Vector3.SmoothDamp(transform.position, origin, ref velocity, 0.1f);
			else
				transform.position = origin;
		}
	}

	void OnMouseUp()
	{
		OnInputUp();
	}

	void OnMouseDrag()
	{
		Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

		if (delta != Vector2.zero)
		{
			touchDelta = delta * (Application.platform == RuntimePlatform.Android ? 0.25f : 4.0f);
			OnInputMove(touchDelta);
		}
	}

	void OnInputUp()
	{
		if (isDragged) game.StopDrag(this);
		else game.SetPiece(this);

		isDragged = false;
	}

	void OnInputMove(Vector2 delta)
	{
		isDragged = true;
		game.StartDrag(this, delta);
	}

	public void SetOrigin()
	{
		lastOrigin = origin;
		origin = transform.parent.position;
		origin.Set(origin.x, origin.y, -1);
	}
}
