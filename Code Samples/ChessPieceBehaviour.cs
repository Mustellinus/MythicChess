/*This is the core class for controlling chess pieces, each chess piece class inheritsfrom this one
copywrite Greg Ostroy*/﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ChessPieceBehaviour : MonoBehaviour {
	#region member variables
	
	public int _hitPoints=10;
	public int _attackMultiplier=1;
	public int _maxMove=1;
	public int _attackRange=1;
	public string _name;
	public bool _kingPiece=false;

	public AudioSource _soundEffect;
	public AudioClip _moveSound;
	public AudioClip _AttackSound;
	public AudioClip _DamagedSound;
	public AudioClip _DeathSound;
	public AudioClip _guardSound;

	protected bool _AI_impaled=false;
	protected int _currentHP= 10;
	protected int _threatHash=Animator.StringToHash("Threatened");
	protected float _pieceValue;
	protected string  _moveDescription;
	protected string  _attackDescription;
	protected string  _specialDescription;//the description of a pieces special abilities in gameplay scene
	protected string _pieceDescription;//the description of a piece in the setup scene
	protected string _pieceId;
	//values for piece interaction
	protected Vector3 _destination;
	protected Vector3 _oldPosition;
	protected bool _held=false;
	protected GameObject _target;
	protected GameObject _homeHex;
	protected GameObject _topThreat;
	protected Material _OriginalMat;

	protected Animator anim;

	protected Color _teamcolor=Color.white;

	public enum behaviorStates{WAITING,THREATENED,HIDDEN};
 	protected behaviorStates _currentBehavior=behaviorStates.WAITING;

	public enum attackType{DIRECTED,AOE};
	public attackType _attackType=attackType.DIRECTED;

	public enum specialEffectType{NONE,PASSIVE,DIRECTED,AOE,SELF};
	public specialEffectType _effectType=specialEffectType.NONE;

	protected List<Modifier> _debuffs = new List<Modifier>();
	List<Modifier> _buffs = new List<Modifier>();
	protected HashSet<GameObject> _targets = new HashSet<GameObject> ();
	protected HashSet<GameObject> _threats= new HashSet<GameObject> ();

	protected PieceBrain _AI_brain;
	protected Player _owner;//the player that owns this piece
	
	protected string _specialPrompt="This piece has no special actions";
	
	#endregion
	public string PieceDescription
	{
		get
		{
			return _pieceDescription;
		}
	}

	public behaviorStates CurrentBehavior
	{
		get
		{
			return _currentBehavior;
		}
		set
		{
			_currentBehavior=value;
		}
	}

	public int ThreatHash
	{
		get
		{
			return _threatHash;
		}
	}

	public Color TeamColor
	{
		get
		{
			return _teamcolor;
		}
	}

	public GameObject HomeHex
	{
		get 
		{
			return _homeHex;
		}
		set
		{
			_homeHex=value;
		}
	}
	public string MoveDescription
	{
		get
		{
			return _moveDescription;
		}
	}
	public string AttackDescription
	{
		get
		{
			return _attackDescription;
		}
	}
	public string SpecialDescription
	{
		get
		{
			return _specialDescription;
		}
	}

	public string PieceId
	{
		get
		{
			return _pieceId;
		}
		set
		{
			_pieceId=value;
		}
	}

	public List<Modifier> Buffs
	{
		get
		{
			return _buffs;
		}
	}

	public List<Modifier> DeBuffs
	{
		get
		{
			return _debuffs;
		}
	}

	public bool Held
	{
		get
		{
			return _held;
		}
		set
		{
			_held=value;
		}
	}
	public int CurrentHP
	{
		get
		{
			return _currentHP;
		}
		set
		{
			_currentHP=value;
		}
	}
	public float PieceValue
	{
		get
		{
			return _pieceValue;
		}
	}

	public HashSet<GameObject> Targets
	{
		get
		{
			return _targets;
		}
	}

	public HashSet<GameObject> Threats
	{
		get
		{
			return _threats;
		}
	}

	public PieceBrain AIBrain
	{
		get
		{
			return _AI_brain;
		}

		set
		{
			_AI_brain=value;
		}
	}

	public Animator Anim
	{
		get
		{
			return anim;
		}
	}

	public GameObject TopThreat
	{
		get
		{
			return _topThreat;
		}
	}

	public Material OriginalMat
	{
		get 
		{
			return _OriginalMat;
		}
		set
		{
			_OriginalMat=value;
		}
	}

	public Player Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner=value;
		}
	}
	
	public virtual void specialInterface()
	{
		//any pieces that use this method will override it
	}
	
	public virtual void specialDialogBox(int winID)
	{
		GUILayout.Label (_specialPrompt);
		if(_effectType==specialEffectType.NONE || _effectType==specialEffectType.PASSIVE)
		{
			if(GUILayout.Button("Ok"))
			{
				GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentPlayersPiece = true;
				Cursor.SetCursor(GameState.Instance.GUI.GetComponent<GameplayGUI>()._pickTexture,Vector2.zero,CursorMode.Auto);
				GameState.Instance.GUI.GetComponent<GameplayGUI>().SpecialPick=false;
				GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState=GameplayGUI.PickingStates.ACTION_MODE;
				GameState.Instance.MouseEnabled=true;
			}
		}

		else
		{
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Yes"))
			{
				GameState.Instance.GUI.GetComponent<GameplayGUI>().SpecialPick=false;
				GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState=GameplayGUI.PickingStates.SPECIAL_MODE;
			}
			if(GUILayout.Button("No"))
			{
				GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentPlayersPiece = true;
				Cursor.SetCursor(GameState.Instance.GUI.GetComponent<GameplayGUI>()._pickTexture,Vector2.zero,CursorMode.Auto);
				GameState.Instance.GUI.GetComponent<GameplayGUI>().SpecialPick=false;
				GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState=GameplayGUI.PickingStates.ACTION_MODE;
				GameState.Instance.MouseEnabled=true;
			}
			GUILayout.EndHorizontal();
		}
	}
	public virtual void initialSetup(HexScript hexInfo)
	{
		_teamcolor = GameState.Instance.CurrentPlayer.getColor ();
		_homeHex = hexInfo.gameObject;
		hexInfo.setOccupant (gameObject);
	}

	protected void calculatePieceValue()
	{
		_pieceValue = (_hitPoints * _maxMove *_attackMultiplier *_attackRange*_attackRange) / 10;
		if (_attackType == attackType.AOE)
						_pieceValue *= 1.2f;
		if (_effectType != specialEffectType.NONE)
						_pieceValue *= 1.5f;
		if (_buffs.Count > 0)
						_pieceValue *= 1f + (_buffs.Count / 10);
		if (_kingPiece)
						_pieceValue *= 10f;
	}

	public virtual void childUpdate()
	{
	
	}

	public virtual void cancelAction()
	{
		Cursor.SetCursor(GameState.Instance.GUI.GetComponent<GameplayGUI>()._pickTexture,Vector2.zero,CursorMode.Auto);
		GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentPlayersPiece=true;
		GameState.Instance.GUI.GetComponent<GameplayGUI> ().SpecialPick = false;
		GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState=GameplayGUI.PickingStates.ACTION_MODE;
	}

	public virtual void mouseInput()
	{
		GameplayGUI gui=GameState.Instance.GUI.GetComponent<GameplayGUI>();
		if(gui.TargetHex!=null)
			gui.TargetHex.GetComponent<HexScript>().setColor();
		if(Input.GetMouseButtonUp(1) && GameState.Instance.MouseEnabled==true)
		{
			cancelAction ();
		}
		else
		{
			Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit,500f))
			{
				if(hit.collider.gameObject.tag=="ChessPiece")
				{
					GameObject targetPiece=hit.collider.gameObject;
					if(legitTarget(targetPiece)&& suseptableToEffect(targetPiece))
					{
						gui.TargetHex=targetPiece.GetComponent<ChessPieceBehaviour>().HomeHex;
						gui.TargetHex.gameObject.GetComponent<Renderer>().material.color=Color.green;
						if(Input.GetAxis("Select")>0 && GameState.Instance.MouseEnabled==true)
						{
							Cursor.SetCursor(GameState.Instance.GUI.GetComponent<GameplayGUI>()._pickTexture,Vector2.zero,CursorMode.Auto);
							GameState.Instance.MouseEnabled=false;
							StartCoroutine(doSpecial(targetPiece));					
						}
					}
					else if(hit.collider.gameObject.tag=="Hex")
					{
						gui.TargetHex=hit.collider.gameObject;
						gui.TargetHex.gameObject.GetComponent<Renderer>().material.color=Color.red;
					}
				}
				else if(hit.collider.gameObject.tag=="Hex")
				{
					gui.TargetHex=hit.collider.gameObject;
					gui.TargetHex.gameObject.GetComponent<Renderer>().material.color=Color.red;
				}
			}
		}
	}

	public void setTeamColor(Color color)
	{
		_teamcolor = color;
	}
	//check if potential target is on the opposite team, is within attack range and is in line of sight
	public virtual bool legitTarget(GameObject target)
	{
		if (target.GetComponent<ChessPieceBehaviour> ().CurrentBehavior == behaviorStates.HIDDEN)
						return false;
		if (target.GetComponent<ChessPieceBehaviour> ().TeamColor.Equals(_teamcolor) || 
		    target.GetComponent<ChessPieceBehaviour> ().containsModifier(Modifier.ModifierType.PETRIFIED))// same team or petrified
						return false;
		//raise the ray a liitle above the board
		Vector3 origin = new Vector3 (transform.position.x, transform.position.y + 1f, transform.position.z);
		Vector3 targetLocation= new Vector3 (target.transform.position.x, target.transform.position.y + 1f, target.transform.position.z);
		if (Vector3.Magnitude (origin-targetLocation)/16f <= _attackRange)//in range 
		{
			RaycastHit hit;
			Ray ray = new Ray (origin,targetLocation-origin);
			if(Physics.Raycast(ray,out hit,100f))
			{
				if(hit.collider.gameObject==target)//target is in line of sight
					return true;
			}
		}
		return false;
	}

	public virtual IEnumerator Move(GameObject hex)
	{
		Vector3 pos=hex.transform.Find("PieceNode").position;
		Vector3 destination=new Vector3(pos.x,pos.y,pos.z);
		if (containsModifier (Modifier.ModifierType.FLYING))//raise flying pieces above the board
		{
			transform.position = new Vector3 (transform.position.x,transform.position.y+18f,transform.position.z);
			destination=new Vector3(destination.x,destination.y+18f,destination.z);
		}
		gameObject.transform.LookAt (destination);			
		_oldPosition = new Vector3 (transform.position.x,transform.position.y,transform.position.z);
		_soundEffect.loop = true;
		_soundEffect.PlayOneShot (_moveSound,.75f);
		for(float time=0;time<2f;time+=Time.deltaTime)
		{
			GameState.Instance.MouseEnabled=false;
			transform.position=Vector3.Lerp(_oldPosition,destination,time);
			anim.SetFloat("Time",time);
			if(time>=1f)
			{
				if(containsModifier(Modifier.ModifierType.FLYING))//lower flying pieces back to the board
					destination=new Vector3(destination.x,destination.y-18f,destination.z);
				transform.position=destination;
				GameState.Instance.GUI.GetComponent<GameplayGUI>().ResetOccupants(this.gameObject,hex);
				time=2f;
				_soundEffect.Stop();
				_soundEffect.loop=false;
			}
			yield return null;
		}
		anim.SetFloat ("Time",1f);
		updateTargets();
		checkThreats();
		GameState.Instance.GUI.GetComponent<GameplayGUI> ().CanMove = false;
		GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentPlayersPiece=true;
		if(_AI_brain==null)//object is controlled by a human
		{
			GameState.Instance.MouseEnabled=true;
			GameState.Instance.GUI.GetComponent<GameplayGUI>().checkEndTurn();
		}			
		else//let the AI player do a second action if it has one
			GameState.Instance.AIPlayer.doSecondAction();
	} 

	public virtual IEnumerator Attack(GameObject target)
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
		GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentPlayersPiece=true;
		yield return new WaitForSeconds (1f);//let the attack animation finish
		if(GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState==GameplayGUI.PickingStates.PICK_MODE 
		   && gameObject.GetComponent<Collider>().enabled==true)
		{
			_owner.King.GetComponent<ChessPieceBehaviour>().checkThreats();
			foreach(string piece in _owner.ChessPieceNames)
				_owner.ChessPieces[piece].GetComponent<ChessPieceBehaviour>().checkThreats();
			if(_AI_brain==null)//object is controlled by a human
			{
				GameState.Instance.MouseEnabled=true;;
				GameState.Instance.GUI.GetComponent<GameplayGUI>().checkEndTurn();
			}
			else//let the AI player do a second action if it has one
				GameState.Instance.AIPlayer.doSecondAction();
		}
	}
	//a modified attack function for when kings are dueling, it does not include threat checks and unecessary gui changes
	public IEnumerator DuelAttack(GameObject target)
	{
		gameObject.transform.LookAt (target.transform.position);
		yield return null;//wait until piece is facing target to do attack animation
		anim.Play ("Attack");
		_soundEffect.PlayOneShot (_AttackSound,.75f);
		yield return new WaitForSeconds (.5f);//make sure attack animation preceeds target's take damage animations
		int damage = Random.Range (1, 5) * _attackMultiplier;
		target.gameObject.transform.LookAt (transform.position);
		yield return null;
		target.GetComponent<ChessPieceBehaviour> ().takeDamage (damage,this);
		yield return new WaitForSeconds (1f);//let the attack animation finish
	}

	public virtual void takeDamage(int damage,ChessPieceBehaviour attacker)
	{
		Cursor.SetCursor(GameState.Instance.GUI.GetComponent<GameplayGUI> ()._pickTexture,Vector2.zero,CursorMode.Auto);
		_currentHP -= damage;
		if (_currentHP < 1)
		{   //AIs piece was killed in an attack
			if(_owner==GameState.Instance.AIPlayer && GameState.Instance.CurrentPlayer==GameState.Instance.AIPlayer)
				_AI_impaled=true;
			death ();
			if(!attacker._name.Equals("Hydra"))//hydra makes multiple attacks and calls a victory check only once after all of them
			{
				GameState.Instance.checkForVictory();
				//if an AIs attacking piece is killed it should still do a second action if available.
				if(GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState==GameplayGUI.PickingStates.PICK_MODE && _AI_impaled == true)
				{
					_AI_impaled=false;
					GameState.Instance.AIPlayer.doSecondAction();
				}
			}	
		}
		else
		{
			anim.Play ("Damaged");
			_soundEffect.PlayOneShot(_DamagedSound,.75f);
			_currentBehavior=behaviorStates.THREATENED;
			anim.SetBool(_threatHash,true);
			//hydra makes multiple attacks and the calls a victory check which sets the gui state, and kings in duel mode should stay in that mode
			if(!attacker._name.Equals("Hydra")&& 
			   GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState!=GameplayGUI.PickingStates.DUEL_MODE)
				GameState.Instance.GUI.GetComponent<GameplayGUI>().CurrentState=GameplayGUI.PickingStates.PICK_MODE;
		}			
	}

	public virtual void death()
	{
		anim.Play("Death");
		_soundEffect.PlayOneShot (_DeathSound,.75f);
		gameObject.GetComponent<Collider>().enabled=false;
		Player opponent=null;
		if(GameState.Instance.getPlayer (true)==_owner)
			opponent=GameState.Instance.getPlayer(false);
		else
			opponent=GameState.Instance.getPlayer(true);
		opponent.King.GetComponent<ChessPieceBehaviour>().Targets.Remove(gameObject);
		opponent.King.GetComponent<ChessPieceBehaviour>().Threats.Remove(gameObject);
		if(opponent.ChessPieceNames.Count>0)
		{
			foreach(string piece in opponent.ChessPieceNames)
			{
				opponent.ChessPieces[piece].GetComponent<ChessPieceBehaviour>().Targets.Remove(gameObject);
				opponent.ChessPieces[piece].GetComponent<ChessPieceBehaviour>().Threats.Remove(gameObject);
			}
		}
		_homeHex.GetComponent<HexScript>().removeOccupant();
		if (_kingPiece == true)
			_owner.King=null;
		else
		{
			_owner.ChessPieceNames.Remove(_pieceId);
			_owner.ChessPieces.Remove(_pieceId);
		}
		Destroy (gameObject,1.5f);
	}

	public virtual IEnumerator doSpecial(GameObject target)
	{
		yield return null;
	}

	public virtual IEnumerator doSpecial()
	{
		yield return null;
	}

	public virtual void checkThreats()
	{
		float threatValue = 0;//rank threatening pieces
		_topThreat = null;
		Player opponent = null;
		if (GameState.Instance.getPlayer (true)==_owner)
						opponent = GameState.Instance.getPlayer (false);
		else
			opponent=GameState.Instance.getPlayer(true);
		if(opponent.King !=null)
		{
			if(legitTarget(opponent.King))//possible threat may also be possible target
			{
				_targets.Add(opponent.King);
				opponent.King.GetComponent<ChessPieceBehaviour>().updateThreat(gameObject);
			}
			if(opponent.King.GetComponent<ChessPieceBehaviour>().legitTarget(gameObject))
			{
				if(!opponent.King.GetComponent<ChessPieceBehaviour>().Targets.Contains(gameObject))
					opponent.King.GetComponent<ChessPieceBehaviour>().Targets.Add(gameObject);
				_threats.Add(opponent.King);
				threatValue=rankThreat(opponent.King.GetComponent<ChessPieceBehaviour>());
				_topThreat=opponent.King;
			}
		}
		if(opponent.ChessPieceNames.Count>0)
		{
			foreach(string piece in opponent.ChessPieceNames )
			{
				GameObject obj=opponent.ChessPieces[piece];
				
				if(legitTarget(obj))//possible threat may also be possible target
				{
					obj.GetComponent<ChessPieceBehaviour>().updateThreat(gameObject);
					_targets.Add(obj);
					
				}
				if(obj.GetComponent<ChessPieceBehaviour>().legitTarget(gameObject))
				{
					obj.GetComponent<ChessPieceBehaviour>().Targets.Add(gameObject);
					//compare all threatening pieces.Mark the most threatening one
					_threats.Add(obj);
					float newThreatValue=rankThreat(obj.GetComponent<ChessPieceBehaviour>());
					if(newThreatValue >threatValue)
					{
						threatValue=newThreatValue;
						_topThreat=obj;
					}
				}
			}
		}
		if(_topThreat!=null)
		{
			transform.LookAt(_topThreat.transform.position);
			if(_currentBehavior!=behaviorStates.THREATENED)
				_soundEffect.PlayOneShot(_guardSound,.75f);
			_currentBehavior=behaviorStates.THREATENED;
			anim.SetBool(_threatHash,true);
		}
		else
		{
			_currentBehavior=behaviorStates.WAITING;
			anim.SetBool(_threatHash,false);
		}
	}

	public virtual float rankThreat(ChessPieceBehaviour threat)
	{
		if (threat.containsModifier (Modifier.ModifierType.MUMMIES_CURSE))
						return .1f;
		if (threat._name == "Medusa" && threat.suseptableToEffect (this.gameObject))
						return threat._attackMultiplier * 3f;
		if (threat.suseptableToEffect (this.gameObject))
						return threat._attackMultiplier*1.5f;
		return threat._attackMultiplier;
	}
	// if a peice comes within threatening range it should be measured against current threats
	public virtual void updateThreat(GameObject threat)
	{
		float oldThreatValue = 0;
		_threats.Add (threat);
		float newThreatValue=rankThreat(threat.GetComponent<ChessPieceBehaviour>());
		if(_topThreat!=null)
		{
			oldThreatValue = _topThreat.GetComponent<ChessPieceBehaviour> ()._attackRange * _topThreat.GetComponent<ChessPieceBehaviour> ()._hitPoints;
			if(newThreatValue>oldThreatValue)
				_topThreat=threat;
			transform.LookAt(_topThreat.transform.position);
			_currentBehavior=behaviorStates.THREATENED;
			anim.SetBool(_threatHash,true);
		}
		else 
		{
			_topThreat=threat;
			transform.LookAt(_topThreat.transform.position);
			_currentBehavior=behaviorStates.THREATENED;
			anim.SetBool(_threatHash,true);
		}
	}
	//when a piece moves out of a hex its pevious potential targets must reevaluate their threats, and the list of potetial targets must be reset
	public virtual void updateTargets()
	{
		if (_targets.Count > 0)
		{
			foreach(GameObject target in _targets)
			{
				if(target==null)
					_targets.Remove(target);
				else
				{
					target.GetComponent<ChessPieceBehaviour>().Threats.Remove(gameObject);
					target.GetComponent<ChessPieceBehaviour>().checkThreats();
				}
			}
			_targets.Clear ();
		}
		if(_threats.Count > 0) 
		{
			foreach( GameObject threat in _threats)
			{
				if(threat==null)
					_targets.Remove(threat);
				else
					threat.GetComponent<ChessPieceBehaviour>().Targets.Remove(gameObject);
			}
			_threats.Clear();
		}
	}
	public virtual void addModifier(Modifier.ModifierType modType)
	{
		if(!containsModifier(modType))
		{
			Modifier mod=new Modifier(modType);
			if(mod.IsBuff==true)
				_buffs.Add(mod);
			else
				_debuffs.Add(mod);
		}
	}

	public virtual void removeBlessing(Modifier.ModifierType modType)
	{
		for(int i=0;i<_buffs.Count;i++)
		{
			if(_buffs[i].Type==modType)
				_buffs.Remove(_buffs[i]);
		}
	}

	public virtual void removeCurses()
	{
		_debuffs.Clear ();
		checkThreats ();
	}

	public bool containsModifier(Modifier.ModifierType modType)
	{
		if(_debuffs.Count>0)
		{
			for(int i=0;i<_debuffs.Count;i++)
			{
				if(_debuffs[i].Type==modType)
					return true;
			}
		}
		if (_buffs.Count > 0) 
		{
			for(int i=0;i<_buffs.Count;i++)
			{
				if(_buffs[i].Type==modType)
					return true;
			}
		}
		return false;
	}

	public virtual bool suseptableToEffect(GameObject target)
	{
		if (target.GetComponent<ChessPieceBehaviour> ().containsModifier (Modifier.ModifierType.UNDEAD))
						return false;
		return true;
	}

	public virtual void onStartOfTurn()
	{
		if(!containsModifier(Modifier.ModifierType.PETRIFIED))
		{
			if (containsModifier (Modifier.ModifierType.DEMONS_POISON))
				_currentHP--;
			if (_currentHP == 0) 
			{
				death();
			}
		}
	}

	public virtual bool canDoSpecial()
	{
		return true;
	}

	#region AI_relatedFunctions
	//These functions will be called by the AI's PieceBrain script

	public virtual void addAI()
	{
		gameObject.AddComponent<PieceBrain>();
		_AI_brain = gameObject.GetComponent<PieceBrain> ();
	}

	#endregion
	// save and load functions

	public virtual ChessPieceData saveData(string name)
	{
		ChessPieceData data = new ChessPieceData ();
		data.typeName = _name;
		data.listName = name;
		data.lastPosition = new ChessPieceData.lastPositionVector(gameObject.transform.position);
		data.lastRotation = new ChessPieceData.lastQuaternion(gameObject.transform.rotation);
		data.currentHP = _currentHP;
		data.homeHexRow = _homeHex.GetComponent<HexScript> ()._row;
		data.homeHexCol = _homeHex.GetComponent<HexScript> ()._column;
		data.held = _held;
		data.effectType = _effectType;
		data.debuffData = _debuffs;

		return data;
	}

	public virtual void initializeFromData(ChessPieceData data,Player player)
	{
		_pieceId = data.listName;
		_owner = player;
		_currentHP = data.currentHP;
		_held = data.held;
		_debuffs = data.debuffData;
		_effectType = data.effectType;
		_teamcolor = player.getColor ();
		_homeHex=GameState.Instance.Board[data.homeHexRow,data.homeHexCol];
		_homeHex.GetComponent<HexScript> ().setOccupant (gameObject);

		if(containsModifier(Modifier.ModifierType.PETRIFIED))
		{
			MaterialChanger[] mats=transform.GetComponentsInChildren<MaterialChanger>();
			foreach(MaterialChanger changer in mats)
			{
				changer.replaceMaterials(GameState.Instance._petrificationMat);
			}
		}
	}
	public virtual bool checkForMoves()
	{
		for(int row=0;row<8;row++)
		{
			for(int column=0;column<8;column++)
			{
				if(_homeHex.GetComponent<HexScript>().canMoveToHex(GameState.Instance.Board[row,column]))
					return true;
			}
		}
		return false;
	}
	public virtual bool checkForAttacks()
	{
		if (_targets.Count > 0)
						return true;
		return false;
	}
}

