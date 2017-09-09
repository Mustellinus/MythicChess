/*Class used by the AI player to control the centaur
copywrite  Greg Ostroy*/

using UnityEngine;
using System.Collections;

public class CentaurBrain : PieceBrain  {

	// Use this for initialization
	void Start () {
		pieceScript=gameObject.GetComponent<ChessPieceBehaviour>();
		pieceScript.AIBrain = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override float rankMoves()
	{
		if(pieceScript._maxMove==1)
		{
			_bestDestination = pieceScript.HomeHex;//clear previous choice
			float bestMoveValue = rankDestination(pieceScript.HomeHex);
			foreach(GameObject hex in GameState.Instance.Board)
			{
				if(!hex.GetComponent<HexScript>().hasOccupant() && pieceScript.HomeHex.GetComponent<HexScript>().canMoveToHex(hex))
				{
					float hexValue=rankDestination(hex);
					if(bestMoveValue<hexValue)
					{
						bestMoveValue=hexValue;
						_bestDestination= hex;
					}
					else if(bestMoveValue==hexValue )//if values are equal 50% chance of replacement
					{
						float rand=Random.Range(0,1f);
						if(rand<.5f)
						{
							bestMoveValue=hexValue;
							_bestDestination= hex;
						}
					}
				}
			}
			if(_bestDestination = pieceScript.HomeHex)
				return 0;
			return bestMoveValue;
		}
		return base.rankMoves ();
	}

	public override float rankDestination(GameObject hex)
	{
		if(pieceScript._maxMove==1)//centaur is moving after an attack
		{
			return rankSecondaryDestination(hex);
		}
		return base.rankDestination (hex);
	}
	//after centaur has attacked it should retreat to the safest hex
	float rankSecondaryDestination(GameObject hex)
	{
		float distanceFromThreat=distanceToClosestThreat(hex);
		float threatValue=pieceScript.rankThreat(_closestThreat.GetComponent<ChessPieceBehaviour>())/pieceScript.CurrentHP;
		threatValue/=distanceFromThreat;

		return threatValue;
	}
}
