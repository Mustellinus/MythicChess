/*A centaur game piece. Features include ranged attack and an option to move one hex when attacking
copywrite  Greg Ostroy*/

using UnityEngine;
using System.Collections;

public class Centaur : ChessPieceBehaviour {

	bool _attacked=false;

	void Awake()
	{
		anim = this.gameObject.GetComponent<Animator>();
		_currentHP = _hitPoints;
	}
	// Use this for initialization
	void Start () {
		_moveDescription = "Move up to 2 hexes in line of sight";
		string lowDamage = _attackMultiplier.ToString();
		string highDamage = (_attackMultiplier * 5).ToString ();
		_attackDescription = "Attack for "+lowDamage+" - "+highDamage+" damage, up to 2 hexes away";
		_specialDescription = "Can move one additional hex after an attack";
		_pieceDescription="The centaur can attack at range and can move one hex after attacking";

		calculatePieceValue ();
	}
	
	// Update is called once per frame
	void Update () {
		if(GameState.Instance.ReadyToPlay == true)
			base.childUpdate ();
	}

	public override IEnumerator Attack(GameObject target)
	{
		gameObject.transform.LookAt (target.transform.position);
		yield return null;//wait until piece is facing target to do attack animation
		anim.Play ("Attack");
		_soundEffect.PlayOneShot (_AttackSound,.75f);
		yield return new WaitForSeconds (.5f);//make sure attack animation preceeds target's take damage animations
		int damage = 1;
		if (!containsModifier (Modifier.ModifierType.MUMMIES_CURSE))
			damage = Random.Range (1, 5) * _attackMultiplier;
		target.gameObject.transform.LookAt (transform.position);
		yield return null;
		target.GetComponent<ChessPieceBehaviour> ().takeDamage (damage,this);
		GameState.Instance.GUI.GetComponent<GameplayGUI> ().CanAttack = false;
		if(GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState==GameplayGUI.PickingStates.PICK_MODE)
		{
			_maxMove = 1;//centaur can move one hex after attacking
			if(_AI_brain==null)//object is controlled by a human
			{
				GameState.Instance.MouseEnabled=true;
				_attacked=true;
				GameState.Instance.GUI.GetComponent<GameplayGUI>().restoreHexColors();
				Cursor.SetCursor(GameState.Instance.GUI.GetComponent<GameplayGUI>()._moveTexture,Vector2.zero,CursorMode.Auto);
				GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState=GameplayGUI.PickingStates.MOVE_MODE;
				GameState.Instance.GUI.GetComponent<GameplayGUI>().lightUpMoveHexes();
			}
			else//have the AI centaur execute its secondary move action if one is appropriate
			{
				if(_AI_brain.rankMoves()>0)
				{
					_attacked=true;
					yield return new WaitForSeconds (1f);//let the attack animation finish
					yield return StartCoroutine(Move (_AI_brain.BestDestination));
				}
				else
					GameState.Instance.AIPlayer.doSecondAction();
			}
		}			
	}

	public override IEnumerator Move (GameObject hex)
	{
		Vector3 pos=hex.transform.Find("PieceNode").position;
		Vector3 destination=new Vector3(pos.x,pos.y,pos.z);
		gameObject.transform.LookAt (destination);			
		_oldPosition = new Vector3 (transform.position.x,transform.position.y,transform.position.z);
		for(float time=0;time<2f;time+=Time.deltaTime)
		{
			GameState.Instance.MouseEnabled=false;
			transform.position=Vector3.Lerp(_oldPosition,destination,time);
			anim.SetFloat("Time",time);
			_soundEffect.loop = true;
			_soundEffect.PlayOneShot (_moveSound,.75f);
			if(time>=1f)
			{
				if(containsModifier(Modifier.ModifierType.FLYING))//lower flying pieces back to the board
					destination=new Vector3(destination.x,destination.y-18f,destination.z);
				transform.position=destination;
				GameState.Instance.GUI.GetComponent<GameplayGUI>().ResetOccupants(this.gameObject,hex);
				_soundEffect.Stop();
				_soundEffect.loop = false;
				time=2f;
			}
			yield return null;
		}
		anim.SetFloat ("Time",1f);
		updateTargets();
		checkThreats();
		if(_attacked==false)
			GameState.Instance.GUI.GetComponent<GameplayGUI> ().CanMove = false;
		else
		{
			_attacked=false;
			_maxMove = 2;
		}
		GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentPlayersPiece=true;
		if(_AI_brain==null)//object is controlled by a human
		{
			GameState.Instance.MouseEnabled=true;
			GameState.Instance.GUI.GetComponent<GameplayGUI>().checkEndTurn();
		}			
		else//let the AI player do a second action if it has one
			GameState.Instance.AIPlayer.doSecondAction();
	}

	public override void addAI()
	{
		gameObject.AddComponent<CentaurBrain>();
		_AI_brain = gameObject.GetComponent<PieceBrain> ();
	}
}
