/* The core class for executing tutorial lessons on playing the game
copywrite Greg Ostroy*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lesson {
	public bool taskCompleted=true;

	protected Lesson _previousLesson;
	protected Lesson _nextLesson;
	
	public string _ID;

	protected List<string> _dialogEntries;
	protected string _currentEntry;
	protected int _chapter=0;

	protected TutorialGUI _gui;

	public Lesson PreviousLesson
	{
		get
		{
			return _previousLesson;
		}
		set
		{
			_previousLesson=value;
		}
	}
	public Lesson NextLesson
	{
		get
		{
			return _nextLesson;
		}
		set
		{
			_nextLesson=value;
		}
	}
	public string CurrentEntry
	{
		get
		{
			return _currentEntry;
		}
	}
	public int Chapter
	{
		get
		{
			return _chapter;
		}
	}
	public List<string> Entries
	{
		get
		{
			return _dialogEntries;
		}
	}

	public Lesson(TutorialGUI host, string id)
	{
		_gui = host;
		_ID = id;
		_dialogEntries=new List<string>();
	}

	public virtual void update()
	{
			
	}

	public virtual void previous()
	{
		if(_chapter==0)
		{
			onClosed();
			_gui.CurrentLesson=_previousLesson;
			_gui.CurrentLesson.onSelected();
		}
		else
			_chapter--;
		_currentEntry = _dialogEntries [_chapter];
	}

	public virtual void next()
	{
		if(_chapter==_dialogEntries.Count-1)
		{
			onClosed ();
			_gui.CurrentLesson=_nextLesson;
			_gui.CurrentLesson.onSelected();
		}
		else
			_chapter++;
		_currentEntry = _dialogEntries [_chapter];
	}

	public virtual void onSelected()
	{
		_gui.resetCamera ();
		_currentEntry = _dialogEntries [0];
	}

	public virtual void onClosed()
	{
		_gui.gameObject.GetComponent<GameplayGUI> ().enabled = false;
	}

	protected void clearBoard()
	{

		if(GameState.Instance.getPlayer(true).ChessPieces.Count>0)
		{
			GameState.Instance.getPlayer(true).ChessPieces.Clear ();
			GameState.Instance.getPlayer(true).ChessPieceNames.Clear ();
		}
		if(GameState.Instance.AIPlayer.ChessPieces.Count>0)
		{
			GameState.Instance.AIPlayer.ChessPieces.Clear();
			GameState.Instance.AIPlayer.ChessPieceNames.Clear();
		}
		foreach(GameObject hex in GameState.Instance.Board)
		{
			if(hex.GetComponent<HexScript>().hasOccupant())
			{
				GameObject piece=hex.GetComponent<HexScript>().Occupant;
				hex.GetComponent<HexScript>().removeOccupant();
				GameObject.Destroy(piece);
			}
			else
				hex.GetComponent<HexScript>().setColor();
		}
	}

	protected void createPiece(string pieceName,Player owner,int row,int column)
	{
		GameObject hex = GameState.Instance.Board [row, column];
		Vector3 location = hex.transform.Find ("PieceNode").position;
		Quaternion rotation = Quaternion.identity;
		if (owner == GameState.Instance.AIPlayer)
		{
			GameState.Instance.setCurrentPlayer (false);
			rotation=Quaternion.AngleAxis (180f, Vector3.up);
		}				
		GameObject piece = (GameObject)GameObject.Instantiate (_gui.findPiece(pieceName),location,rotation);
		if (piece.GetComponent<ChessPieceBehaviour> ()._kingPiece == true)
		{
			owner.King = piece;
			piece.GetComponent<ChessPieceBehaviour>().Owner=owner;
		}				
		else
			owner.addToDictionary(piece);
		piece.GetComponent<ChessPieceBehaviour> ().initialSetup (hex.GetComponent<HexScript>());
		GameState.Instance.setCurrentPlayer (true);
	}

	protected void moveTarget(GameObject target, GameObject hex)
	{
		Vector3 location = hex.transform.Find("PieceNode").position;
		target.GetComponent<ChessPieceBehaviour> ().HomeHex.GetComponent<HexScript> ().removeOccupant ();
		target.transform.position = location;
		target.GetComponent<ChessPieceBehaviour> ().HomeHex = hex;
		hex.GetComponent<HexScript> ().setOccupant (target);
	}	
}
