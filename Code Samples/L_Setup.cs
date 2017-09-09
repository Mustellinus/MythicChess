/*Lesson on how to set up the chess board
copywrite Greg Ostroy*/
using UnityEngine;
using System.Collections;

public class L_Setup : Lesson {

	public L_Setup(TutorialGUI host, string id):base(host,id)
	{
		createText ();
	}

	void createText()
	{
		_dialogEntries.Add ("Let's set up your pieces. Your AI opponents pieces are already set up on the far side of the board. Note that the" +
			" hexes are the far side of the board are gray. That's your opponents team color. The hexes on your side of the board are white, " +
		    "your team color.");
		_dialogEntries.Add ("During the game, a hex with a piece on it will bear that pieces team color. Open hexes will be black. During an " +
			"actual game you will be allowed to choose which color is yours.");
		_dialogEntries.Add ("Select one of your opponents pieces by hovering the mouse over it and left clicking. Information about that piece " +
			"can now be seen in the panel to the left. This information may be useful when deciding what peices to choose for your side.");
		_dialogEntries.Add ("Your side of the board is empty for now, except for the king. Note that kings have crowns over their heads. Select the " +
			"king. The panel to left now displays information about that king. It will display information for any selected piece.");
		_dialogEntries.Add ("At the top of the information panel are two arrow buttons. These can be used to switch between different king types." +
			" You can also cycle through all the available types by right clicking. Exam the different king types. When you decide on a type. " +
			"press 'next'.");
		_dialogEntries.Add ("Select one of the empty white hexes. A chess piece will appear and its information will be in the panel to the left. " +
			"You can switch between different types the same way you did for the king. There are two different sets of pieces. Ones for the back" +
			" row and ones for the front row.");
		_dialogEntries.Add ("Choose pieces for all the remaining open white hexes. Notice that when you swich between types for each new hex, the types" +
			"you have laready chosen are no longer in the set. You can only choose one of each type per game.");
		_dialogEntries.Add ("Your choice of pieces matters, because some pieces are better to use against other sepcific pieces. For eample the Phoenix " +
			"can resurrect when it dies, but not if it is petrified by the Medusa or cursed by the Mummy.");
		_dialogEntries.Add ("You can select any previously chosen pieces and switch the chosen type with one of the remaining unchosen types. When " +
			"you have chosen pieces for each white hex, press the 'Play Game' button in the left panel.");
	}

	public override void onSelected()
	{
		base.onSelected ();
		clearBoard ();
		_gui.ChosenPieces.Clear ();
		foreach(GameObject hex in GameState.Instance.Board)
		{
			HexScript script=hex.GetComponent<HexScript>();
			if(script._row<2)
				script.GetComponent<Renderer>().material.SetColor("_Color",GameState.Instance.getPlayer(true).getColor());
			else if(script._row>5)
				script.GetComponent<Renderer>().material.SetColor("_Color",GameState.Instance.AIPlayer.getColor());
			else
				script.setColor();
		}
		_gui.generatePlayer2Pieces ();
		_gui.gameObject.GetComponent<TutorialSelection>().enabled=true;
		_gui.gameObject.GetComponent<TutorialSelection> ().initializePlayer1 ();
	}
	
	public override void onClosed()
	{
		if(_gui.gameObject.GetComponent<TutorialSelection>().playerHasAFullSet())
		{
			TutorialSelection.loadPlayerPieces ();
			_gui.copyPieces ();
		}
		_gui.gameObject.GetComponent<TutorialSelection>().enabled=false;
		_gui.resetLists ();
	}
}
