/* The GUI rendered during gameplay
copywrite Greg Ostroy*/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameplayGUI : MonoBehaviour {
	// MemberVaraibles
	GameObject _cameraFocus;
	GameObject _guiCanvas;

	// cursor textures
	public Texture2D _pickTexture;
	public Texture2D _attackTexture;
	public Texture2D _moveTexture;
	public Texture2D _specialTexture;
	//public Texture2D _negativeTexture;

	int _saveSelectionIndex=0;
	Vector2 _scrollPos;

	GameObject _currentHex;
	GameObject _currentPiece;
	GameObject _targetHex;

	Ray _ray;

	Rect _gameMenuWindow;
	Rect _pieceInterfaceWindow;
	Rect _endGamelWindow;
	Rect _endGamePlayer1;
	Rect _endGamePlayer2;
	Rect _loadSaveDialog;
	Rect _MenuDialog;
	Rect _popuplDialog;
	
	string _defaultFileName;
	string _endGameDisplay;

	bool _canMove=true;
	bool _canAttack=true;
	bool _canDoSpecial=true;
	bool _currentPlayersPiece=false;
	bool _tutorial=false;

	float _timeSinceLastClick=0;

	bool _specialPick=false;

	Color startColor= new Color(0,.5f,0);

	public enum PickingStates{PICK_MODE,ACTION_MODE,MOVE_MODE,ATTACK_MODE,SPECIAL_MODE,MENU_MODE,SAVE_MODE,VICTORY_MODE,DUEL_MODE};
	PickingStates _currentState=PickingStates.PICK_MODE;

	//Rect text;
	//public string log="";
	public PickingStates CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			_currentState=value;
		}
	}
	public bool CanAttack
	{
		get
		{
			return _canAttack;
		}
		set
		{
			_canAttack=value;
		}
	}
	public bool CanMove
	{	
		get
		{
			return _canMove;
		}
		set
		{
			_canMove=value;
		}	
	}
	public bool CanDoSpecial
	{
		get
		{
			return _canDoSpecial;
		}
		set
		{
			_canDoSpecial=value;
		}
	}
	public bool CurrentPlayersPiece
	{
		get
		{
			return _currentPlayersPiece;
		}
		set
		{
			_currentPlayersPiece=value;
		}
	}
	public GameObject TargetHex
	{
		get
		{
			return _targetHex;
		}
		set
		{
			_targetHex=value;
		}
	}
	public GameObject CurrentHex
	{
		get
		{
			return _currentHex;
		}
		set
		{
			_currentHex=value;
		}
	}
	public GameObject CurrentPiece
	{
		get
		{
			return _currentPiece;
		}
		set
		{
			_currentPiece=value;
		}
	}
	public Rect Popup
	{
		get
		{
			return _popuplDialog;
		}
	}
	public bool SpecialPick
	{
		get
		{
			return _specialPick;
		}
		set
		{
			_specialPick=value;
		}
	}
	// Use this for initialization
	void Start () 
	{
		GameState.Instance.GUI = null;
		if(SceneManager.GetActiveScene().name=="GameCreation")
			gameObject.GetComponent<GameGenerationGUI> ().enabled = false;

		_gameMenuWindow = new Rect (Screen.width*.25f,Screen.height*.9f,Screen.width*.75f,Screen.height/10f);
		_pieceInterfaceWindow = new Rect (0,0,Screen.width/4f,Screen.height);
		_endGamelWindow = new Rect (Screen.width/2f-Screen.width/6f,Screen.height*.05f,Screen.width/3f,Screen.height/6f);
		_endGamePlayer2=new Rect(Screen.width*.4f,Screen.height/2f-Screen.height/12f,Screen.width/3f,Screen.height/6f);
		_endGamePlayer1=new Rect(Screen.width-Screen.width*.4f,Screen.height/2f-Screen.height/12f,Screen.width/3f,Screen.height/6f);

		float width = Screen.width / 2f;
		float height = Screen.height / 2f;
		float startY = Screen.height / 4f;
		_loadSaveDialog = new Rect (0,startY,width,height);
 		width = Screen.width / 4f;
		_MenuDialog = new Rect (0,startY,width,height);
		width = Screen.width / 5f;
		height = Screen.height / 8f;
		_popuplDialog = new Rect ((Screen.width/2f)-(width/2f),Screen.height/4f,width,height);

		GameState.Instance.GUI = gameObject;
		_cameraFocus = GameObject.Find ("CameraFocus");
		_guiCanvas = gameObject.transform.Find ("GameplayCanvas").gameObject;
		_guiCanvas.GetComponent<GUIMetaControls>().EndTurnDialog =	_guiCanvas.transform.Find ("EndTurnDialog").gameObject;
		GameState.Instance.setCurrentPlayer (true);
		//do some initialization that can't be done until all pieces are chosen
		if(SceneManager.GetActiveScene().buildIndex==1)
			initializePiecesForPlay();
		else if(SceneManager.GetActiveScene().buildIndex==3)
			_tutorial=true;

		//text = new Rect (Screen.width*.8f,Screen.height*.1f,Screen.width/5f,Screen.height/10f);
	}
	
	// Update is called once per frame
	void Update () 
	{
		_timeSinceLastClick += Time.deltaTime;
		switch (_currentState) 
		{
			case PickingStates.PICK_MODE:
				mousePick ();
				break;
			case PickingStates.ACTION_MODE:
				mouseChooseAction();
				break;
			case PickingStates.MOVE_MODE:
				mouseMove();
				break;
			case PickingStates.ATTACK_MODE:
				mouseAttack();
				break;
			case PickingStates.SPECIAL_MODE:
				mouseSpecial();
				break;
			default:
				break;
		}
		if (Input.GetKey("p"))
		{
			string defaultName="MythicChess_Screenshot_"+System.DateTime.Now.ToString();
			defaultName=defaultName.Replace("/","_");
			defaultName = defaultName.Replace (":",";");
			Application.CaptureScreenshot (Application.persistentDataPath +"/"+defaultName+".png");
		}
	}
	
	void OnGUI()
	{
		//GUI.TextArea (text,log);
		if (_currentState == PickingStates.DUEL_MODE )
		{
			GUI.Label (_endGamelWindow,"Duel Mode Initiated!");
			GUI.Label(_endGamePlayer1,GameState.Instance.getPlayer(true).Name);
			GUI.Label(_endGamePlayer2,GameState.Instance.getPlayer(false).Name);
		}
		else if( _currentState == PickingStates.VICTORY_MODE)
		{
			GUILayout.BeginArea(_endGamelWindow);
			GUILayout.Label(GameState.Instance.Winner.Name+ " Wins!");
			if(_tutorial==false)
			{
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Main Menu"))
				{
					GameState.Instance.clearGameData();
					SceneManager.LoadScene("MainMenu");
				}
				if(GUILayout.Button("Quit"))
				{
					Application.Quit();
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndArea();
		}
		else if(_currentState == PickingStates.MENU_MODE)
		{
			menuDialog();
		}
		else if(_currentState == PickingStates.SAVE_MODE)
		{
			saveDialog();
		}
		else
		{
			_pieceInterfaceWindow=GUILayout.Window(0,_pieceInterfaceWindow,pieceInterface,"Current Piece");
			_gameMenuWindow=GUILayout.Window(1,_gameMenuWindow,gameMenu,"Menu");

			if( _specialPick==true)
			{
				if(!_currentPiece.GetComponent<ChessPieceBehaviour> ().canDoSpecial()) 
				{
					_popuplDialog=GUILayout.Window (4,_popuplDialog,cantDoSpecialDialog,"");
				}
				else if(_currentPiece.GetComponent<ChessPieceBehaviour>()._effectType!=ChessPieceBehaviour.specialEffectType.DIRECTED)
				{
					_popuplDialog=GUILayout.Window (4,_popuplDialog,_currentPiece.GetComponent<ChessPieceBehaviour>().specialDialogBox,"");
				}
			}
		}
	}

	void cantDoSpecialDialog(int winID)
	{
		GUILayout.Label ("Can't do special action this turn.");
		if(GUILayout.Button("Ok"))
		{
			_currentPlayersPiece = true;
			Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
			_specialPick=false;
			_currentState=GameplayGUI.PickingStates.ACTION_MODE;
			GameState.Instance.MouseEnabled=true;
		}
	}
	void menuDialog()
	{
		GUILayout.BeginArea (_MenuDialog);
		if(GUILayout.Button("Save Game"))
		{
			GameState.Instance.populateSaveList();
			_defaultFileName=GameState.Instance.setDefaultSaveName();
			_currentState=PickingStates.SAVE_MODE;
		}
		if(GUILayout.Button("Exit(Main Menu)"))
		{
			GameState.Instance.clearGameData();
			SceneManager.LoadScene("MainMenu");
		}
		if(GUILayout.Button("Exit(Desktop)"))
		{
			Application.Quit ();
		}
		if(GUILayout.Button("Resume"))
		{
			_currentState=PickingStates.PICK_MODE;
		}
		GUILayout.EndArea ();
	}

	void saveDialog()
	{
		GUILayout.BeginArea (_loadSaveDialog);
		_saveSelectionIndex = GUILayout.SelectionGrid (_saveSelectionIndex,GameState.Instance.SaveList,1);
		GUILayout.BeginHorizontal();
		string file=GUILayout.TextField(_defaultFileName);
		if(GUILayout.Button("New Save"))
		{
			GameState.Instance.SaveGame(file);
			_currentState=PickingStates.MENU_MODE;
			_saveSelectionIndex=0;
		}
		GUILayout.EndHorizontal();
		string prevSave=GameState.Instance.SaveList[_saveSelectionIndex];
		if(!prevSave.Equals("No Games Are Saved"))
		{
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Save Over.."))
			{
				GameState.Instance.SaveGame(prevSave);
				_currentState=PickingStates.MENU_MODE;
				_saveSelectionIndex=0;
			}
			GUILayout.Label(prevSave);
			GUILayout.EndHorizontal();
		}
		if(GUILayout.Button("Cancel"))
		{
			_currentState=PickingStates.MENU_MODE;
			_saveSelectionIndex=0;
		}
		GUILayout.EndArea ();
	}

	void pieceInterface(int winID)
	{
		if (_currentPiece == null)
						GUILayout.Label (" No Piece Selected");
		else if (_currentState == PickingStates.MOVE_MODE)
		{
			GUILayout.Label (" Hover the mouse over a valid hex to move to, then left click to move. Right click to cancel");
			if(GUILayout.Button ("Cancel"))
			{
				_currentPlayersPiece=true;
				Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
				_currentState=PickingStates.ACTION_MODE;
				restoreHexColors();
			}
		}				
		else if (_currentState== PickingStates.ATTACK_MODE)
		{
			GUILayout.Label(" Hover the mouse over a valid target, then left click to attack. Right click to cancel");
			if(GUILayout.Button ("Cancel"))
			{
				_currentPlayersPiece=true;
				_currentState=PickingStates.ACTION_MODE;
				restoreHexColors();
			}
		}				
		else if (_currentState== PickingStates.SPECIAL_MODE &&
		         _currentPiece.GetComponent<ChessPieceBehaviour>()._effectType==ChessPieceBehaviour.specialEffectType.DIRECTED)
		{
			_currentPiece.GetComponent<ChessPieceBehaviour>().specialInterface();
			if(GUILayout.Button ("Cancel"))
			{
				_specialPick=false;
				Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
				_currentPiece.GetComponent<ChessPieceBehaviour>().cancelAction();
			}
		}
		else 
		{
			_scrollPos=GUILayout.BeginScrollView(_scrollPos);
			string currentHitPoints=_currentPiece.GetComponent<ChessPieceBehaviour>().CurrentHP.ToString();
			string fullHitPoints=_currentPiece.GetComponent<ChessPieceBehaviour>()._hitPoints.ToString();
			//type of piece
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Piece Type:");
			GUILayout.Label(_currentPiece.GetComponent<ChessPieceBehaviour> ()._name);
			GUILayout.EndHorizontal ();
			//peices hit points
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Current Hit Points:");
			GUILayout.Label (currentHitPoints+" / "+ fullHitPoints);
			GUILayout.EndHorizontal ();

			GUILayout.Label ("Current Modifiers");
			GUILayout.TextArea(modDescriptions());

			GUI.enabled=enableMove();
			if(GUILayout.Button("Move"))
			{
				Cursor.SetCursor(_moveTexture,Vector2.zero,CursorMode.Auto);
				lightUpMoveHexes();
				_currentState=PickingStates.MOVE_MODE;
			}
			GUILayout.TextArea(_currentPiece.GetComponent<ChessPieceBehaviour> ().MoveDescription);
			GUI.enabled=enableAttack();
			if(GUILayout.Button("Attack"))
			{
				Cursor.SetCursor(_attackTexture,Vector2.zero,CursorMode.Auto);
				lightUpAttackHexes();
				_currentState=PickingStates.ATTACK_MODE;
			}
			GUILayout.TextArea(_currentPiece.GetComponent<ChessPieceBehaviour> ().AttackDescription);
			GUI.enabled=enableSpecial();
			if(GUILayout.Button("Special"))
			{
				_currentState=PickingStates.SPECIAL_MODE;
			}
			GUILayout.TextArea(_currentPiece.GetComponent<ChessPieceBehaviour> ().SpecialDescription);
			GUILayout.EndScrollView();
		}
	}

	void gameMenu(int winID)
	{
		GUILayout.BeginVertical ();
		GUILayout.BeginHorizontal ();
		GUILayout.Label (GameState.Instance.getPlayer (true).Name + " vs " + GameState.Instance.getPlayer (false).Name +":  ");
		GUILayout.Label (GameState.Instance.CurrentPlayer.Name + "'s turn");
		GUI.enabled = GameState.Instance.MouseEnabled;
		if(GUILayout.Button("End Turn"))
		{
			if(_tutorial==false)
				endTurn();
			else
				gameObject.GetComponent<TutorialGUI>().CurrentLesson.next();
		}
		if(_tutorial==false)
		{
			if (GUILayout.Button ("Menu Options"))
			{
				_currentState=PickingStates.MENU_MODE;
			}
			/*if(GUILayout.Button("Duel Mode"))
			{
				_currentState=GameplayGUI.PickingStates.DUEL_MODE;
				StartCoroutine(GameState.Instance.DuelRoutine());
			}*/
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
	}

	public void resetGUI () 	
	{
		_canAttack = true;
		_canMove = true;
		_canDoSpecial = true;
		if(_currentHex!=null)
			_currentHex.GetComponent<HexScript> ().setColor ();
		_currentPiece = null;
		_currentState = PickingStates.PICK_MODE;
	}

	public void ResetOccupants(GameObject piece,GameObject hex)
	{
		hex.GetComponent<HexScript>().setOccupant(piece);
		piece.GetComponent<ChessPieceBehaviour>().HomeHex.GetComponent<HexScript>().removeOccupant();
		piece.GetComponent<ChessPieceBehaviour>().HomeHex=hex;
		if(piece.GetComponent<PieceBrain>()==null)//peice is owned by a human player, reset mouse picking
		{
			_currentHex=_targetHex;
			_targetHex=null;
		}
	}

	bool doubleClicked()
	{
		if (GameState.Instance.MouseEnabled == false)
						return false;
		if(Input.GetMouseButtonDown(0))
		{
			if(_timeSinceLastClick<.3f)
			{
					_timeSinceLastClick=0;
					return true;
			}
			_timeSinceLastClick=0;
			return false;
		}
		return false;
	}
	void mousePick()
	{
		_specialPick = false;
		if(_targetHex!=null)
			_targetHex.GetComponent<HexScript>().setColor();
		if (_currentPiece != null && GameState.Instance.CurrentPlayer.getColor()==_currentPiece.GetComponent<ChessPieceBehaviour>().TeamColor)
						_currentPiece.GetComponent<ChessPieceBehaviour> ().HomeHex.gameObject.GetComponent<Renderer>().material.color = Color.green;
		if (Input.GetAxis("Select")>0 && GameState.Instance.MouseEnabled==true) 
		{
			if(_currentHex!=null)
				_currentHex.GetComponent<HexScript>().setColor();
			_ray=Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(_ray,out hit,500f))
			{
				if(hit.collider.gameObject.tag=="ChessPiece")
				{
					_currentPiece=hit.collider.gameObject;
					_currentHex=_currentPiece.GetComponent<ChessPieceBehaviour>().HomeHex;
					if(GameState.Instance.CurrentPlayer.getColor()==_currentPiece.GetComponent<ChessPieceBehaviour>().TeamColor && !
						_currentPiece.GetComponent<ChessPieceBehaviour>().containsModifier(Modifier.ModifierType.PETRIFIED)
						&& _currentPiece.GetComponent<ChessPieceBehaviour>().Held==false)
					{
						_currentHex.gameObject.GetComponent<Renderer>().material.color=Color.green;
						_currentState=PickingStates.ACTION_MODE;
						_currentPlayersPiece=true;
					}							
					else
					{
							_currentHex.gameObject.GetComponent<Renderer>().material.color=Color.red;
							_currentPlayersPiece=false;
					}						
				}
				else
				{
					_currentPiece=null;
					_currentPlayersPiece=false;
				}
			}
		}
	}

	public void Cancel()
	{
		if(_targetHex!=null)
			_targetHex.GetComponent<HexScript>().setColor();
		_targetHex=null;
		if(_currentHex!=null)
			_currentHex.GetComponent<HexScript>().setColor();
		_currentHex=null;
		_currentPiece=null;
		_currentPlayersPiece=false;
		Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
		_currentState=PickingStates.PICK_MODE;
	}

	void mouseChooseAction()
	{
		_currentPiece.GetComponent<ChessPieceBehaviour> ().HomeHex.gameObject.GetComponent<Renderer>().material.color = Color.green;
		if(doubleClicked())
		{
				Cursor.SetCursor(_specialTexture,Vector2.zero,CursorMode.Auto);
				_specialPick=true;
		}
		if(_specialPick==false)
		{
			if(Input.GetAxis("Cancel")>0 && GameState.Instance.MouseEnabled==true)
			{
				Cancel();
			}
			else
			{
				_ray=Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(_ray,out hit,500f))
				{
					if(_targetHex!=null & _targetHex!=_currentHex)
						_targetHex.GetComponent<HexScript>().setColor();
					if(hit.collider.gameObject.tag=="Hex")
					{
						_targetHex=hit.collider.gameObject;
						if(_targetHex.GetComponent<HexScript>().hasOccupant())
						{
							pieceSelect(_targetHex.GetComponent<HexScript>().Occupant);
						}
						else
						{
							Cursor.SetCursor(_moveTexture,Vector2.zero,CursorMode.Auto);
							moveSelect(hit.collider.gameObject);
						}
					}
					else if(hit.collider.gameObject.tag=="ChessPiece")
					{
						GameObject targetPiece=hit.collider.gameObject;
						pieceSelect(targetPiece);
					}
					else
						Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
				}
				else
				{
					if(_targetHex!=null&& _targetHex!=_currentHex)
						_targetHex.GetComponent<HexScript>().setColor();
					_targetHex=null;
					Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
				}
				
			}
		}
		else
				specialSelect();
	}
	void pieceSelect(GameObject piece)
	{
		if(piece.GetComponent<ChessPieceBehaviour>().TeamColor==_currentPiece.GetComponent<ChessPieceBehaviour>().TeamColor)
		{
			Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
			allySelect(piece);
		}
		else
		{
			Cursor.SetCursor(_attackTexture,Vector2.zero,CursorMode.Auto);
			attackSelect(piece);
		}
	}
	void allySelect(GameObject target)
	{
		if(target.GetComponent<ChessPieceBehaviour>().containsModifier(Modifier.ModifierType.PETRIFIED)||
		   target.GetComponent<ChessPieceBehaviour>().Held==true)
		{
			_currentPiece=target;
			_currentHex.GetComponent<HexScript>().setColor();
			_currentHex=_currentPiece.GetComponent<ChessPieceBehaviour>().HomeHex;
			_currentHex.gameObject.GetComponent<Renderer>().material.color=Color.red;
			_currentPlayersPiece=false;
			_currentState=PickingStates.PICK_MODE;
		}
		else
		{
			if(Input.GetAxis("Select")>0 && GameState.Instance.MouseEnabled==true)
			{
				_currentPiece=target;
				_currentHex.GetComponent<HexScript>().setColor();
				_currentHex=_currentPiece.GetComponent<ChessPieceBehaviour>().HomeHex;
				_currentHex.gameObject.GetComponent<Renderer>().material.color=Color.green;
			}
		}
	}

	void specialSelect()
	{
		if (_currentPiece.GetComponent<ChessPieceBehaviour> ()._effectType == ChessPieceBehaviour.specialEffectType.DIRECTED
		    && _currentPiece.GetComponent<ChessPieceBehaviour> ().canDoSpecial())
						_currentState = PickingStates.SPECIAL_MODE;
	}

	void mouseMove()
	{
		_specialPick = false;
		_currentPlayersPiece = false;
		if(_currentHex!=_targetHex)
			_currentHex.gameObject.GetComponent<Renderer>().material.color=Color.green;
		if(Input.GetMouseButtonUp(1) && GameState.Instance.MouseEnabled==true)
		{
			_currentPlayersPiece=true;
			Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
			_currentState=PickingStates.ACTION_MODE;
			restoreHexColors();
		}
		else
		{
			_ray=Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hits=Physics.RaycastAll(_ray,500f);
			foreach(RaycastHit hit in hits)
			{
				if(hit.collider.gameObject.tag=="Hex")
				{
					if(_targetHex!=null)
					{
						if(_targetHex.GetComponent<Renderer>().material.color.Equals(startColor))
							_targetHex.GetComponent<Renderer>().material.color=Color.green;
					}
					_targetHex=hit.collider.gameObject;
					if(_targetHex.GetComponent<HexScript>().Occupant==null && _currentHex.GetComponent<HexScript>().canMoveToHex(_targetHex))
					{
						_targetHex.GetComponent<Renderer>().material.color=startColor;
						if(Input.GetAxis("Select")>0 && GameState.Instance.MouseEnabled==true)
						{
							GameState.Instance.MouseEnabled=false;
							Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
							_currentState=PickingStates.ACTION_MODE;
							StartCoroutine( _currentPiece.GetComponent<ChessPieceBehaviour>().Move(_targetHex));
						}
					}
				}
				else
				{
					if(_targetHex!=null)
					{
						if(_targetHex.GetComponent<Renderer>().material.color.Equals(startColor))
							_targetHex.GetComponent<Renderer>().material.color=Color.green;
						_targetHex=null;
					}
				}
			}
		}
	}

	void mouseAttack()
	{
		_specialPick = false;
		_currentPlayersPiece = false;
		if(_currentHex!=_targetHex)
			_currentHex.gameObject.GetComponent<Renderer>().material.color=Color.green;
		if(Input.GetMouseButtonUp(1)&& GameState.Instance.MouseEnabled==true)
		{
			_currentPlayersPiece=true;
			Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
			_currentState=PickingStates.ACTION_MODE;
			restoreHexColors();
		}
		else
		{
			_ray=Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
            if(Physics.Raycast(_ray,out hit,500f))
			{
				if(hit.collider.gameObject.tag=="ChessPiece")
  				{
					if(_targetHex!=null)
						_targetHex.GetComponent<Renderer>().material.color=startColor;
					GameObject targetPiece=hit.collider.gameObject;
					_targetHex=targetPiece.GetComponent<ChessPieceBehaviour>().HomeHex;
					startColor=_targetHex.GetComponent<Renderer>().material.color;
					if(_currentPiece.GetComponent<ChessPieceBehaviour>().legitTarget(targetPiece))
					{
						_targetHex.gameObject.GetComponent<Renderer>().material.color=new Color(0,.5f,0);
						if(Input.GetAxis("Select")>0 && GameState.Instance.MouseEnabled==true)
						{
							GameState.Instance.MouseEnabled=false;
							StartCoroutine(_currentPiece.GetComponent<ChessPieceBehaviour>().Attack(targetPiece));
						}
					}
				}
			}
			else
			{
				_targetHex=null;
			}
		}
	}

	void mouseSpecial()
	{
		_currentPlayersPiece = false;
		if(_targetHex!=null)
			_targetHex.GetComponent<HexScript>().setColor();
		switch(_currentPiece.GetComponent<ChessPieceBehaviour>()._effectType)
		{
			case ChessPieceBehaviour.specialEffectType.SELF:
				GameState.Instance.MouseEnabled=false;
				StartCoroutine(_currentPiece.GetComponent<ChessPieceBehaviour>().doSpecial());
				_currentState=PickingStates.ACTION_MODE;
				break;
			case ChessPieceBehaviour.specialEffectType.AOE:
				GameState.Instance.MouseEnabled=false;
				StartCoroutine(_currentPiece.GetComponent<ChessPieceBehaviour>().doSpecial());
				_currentState=PickingStates.ACTION_MODE;
				break;
			case ChessPieceBehaviour.specialEffectType.DIRECTED:
				Cursor.SetCursor(_specialTexture,Vector2.zero,CursorMode.Auto);
				_currentPiece.GetComponent<ChessPieceBehaviour>().mouseInput();
				break;
			default:
				_currentState=PickingStates.PICK_MODE;
				break;
		}
	}
 	void moveSelect(GameObject target)
	{
		_targetHex=target;
		if(_targetHex.GetComponent<HexScript>().Occupant==null && _currentHex.GetComponent<HexScript>().canMoveToHex(_targetHex)
		   && _canMove==true)
		{
			_targetHex.gameObject.GetComponent<Renderer>().material.color=Color.green;
			if(Input.GetAxis("Select")>0 && GameState.Instance.MouseEnabled==true)
			{
				GameState.Instance.MouseEnabled=false;
				Cursor.SetCursor(_pickTexture,Vector2.zero,CursorMode.Auto);
				_currentState=PickingStates.PICK_MODE;
				StartCoroutine( _currentPiece.GetComponent<ChessPieceBehaviour>().Move(_targetHex));
			}
		}
		else
			_targetHex.gameObject.GetComponent<Renderer>().material.color=Color.red;
			
	}
	void attackSelect(GameObject target)
	{
		_targetHex=target.GetComponent<ChessPieceBehaviour>().HomeHex;
		if(_currentPiece.GetComponent<ChessPieceBehaviour>().legitTarget(target))
		{
			if(_canAttack==true)
			{
				_targetHex.gameObject.GetComponent<Renderer>().material.color=Color.green;
				if(Input.GetAxis("Select")>0 && GameState.Instance.MouseEnabled==true)
				{
					GameState.Instance.MouseEnabled=false;
					StartCoroutine(_currentPiece.GetComponent<ChessPieceBehaviour>().Attack(target));
				}
			}
			else
			{
				_targetHex.gameObject.GetComponent<Renderer>().material.color=Color.red;
			}
		}
		else
			_targetHex.gameObject.GetComponent<Renderer>().material.color=Color.red;
	}

	public void lightUpMoveHexes()
	{
		foreach(GameObject hex in GameState.Instance.Board)
		{
			if(hex.GetComponent<HexScript>().Occupant==null && _currentHex.GetComponent<HexScript>().canMoveToHex(hex))
				hex.gameObject.GetComponent<Renderer>().material.color=Color.green;
		}
	}

	void lightUpAttackHexes()
	{
		foreach(GameObject hex in GameState.Instance.Board)
		{
			if(hex.GetComponent<HexScript>().hasOccupant())
			{
				if(_currentPiece.GetComponent<ChessPieceBehaviour>().legitTarget(hex.GetComponent<HexScript>().Occupant))
					hex.gameObject.GetComponent<Renderer>().material.color=Color.green;
			}
		}
	}

	public void restoreHexColors ()
	{
		foreach (GameObject hex in GameState.Instance.Board)
						hex.GetComponent<HexScript> ().setColor ();
		if (_currentState == PickingStates.ACTION_MODE)
						_currentPiece.GetComponent<ChessPieceBehaviour> ().HomeHex.GetComponent<Renderer>().material.color = Color.green;
	}

	public void endTurn()
	{
		GameState.Instance.MouseEnabled=false;
		resetGUI();
		GameState.Instance.newTurn ();
		if(GameState.Instance.AIPlayer==null)//rotate the camera to the next players side of the board if the both players are human
			_cameraFocus.GetComponent<MCPlay_CameraController>().flip();

	}
 	bool enableMove()
	{
		if (GameState.Instance.MouseEnabled == false)
						return false;
		if (_canMove == true && _currentPlayersPiece == true)
						return true;
		return false;
	}
	bool enableAttack()
	{
		if (GameState.Instance.MouseEnabled == false)
			return false;
		if (_canAttack == true && _currentPlayersPiece == true)
			return true;
		return false;
	}
	public bool enableSpecial()
	{
		if (GameState.Instance.MouseEnabled == false)
			return false;
		if(_currentPlayersPiece==true)
		{
			if (_currentPiece.GetComponent<ChessPieceBehaviour> ()._effectType == ChessPieceBehaviour.specialEffectType.NONE ||
			    _currentPiece.GetComponent<ChessPieceBehaviour> ()._effectType == ChessPieceBehaviour.specialEffectType.PASSIVE)
				return false;
			return  _currentPiece.GetComponent<ChessPieceBehaviour> ().canDoSpecial();
		}
		return false;
	}
	string modDescriptions()
	{
		string description=null;
		if (_currentPiece.GetComponent<ChessPieceBehaviour> ().Buffs.Count > 0) 
		{
			foreach(Modifier mod in _currentPiece.GetComponent<ChessPieceBehaviour>().Buffs)
			{
				description+=mod.Description+"\n";
			}				
		}
		if (_currentPiece.GetComponent<ChessPieceBehaviour> ().DeBuffs.Count > 0) 
		{
			foreach(Modifier mod in _currentPiece.GetComponent<ChessPieceBehaviour>().DeBuffs)
			{
				description+=mod.Description+"\n";
			}			
		}
		if (_currentPiece.GetComponent<ChessPieceBehaviour> ().Held == true)
						description += "HELD \n The piece is held and cannot act";
		if (description == null)
						description = "None";
		return description;
	}
	//do some initialization that can't be done until all pieces are chosen
	public void initializePiecesForPlay()
	{
		//add AI to all computer controlled pieces
		if(GameState.Instance.AIPlayer!=null)
		{
			GameState.Instance.AIPlayer.King.GetComponent<ChessPieceBehaviour> ().addAI ();
			foreach(string AIpiece in GameState.Instance.AIPlayer.ChessPieceNames)
			{
				GameState.Instance.AIPlayer.ChessPieces[AIpiece].GetComponent<ChessPieceBehaviour> ().addAI ();
			}
		}
		GameState.Instance.ReadyToPlay = true;
	}
	//check to see if the player can make anymore actions
	public void checkEndTurn()
	{
		_specialPick = false;
		if (_tutorial == false)
		{
			if (!GameState.Instance.checkPossibleAttacks ()&& !GameState.Instance.checkPossibleMoves ())
				_guiCanvas.GetComponent<GUIMetaControls> ().OpenEndTurnDialog ();
			else if(!GameState.Instance.checkPossibleAttacks ()&& _canMove ==false)
				_guiCanvas.GetComponent<GUIMetaControls> ().OpenEndTurnDialog ();
			else if(_canAttack==false && !GameState.Instance.checkPossibleMoves ())
				_guiCanvas.GetComponent<GUIMetaControls> ().OpenEndTurnDialog ();
			else if (_canMove ==false && _canAttack ==false)
				_guiCanvas.GetComponent<GUIMetaControls> ().OpenEndTurnDialog ();
			else if (_canMove==false || _canAttack==false)
				_currentState=PickingStates.ACTION_MODE;
		}
		else
		{
			_currentState=PickingStates.ACTION_MODE;
		}
		restoreHexColors ();
	}	
}
