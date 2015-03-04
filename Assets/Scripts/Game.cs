using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Move = Utility.Pair<Piece[], Board.Drag>;

public class Game : MonoBehaviour
{
	static Board board;

	[SerializeField]
	uint numPlayers;
	Player[] players;

	IEnumerator iterator;
	public Player currentPlayer { get; private set; }

	bool hasWon = false;

	[SerializeField]
	UILabel currentPlayerLabel;
	[SerializeField]
	string currentPlayerText = 
	"Current Player\nPlayer $player ($piece)";
	[SerializeField]
	UIButton undoButton;
	[SerializeField]
	UIButton confirmButton;

	Move lastMove = new Move(null, Board.Drag.NONE);
	Stack<Piece> placeStack = new Stack<Piece>();

	// Use this for initialization
	void Start()
	{
		if (board == null) board = FindObjectOfType<Board>();

		if (numPlayers < 2) numPlayers = (uint)Piece.Type.MAX;

		players = new Player[numPlayers];

		for (int i = 0; i < numPlayers; ++i)
		{
			players[i] = new GameObject().AddComponent<Player>();
			players[i].transform.parent = transform;
			players[i].pieceType = (Piece.Type)i;
			players[i].name = "Player " + i;
			players[i].playerColor = (i == 0) ? Color.red : Color.blue;
			players[i].playerIndex = i;
		}

		currentPlayer = players[0];
		iterator = players.GetEnumerator();
		iterator.MoveNext();

		currentPlayerLabel.text = currentPlayerText
			.Replace("$player", (currentPlayer.playerIndex + 1).ToString())
			.Replace("$piece", currentPlayer.pieceType.ToString());

		confirmButton.enabled = undoButton.enabled = false;
		confirmButton.SetState(UIButtonColor.State.Disabled, true);
		undoButton.SetState(UIButtonColor.State.Disabled, true);
	}

	// Update is called once per frame
	void Update()
	{
	}

	void NextPlayer()
	{
		while (!iterator.MoveNext()) iterator = players.GetEnumerator();
		currentPlayer = (Player)iterator.Current;

		currentPlayerLabel.text = currentPlayerText
			.Replace("$player", (currentPlayer.playerIndex + 1).ToString())
			.Replace("$piece", currentPlayer.pieceType.ToString());

		lastMove.first = null;
		lastMove.second = Board.Drag.NONE;
	}

	public void SetPiece(Piece piece)
	{
		if (hasWon || lastMove.second != Board.Drag.NONE || !board.SetPiece(currentPlayer, piece))
		{
			return;
		}

		if (lastMove.first != null && lastMove.second == Board.Drag.NONE &&
			lastMove.first[0] != piece)
		{
			board.SetPiece(null, lastMove.first[0]);
			placeStack.Push(lastMove.first[0]);
		}

		lastMove.first = new Piece[] { piece };
		lastMove.second = Board.Drag.NONE;

		confirmButton.enabled = undoButton.enabled = true;
		confirmButton.SetState(UIButtonColor.State.Normal, true);
		undoButton.SetState(UIButtonColor.State.Normal, true);
	}

	public void StartDrag(Piece piece, Vector2 delta)
	{
		if (!hasWon &&
			(lastMove.first == null || !undoButton.enabled))
		{
			board.StartDrag(piece, delta);
		}
	}

	public void StopDrag(Piece piece)
	{
		if (!hasWon)
		{
			Piece[] draggedPieces = board.StopDrag(piece);

			if (draggedPieces != null)
			{
				lastMove.first = draggedPieces;
				lastMove.second = board.lastDrag;

				confirmButton.enabled = undoButton.enabled = true;
				confirmButton.SetState(UIButtonColor.State.Normal, true);
				undoButton.SetState(UIButtonColor.State.Normal, true);
			}
		}
	}

	public void ConfirmMove()
	{
		Piece[] winPieces = board.CheckWin();
		if (winPieces != null)
		{
			foreach (Piece p in winPieces)
			{
				p.GetComponent<SpriteRenderer>().material.color = currentPlayer.playerColor;
			}
			hasWon = true;
		}
		else
		{
			lastMove.Set(null, Board.Drag.NONE);
			NextPlayer();
			confirmButton.enabled = undoButton.enabled = false;
			confirmButton.SetState(UIButtonColor.State.Disabled, true);
			undoButton.SetState(UIButtonColor.State.Disabled, true);
		}
	}

	public void Undo()
	{
		if (lastMove.first != null)
		{
			if (lastMove.second == Board.Drag.NONE)
			{
				board.SetPiece(null, lastMove.first[0]);

				if (placeStack.Count > 0)
				{
					lastMove.first[0] = placeStack.Pop();
					board.SetPiece(currentPlayer, lastMove.first[0]);
					return;
				}
				else
				{
					lastMove.first = null;
				}

			}
			else
			{
				foreach (Piece p in lastMove.first)
				{
					p.transform.position = p.lastOrigin;
				}

				lastMove.first = board.StopDrag(lastMove.first[0], lastMove.second);

				foreach (Piece p in lastMove.first)
				{
					p.transform.position = p.lastOrigin;
				}
			}

			undoButton.enabled = false;
			undoButton.SetState(UIButtonColor.State.Disabled, true);
		}
	}

	public void Reset()
	{
		board.Reset();
		hasWon = false;

		currentPlayer = players[0];
		iterator = players.GetEnumerator();
		iterator.MoveNext();

		currentPlayerLabel.text = currentPlayerText
			.Replace("$player", (currentPlayer.playerIndex + 1).ToString())
			.Replace("$piece", currentPlayer.pieceType.ToString());

		confirmButton.enabled = undoButton.enabled = false;
		confirmButton.SetState(UIButtonColor.State.Disabled, true);
		undoButton.SetState(UIButtonColor.State.Disabled, true);

		lastMove.Set(null, Board.Drag.NONE);
	}
}