[System.Serializable]
public class ChessPieceData
{
	public string typeName;
	public string listName;
	[System.Serializable]
	public struct lastPositionVector
	{
		public float posX;
		public float posY;
		public float posZ;

		public lastPositionVector(Vector3 position)
		{
			posX = position.x;
			posY = position.y;
			posZ = position.z;
		}
	}
	[System.Serializable]
	public struct lastQuaternion
	{
		public float rotX;
		public float rotY;
		public float rotZ;
		public float w;

		public lastQuaternion(Quaternion rotation)
		{
			rotX=rotation.x;
			rotY=rotation.y;
			rotZ=rotation.z;
			w=rotation.w;
		}
	}
	public lastPositionVector lastPosition;
	public lastQuaternion lastRotation;
	public int currentHP;
	public int homeHexRow;
	public int homeHexCol;
	public bool held;
	public ChessPieceBehaviour.specialEffectType effectType;
	public List<Modifier> debuffData;
	//lists for additional piece specific data. Data ids will kept in the sam order as the data;
	public List<string> additionalDataIds;
	public List<string> additionalStrings;
	public List<bool> additionalBools;


	public Vector3 loadLastPosition()
	{
		Vector3 vector = new Vector3 (lastPosition.posX,lastPosition.posY,lastPosition.posZ);
		return vector;
	}

	public Quaternion loadLastRotation()
	{
		Quaternion quat = new Quaternion (lastRotation.rotX,lastRotation.rotY,lastRotation.rotZ,lastRotation.w);
		return quat;
	}
}

