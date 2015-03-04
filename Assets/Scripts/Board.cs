using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class Board : MonoBehaviour
{
	public Vector3 boardSize = new Vector3(4, 4, 0);

	public Sprite corner, side, middle;
	public Sprite cross, circle;

	Piece[] pieces;
	public Piece GetPiece(int x, int y) { return pieces.FirstOrDefault(p => p.x == x && p.y == y); }

	public enum Drag { NONE, HORIZONTAL, VERTICAL, DIAGONAL_RIGHT, DIAGONAL_LEFT };
	public Drag currentDrag { get; private set; }
	public Drag lastDrag;

	// Use this for initialization
	void Start()
	{
		int gridX = Mathf.CeilToInt(Mathf.Clamp(boardSize.x, 0, boardSize.x));
		int gridY = Mathf.CeilToInt(Mathf.Clamp(boardSize.y, 0, boardSize.y));
		boardSize = new Vector3(gridX, gridY, corner.bounds.size.x);
		pieces = new Piece[gridX * gridY];

		for (int i = 0; i < gridX; ++i)
		{
			for (int j = 0; j < gridY; ++j)
			{
				GameObject go = new GameObject();
				go.transform.parent = transform;
				go.name = i.ToString() + ", " + j.ToString();

				GameObject pieceTemp = new GameObject();
				pieceTemp.transform.parent = go.transform;
				pieceTemp.name = "None";
				Piece piece = pieces[i + (gridX * j)] = pieceTemp.AddComponent<Piece>();
				piece.x = i; piece.y = j;

				GameObject spriteTemp = new GameObject();
				spriteTemp.transform.parent = go.transform;
				SpriteRenderer sr = spriteTemp.AddComponent<SpriteRenderer>();

				if (i == 0 && j == 0)
				{
					sr.sprite = corner;
				}
				else if (i == 0 && j == gridY - 1)
				{
					sr.sprite = corner;
					spriteTemp.transform.Rotate(0, 0, 90); // rotate clockwise 90
				}
				else if (i == gridX - 1 && j == 0)
				{
					sr.sprite = corner;
					spriteTemp.transform.Rotate(0, 0, -90); // rotate c-clockwise 90
				}
				else if (i == gridX - 1 && j == gridY - 1)
				{
					sr.sprite = corner;
					spriteTemp.transform.Rotate(0, 0, 180); // rotate 180
				}
				else if (i == 0)
				{
					sr.sprite = side;
				}
				else if (i == gridX - 1)
				{
					sr.sprite = side;
					spriteTemp.transform.Rotate(0, 0, 180); // rotate 180
				}
				else if (j == 0)
				{
					sr.sprite = side;
					spriteTemp.transform.Rotate(0, 0, -90); // rotate c-clockwise 90
				}
				else if (j == gridY - 1)
				{
					sr.sprite = side;
					spriteTemp.transform.Rotate(0, 0, 90); // rotate clockwise 90
				}
				else
				{
					sr.sprite = middle;
				}

				spriteTemp.name = sr.sprite.name;

				go.transform.Translate((i * corner.bounds.size.x), -(j * corner.bounds.size.y), 0);
				go.transform.Translate(-corner.bounds.size.x * (gridX - 1) / 2, corner.bounds.size.y * (gridY - 1) / 2, 0);
			}
		}

		Camera.main.orthographicSize = Mathf.Max(gridX * 5, gridY * 5);
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void Reset()
	{
		foreach (Piece p in pieces)
		{
			p.pieceType = Piece.Type.None;
			SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
			sr.material.color = Color.white;
			sr.sprite = null;
		}

		currentDrag = lastDrag = Drag.NONE;
	}

	public bool SetPiece(Player player, Piece piece)
	{
		bool addTile =	player != null &&
						player.pieceType != Piece.Type.None &&
						piece.pieceType == Piece.Type.None;
		bool removeTile = (player == null && piece.pieceType != Piece.Type.None) ||
							(player != null && piece.pieceType == player.pieceType);

		if (addTile)
		{
			piece.name = player.pieceType.ToString();
			piece.pieceType = player.pieceType;
		}
		else if (removeTile)
		{
			piece.name = "None";
			piece.pieceType = Piece.Type.None;
		}

		if (addTile || removeTile)
		{
			SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
			sr.sprite = (removeTile) ? null : (player.pieceType == Piece.Type.Circle) ? circle : cross;
		}

		return addTile;
	}

	public void StartDrag(Piece piece, Vector2 delta)
	{
		Vector3 dragDelta;
		Piece[] arr;

		switch (currentDrag)
		{
			case Drag.NONE:
				// Check direction
				float angle = Vector2.Angle(Vector2.up, delta);
				if (angle < 5 || angle > 175)
					currentDrag = Drag.VERTICAL;
				else if (angle < 85)
					currentDrag = (delta.x > 0) ? Drag.DIAGONAL_LEFT : Drag.DIAGONAL_RIGHT;
				else if (angle < 95)
					currentDrag = Drag.HORIZONTAL;
				else if (angle < 175)
					currentDrag = (delta.x < 0) ? Drag.DIAGONAL_LEFT : Drag.DIAGONAL_RIGHT;
				print(currentDrag);
				return;
			case Drag.VERTICAL:
				dragDelta = new Vector3(0, delta.y, 0);
				arr = Array.FindAll<Piece>(pieces, p => p.x == piece.x);
				foreach (Piece p in arr)
				{
					p.transform.position += dragDelta;
					p.isDragged = true;
				}
				break;
			case Drag.HORIZONTAL:
				dragDelta = new Vector3(delta.x, 0, 0);
				arr = Array.FindAll<Piece>(pieces, p => p.y == piece.y);
				foreach (Piece p in arr)
				{
					p.transform.position += dragDelta;
					p.isDragged = true;
				}
				break;
			case Drag.DIAGONAL_RIGHT:
				if (piece.x != piece.y) return;
				dragDelta = new Vector3(-delta.y, delta.y, 0);
				arr = Array.FindAll<Piece>(pieces, p => p.x == p.y);
				foreach (Piece p in arr)
				{
					p.transform.position += dragDelta;
					p.isDragged = true;
				}
				break;
			case Drag.DIAGONAL_LEFT:
				if (piece.x != boardSize.y - piece.y - 1) return;
				dragDelta = new Vector3(delta.y, delta.y, 0);
				arr = Array.FindAll<Piece>(pieces, p => p.x == boardSize.y - 1 - p.y);
				foreach (Piece p in arr)
				{
					p.transform.position += dragDelta;
					p.isDragged = true;
				}
				break;
			default:
				Debug.LogError("Invalid drag type");
				break;
		}
	}

	public Piece[] StopDrag(Piece piece, Drag? setDrag = null)
	{
		Piece[] arr = null;
		bool havePiecesMoved = false;

		Action<Piece,int,int> ChangePiece = ((_p, _newx, _newy) =>
		{
			if (_p == null)
				Debug.LogError("_p is null");
			if (_newx < 0 || _newy < 0 || _newx >= boardSize.x || _newy >= boardSize.y)
				Debug.LogError("xy board coordinates out of range");

			Transform _newParent = transform.FindChild(_newx + ", " + _newy);
			_p.transform.parent = _newParent;
			_p.SetOrigin();
			_p.x = _newx; _p.y = _newy;
			havePiecesMoved = true;
		});

		switch ((setDrag == null) ? currentDrag : setDrag)
		{
			case Drag.VERTICAL:
				arr = Array.FindAll<Piece>(pieces, p => p.x == piece.x);
				foreach (Piece p in arr)
				{
					p.isDragged = false;

					Vector3 movementDelta = p.transform.position - p.origin;
					if (movementDelta.y < -boardSize.z / 2)
					{
						// drag down
						ChangePiece(p, p.x, (p.y == boardSize.y - 1) ? 0 : p.y + 1);
					}
					else if (movementDelta.y > boardSize.z / 2)
					{
						// drag up
						ChangePiece(p, p.x, (p.y == 0) ? (int)boardSize.y - 1 : p.y - 1);
					}
				}
				break;
			case Drag.HORIZONTAL:
				arr = Array.FindAll<Piece>(pieces, p => p.y == piece.y);
				foreach (Piece p in arr)
				{
					p.isDragged = false;

					Vector3 movementDelta = p.transform.position - p.origin;
					if (movementDelta.x < -boardSize.z / 2)
					{
						// drag left
						ChangePiece(p, (p.x == 0) ? (int)boardSize.x - 1 : p.x - 1, p.y);
					}
					else if (movementDelta.x > boardSize.z / 2)
					{
						// drag right
						ChangePiece(p, (p.x == boardSize.x - 1) ? 0 : p.x + 1, p.y);
					}
				}
				break;
			case Drag.DIAGONAL_RIGHT:
				arr = Array.FindAll<Piece>(pieces, p => p.x == p.y);
				foreach (Piece p in arr)
				{
					p.isDragged = false;

					Vector3 movementDelta = p.transform.position - p.origin;
					if (movementDelta.x < -boardSize.z / 2)
					{
						// drag to left up
						ChangePiece(p, (p.x == 0) ? (int)boardSize.x - 1 : p.x - 1, (p.y == 0) ? (int)boardSize.y - 1 : p.y - 1);
					}
					else if (movementDelta.x > boardSize.z / 2)
					{
						// drag to right down
						ChangePiece(p, (p.x == boardSize.x - 1) ? 0 : p.x + 1, (p.y == boardSize.y - 1) ? 0 : p.y + 1);
					}
				}
				break;
			case Drag.DIAGONAL_LEFT:
				arr = Array.FindAll<Piece>(pieces, p => p.x == boardSize.y - 1 - p.y);
				foreach (Piece p in arr)
				{
					p.isDragged = false;

					Vector3 movementDelta = p.transform.position - p.origin;
					if (movementDelta.x < -boardSize.z / 2)
					{
						// drag to left down
						ChangePiece(p, (p.x == 0) ? (int)boardSize.x - 1 : p.x - 1, (p.y == boardSize.y - 1) ? 0 : p.y + 1);
					}
					else if (movementDelta.x > boardSize.z / 2)
					{
						// drag to right up
						ChangePiece(p, (p.x == boardSize.x - 1) ? 0 : p.x + 1, (p.y == 0) ? (int)boardSize.y - 1 : p.y - 1);
					}
				}
				break;
			default:
				return null;
		}

		lastDrag = (setDrag == null) ? currentDrag : (Drag)setDrag;
		if (setDrag == null)
		{
			currentDrag = Drag.NONE;
		}

		return (havePiecesMoved) ? arr : null;
	}

	public Piece[] CheckWin()
	{
		Piece[] winPieces = new Piece[(int)boardSize.x];
		bool hasWin = false;
		Piece.Type prevType = Piece.Type.None;

		// Horizontal tiles
		for (int i = 0; i < boardSize.x; ++i)
		{
			prevType = Piece.Type.None;

			for (int j = 0; j < boardSize.y; ++j)
			{
				if (j == 0)
				{
					prevType = GetPiece(i, j).pieceType;
				}
				else if (GetPiece(i, j).pieceType == prevType &&
					prevType != Piece.Type.None)
				{
					hasWin = true;
				}
				else
				{
					hasWin = false;
					break;
				}

				winPieces[j] = GetPiece(i, j);
			}

			if (hasWin) return winPieces;
		}

		// Vertical tiles
		for (int j = 0; j < boardSize.y; ++j)
		{
			prevType = Piece.Type.None;

			for (int i = 0; i < boardSize.x; ++i)
			{
				if (i == 0)
				{
					prevType = GetPiece(i, j).pieceType;
				}
				else if (GetPiece(i, j).pieceType == prevType &&
					prevType != Piece.Type.None)
				{
					hasWin = true;
				}
				else
				{
					hasWin = false;
					break;
				}

				winPieces[i] = GetPiece(i, j);
			}

			if (hasWin) return winPieces;
		}

		// Diagonal right
		prevType = Piece.Type.None;
		for (int i = 0; i < boardSize.x; ++i)
		{
			if (i == 0)
			{
				prevType = GetPiece(i, i).pieceType;
			}
			else if (GetPiece(i, i).pieceType == prevType &&
					prevType != Piece.Type.None)
			{
				hasWin = true;
			}
			else
			{
				hasWin = false;
				break;
			}

			winPieces[i] = GetPiece(i, i);
		}

		if (hasWin) return winPieces;

		// Diagonal left
		prevType = Piece.Type.None;
		for (int i = 0; i < boardSize.x; ++i)
		{
			if (i == 0)
			{
				prevType = GetPiece(i, (int)boardSize.x - i - 1).pieceType;
			}
			else if (GetPiece(i, (int)boardSize.x - i - 1).pieceType == prevType &&
					prevType != Piece.Type.None)
			{
				hasWin = true;
			}
			else
			{
				hasWin = false;
				break;
			}

			winPieces[i] = GetPiece(i, (int)boardSize.x - i - 1);
		}

		if (hasWin) return winPieces;
		else return null;
	}
}
