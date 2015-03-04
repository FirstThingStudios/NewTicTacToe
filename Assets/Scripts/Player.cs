using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
	static Game game;

	public Piece.Type pieceType;
	public Color playerColor = Color.red;
	public int playerIndex;

	// Use this for initialization
	void Start()
	{
		if (game == null) game = FindObjectOfType<Game>();
	}

	// Update is called once per frame
	void Update()
	{

	}
}
