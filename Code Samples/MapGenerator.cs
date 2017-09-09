/*class for generating the chess board
copywrite Greg Ostroy*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour 
{
	// constants for the dimesions of each hex so they can be properly spaced
	const float X_DIMESION=18.2f;
	const float Z_DIMENSION=16f;
	//variables
	public GameObject hex;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	//creates a new board set up for player piece selection
	public void generateNewBoard()
	{
		Color player1Color;
		Color player2Color;
		if (GameState.Instance.getPlayer (true).getColor () == Color.white) 
		{
			player1Color = Color.white;
			player2Color = new Color(.25f,.25f,.25f,1f);
		} 
		else 
		{
			player1Color = new Color(.25f,.25f,.25f,1f);;
			player2Color = Color.white;
		}
		for (int column=0; column<8; column++) 
		{
			float incrementX=((X_DIMESION*3)/4) *column;
			float offset=0;
			if(column%2!=0)
			{
				offset=Z_DIMENSION/2;
			}
			for(int row=0;row<8;row++)
			{
				float incrementZ=Z_DIMENSION*row;
				Color hexColor=Color.black;
				if(row<2)
					hexColor=player1Color;
				if(row>5)
					hexColor=player2Color;
				
				GameObject hexClone=(GameObject)Instantiate(hex,new Vector3(incrementX,0,incrementZ+offset),Quaternion.AngleAxis(-90,Vector3.right));
				hexClone.GetComponent<Renderer>().material.SetColor("_Color",hexColor);
				hexClone.GetComponent<HexScript>().setPosition(row,column);
				GameState.Instance.Board[row,column]=hexClone; 
			}
		}
		foreach (GameObject hex in GameState.Instance.Board)
						hex.GetComponent<HexScript> ().setNeighbors ();
	}
	//generate a board for saved game
	public void reloadBoard()
	{
		for (int column=0; column<8; column++) 
		{
			float incrementX=((X_DIMESION*3)/4) *column;
			float offset=0;
			if(column%2!=0)
			{
				offset=Z_DIMENSION/2;
			}
			for(int row=0;row<8;row++)
			{
				float incrementZ=Z_DIMENSION*row;
				Color hexColor=Color.black;
				
				GameObject hexClone=(GameObject)Instantiate(hex,new Vector3(incrementX,0,incrementZ+offset),Quaternion.AngleAxis(-90,Vector3.right));
				hexClone.GetComponent<Renderer>().material.SetColor("_Color",hexColor);
				hexClone.GetComponent<HexScript>().setPosition(row,column);
				GameState.Instance.Board[row,column]=hexClone; 
			}
		}
		foreach (GameObject hex in GameState.Instance.Board)
			hex.GetComponent<HexScript> ().setNeighbors ();
	}
}
