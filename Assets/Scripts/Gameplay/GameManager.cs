using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public enum Team
{
	Blue,
	Red
}

public enum Controller
{
	Keyboard,
	Joystick1,
	Joystick2,
	Joystick3,
	Joystick4,
    None
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    Count
}

public class GameManager : MonoBehaviour
{
	enum STATE
	{
        SplashScreen,
        Credit,
        PlayerSelection,
        AIDifficulty,
		StartTimer,
		InGame,
		WinnerScreen
	}

	
	const int COUNTDOWN_STEPS = 3; // 3... 2... 1... GO!
    const int MAX_TEAM_SIZE = 2; // Max number of players per team
    const int GOALS_TO_WIN = 5;

    public static Color RedColor = new Color(2.0f, 0f, 0f);
    public static Color BlueColor = new Color(0f, 0f, 2.0f);

	private int playerControllerMask;
	
	public static GameManager instance;
	
	public Text scoreboard;
	public AudioClip timerSound;
	public AudioClip timerGoSound;
	public AudioClip goalSound;
	public Material MaterialTeamRed;
    public Material MaterialTeamRed2;
	public Material MaterialTeamBlue;
    public Material MaterialTeamBlue2;
    public Difficulty GameDifficulty = Difficulty.Easy;

    private GameObject ball;
    private GameObject uiScreen;
	private float startTimer;
	private AudioSource audioSource;
	private int startTimerSoundNbPlayed;
	private STATE state;
	private int[] score = {0,0};
	private GameObject[] players = new GameObject[4];
	private SpawnController spawnController;
    private int gamePlayerCount = 4;
	
	// Use this for initialization
	void Start () {
		instance = this;

        TransitionToSplashScreen();

		spawnController = GetComponent<SpawnController>();
		startTimer = 0;
		UpdateTimerText();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

		switch (state)
		{
        case STATE.SplashScreen:
                if (IsAcceptKeyPressed())
                {
                    TransitionToCredits();
                }
            break;
        case STATE.Credit:
            if (IsAcceptKeyPressed())
            {
                TransitionToPlayerSelection();
            }
            break;
		case STATE.PlayerSelection:
			CheckForNewPlayers();
			CheckForTeamSwitch();

            if (IsStartKeyPressed())
            {
                if (GetNbPlayers() < gamePlayerCount)
                {
                    TransitionToAIDifficulty();
                }
                else
                {
                    StartGame();
                }
            }
			break;
        case STATE.AIDifficulty:
            if (IsAcceptKeyPressed() || IsDownArrowPressed())
            {
                GameDifficulty++;
                GameDifficulty = (Difficulty)((int)GameDifficulty % (int)Difficulty.Count);
                OnDifficultyChanged();
            }

            if(IsUpArrowPressed())
            {
                if (GameDifficulty == Difficulty.Easy)
                    GameDifficulty = Difficulty.Hard;
                else
                    GameDifficulty--;
                OnDifficultyChanged();
            }

            if (IsStartKeyPressed())
                StartGame();


            break;
		case STATE.StartTimer:
			startTimer -= Time.deltaTime;
			UpdateTimerText();
			if(startTimer < 0)
			{
				ball = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Ball"));
				ball.transform.position = Vector3.zero;
				state = STATE.InGame;
			}
			ResetPlayersPositions();
            break;
        case STATE.InGame:
            if (IsGameDone())
            {
                state = STATE.WinnerScreen;

                if (score[(int)Team.Red] == GOALS_TO_WIN)
                {
                    uiScreen = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI_LeviathanWin"));
                }
                else if (score[(int)Team.Blue] == GOALS_TO_WIN)
                {
                    uiScreen = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI_KrakenWin"));
                }
                uiScreen.transform.position = Vector3.zero;
                uiScreen.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                //GameObject.Destroy(ball);
                GameObject.Destroy(players[0]);
                GameObject.Destroy(players[1]);
                GameObject.Destroy(players[2]);
                GameObject.Destroy(players[3]);
            }
            break;
        case STATE.WinnerScreen:
            if (IsAcceptKeyPressed())
            {
                TransitionToPlayerSelection();
            }
			break;
		default:
			break;
		}
	}

    private void OnDifficultyChanged()
    {
        string [] difficultyPrefab = {"UI_Easy", "UI_Medium", "UI_Hard"};

        for(int i = 0; i < (int)Difficulty.Count; ++i)
        {
            GameObject.Find(difficultyPrefab[i]).transform.localScale  = new Vector3(0.5f, 0.5f, 0.5f);
        }

        GameObject.Find(difficultyPrefab[(int)GameDifficulty]).transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private bool IsGameDone()
    {
        return (score[(int)Team.Red] == GOALS_TO_WIN || score[(int)Team.Blue] == GOALS_TO_WIN);
    }

    private bool IsAcceptKeyPressed()
    {
        return Input.GetKeyDown(KeyCode.Return) ||
               Input.GetKeyDown(KeyCode.Joystick1Button0) ||
               Input.GetKeyDown(KeyCode.Joystick2Button0) ||
               Input.GetKeyDown(KeyCode.Joystick3Button0) ||
               Input.GetKeyDown(KeyCode.Joystick4Button0);
    }

    private bool IsUpArrowPressed()
    {
        return Input.GetKeyDown(KeyCode.UpArrow);
    }

    private bool IsDownArrowPressed()
    {
        return Input.GetKeyDown(KeyCode.DownArrow);
    }

    private bool IsStartKeyPressed()
    {
        return (Input.GetKeyDown(KeyCode.Space) && IsControllerActive(Controller.Keyboard))
            || (Input.GetKeyDown(KeyCode.Joystick1Button7) && IsControllerActive(Controller.Joystick1))
            || (Input.GetKeyDown(KeyCode.Joystick2Button7) && IsControllerActive(Controller.Joystick2))
            || (Input.GetKeyDown(KeyCode.Joystick3Button7) && IsControllerActive(Controller.Joystick3))
            || (Input.GetKeyDown(KeyCode.Joystick4Button7) && IsControllerActive(Controller.Joystick4));
    }

    private void TransitionToSplashScreen()
    {
        state = STATE.SplashScreen;
        if (uiScreen)
            GameObject.Destroy(uiScreen);
        uiScreen = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/SplashScreen"));
        uiScreen.transform.position = Vector3.zero;
    }

    private void TransitionToCredits()
    {
        GameObject splashScreen = GameObject.Find("UI_SplashScreen");
        DestroyObject(splashScreen);

        state = STATE.Credit;
        if (uiScreen)
            GameObject.Destroy(uiScreen);
        uiScreen = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Credits"));
        uiScreen.transform.position = Vector3.zero;
    }

    private void TransitionToPlayerSelection()
    {
        playerControllerMask = 0;
        score[0] = score[1] = 0;

        UpdateScoreboard();

        state = STATE.PlayerSelection;
        if (uiScreen)
            GameObject.Destroy(uiScreen);
        uiScreen = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI_ChoseYourTeam"));
        //uiScreen.transform.position = Vector3.zero;
    }

    private void TransitionToAIDifficulty()
    {
        state = STATE.AIDifficulty;

        if (uiScreen)
            GameObject.Destroy(uiScreen);
        uiScreen = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI_Difficulty"));

        OnDifficultyChanged();
    }
	
	private void CheckForNewPlayers()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			AddPlayerController(Controller.Keyboard);
		}
		if (Input.GetKeyDown(KeyCode.Joystick1Button0))
		{
			AddPlayerController(Controller.Joystick1);
		}
		if (Input.GetKeyDown(KeyCode.Joystick2Button0))
		{
			AddPlayerController(Controller.Joystick2);
		}
		if (Input.GetKeyDown(KeyCode.Joystick3Button0))
		{
			AddPlayerController(Controller.Joystick3);
		}
		if (Input.GetKeyDown(KeyCode.Joystick4Button0))
		{
			AddPlayerController(Controller.Joystick4);
		}
	}

    private void createPlayer(int playerId, Team team, Controller controller, bool isHuman)
    {
        if (isHuman)
        {
            players[playerId] = Instantiate<GameObject>(Resources.Load<GameObject>((team == Team.Red ? "Prefabs/Player" : "Prefabs/Player2")));
            players[playerId].GetComponent<PlayerInputController>().SetControls(controller);
        }
        else
        {
            players[playerId] = Instantiate<GameObject>(Resources.Load<GameObject>((team == Team.Red ? "Prefabs/AI" : "Prefabs/AI2")));
        }
        players[playerId].GetComponentInChildren<SkinnedMeshRenderer>().material = GetTeamMaterial(team, (playerId < 2));

        var player = players[playerId].GetComponent<Player>();
        player.team = team;
        player.playerId = playerId;
        player.isHuman = isHuman;
        spawnController.PlayerChangedTeam(player, player.team);
    }

	private void AddPlayerController(Controller controller)
	{
		if (!IsControllerActive(controller))
		{
            var nbPlayers = GetNbPlayers();
            var team = GetSmallestTeam();
            createPlayer(nbPlayers, team, controller, true);
            SetActiveController(controller);
		}
	}
	
	private void CheckForTeamSwitch()
	{
		// Keyboard
		if (Input.GetKeyDown(KeyCode.Q))
		{
			SwitchPlayerTeam(Controller.Keyboard, Team.Red);
		}
		else if (Input.GetKeyDown(KeyCode.E))
		{
			SwitchPlayerTeam(Controller.Keyboard, Team.Blue);
		}
		
		// Joystick1
		if (Input.GetKeyDown(KeyCode.Joystick1Button4)) // LB
		{
			SwitchPlayerTeam(Controller.Joystick1, Team.Red);
		}
		else if (Input.GetKeyDown(KeyCode.Joystick1Button5)) // RB
		{
			SwitchPlayerTeam(Controller.Joystick1, Team.Blue);
		}
		
		// Joystick2
		if (Input.GetKeyDown(KeyCode.Joystick2Button4)) // LB
		{
			SwitchPlayerTeam(Controller.Joystick2, Team.Red);
		}
		else if (Input.GetKeyDown(KeyCode.Joystick2Button5)) // RB
		{
			SwitchPlayerTeam(Controller.Joystick2, Team.Blue);
		}
		
		// Joystick3
		if (Input.GetKeyDown(KeyCode.Joystick3Button4)) // LB
		{
			SwitchPlayerTeam(Controller.Joystick3, Team.Red);
		}
		else if (Input.GetKeyDown(KeyCode.Joystick3Button5)) // RB
		{
			SwitchPlayerTeam(Controller.Joystick3, Team.Blue);
		}
		
		// Joystick4
		if (Input.GetKeyDown(KeyCode.Joystick4Button4)) // LB
		{
			SwitchPlayerTeam(Controller.Joystick4, Team.Red);
		}
		else if (Input.GetKeyDown(KeyCode.Joystick4Button5)) // RB
		{
			SwitchPlayerTeam(Controller.Joystick4, Team.Blue);
		}
	}
	
	private void SwitchPlayerTeam(Controller controller, Team team)
	{
		var playerGameObject = GetPlayerFromController(controller);
		if (!playerGameObject) return;
		
		var canSwitch = GetPlayersInTeam(team).Count() <= MAX_TEAM_SIZE;
		if (!canSwitch) return;
		
		// Change player team and respawn
		//Debug.Log(string.Format("Swich player '{0}' to team '{1}'", controller, team));
		var player = playerGameObject.GetComponent<Player>();
        int id = player.playerId;
        bool isHuman = player.isHuman;
        Object.Destroy(players[id]);

        createPlayer(id, team, controller, isHuman);
	}
	
	private void StartGame()
	{
        GameObject.Destroy(uiScreen);

		var nbPlayers = GetNbPlayers();
        while (nbPlayers < gamePlayerCount)
        {
            var smallestTeam = GetSmallestTeam();
            createPlayer(nbPlayers, smallestTeam, Controller.None, false);
            //Debug.Log(string.Format("AI is team: '{0}'", player.team));
            nbPlayers++;
        }

        UpdateAIForTeam(Team.Red);
        UpdateAIForTeam(Team.Blue);

		StartBallTimer();
	}

    void UpdateAIForTeam(Team team)
    {
        List<GameObject> list = GetPlayersInTeam(team);
        int count = 0;
        foreach (GameObject obj in list)
        {
            CharacterController controller = obj.GetComponent<CharacterController>();
            AIInputController ai = controller as AIInputController;
            if (ai)
            {
                ai.AIDifficulty = GameDifficulty;
                ai.Defensive = (count == 0);
                count++;
            }

        }
    }
	
	//Unity Singleton pattern
	void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else Destroy(this); // or gameObject
	}
	
	public void GivePointTo(Team team)
	{
		audioSource.PlayOneShot(goalSound, 0.7f);
		score[(int)team]++;
		UpdateScoreboard();

        if (team == Team.Red)
        {
            GameObject clone = Instantiate(Resources.Load("Prefabs/Fx_score"), new Vector3(-5.75f, 0, 0), Quaternion.identity) as GameObject;
            clone.SetActive(true);
        }
        else
        {
            GameObject clone = Instantiate(Resources.Load("Prefabs/Fx_score"), new Vector3(5.75f, 0, 0), Quaternion.identity) as GameObject;
            clone.SetActive(true);
        }

        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>().StartShake(0.3f, 0.2f);

		DestroyBall();

        if (!IsGameDone())
		    StartBallTimer();
	}
	
	void DestroyBall()
	{
		DestroyObject(ball);
	}
	
	void StartBallTimer()
	{
		state = STATE.StartTimer;
		startTimer = COUNTDOWN_STEPS + 1;
		startTimerSoundNbPlayed = COUNTDOWN_STEPS;
	}
	
	private void UpdateScoreboard()
	{
		scoreboard.text = score[0] + " : " + score[1];
	}
	
	private void UpdateTimerText()
	{
		string[] names = new string[4] {"UI_countdowngo", "UI_countdown1", "UI_countdown2", "UI_countdown3"};
		for (int i = 0; i < 4; ++i)
			GameObject.Find (names[i]).GetComponent<SpriteRenderer>().enabled = false;
		
		float scale = (startTimer - Mathf.Floor (startTimer)) * 1.0f;
		if(startTimer > 1)
		{
			int time = (int)Mathf.Floor (startTimer);
			GameObject.Find (names[time]).GetComponent<SpriteRenderer>().enabled = true;

			if ((int)startTimer == startTimerSoundNbPlayed)
			{
				audioSource.PlayOneShot(timerSound, 1.0f);
				startTimerSoundNbPlayed--;
			}
		}
		else if (startTimer > 0)
		{
			GameObject.Find (names[0]).GetComponent<SpriteRenderer>().enabled = true;
			if ((int)startTimer == startTimerSoundNbPlayed)
			{
				audioSource.PlayOneShot(timerGoSound, 1.0f);
				startTimerSoundNbPlayed--;
			}
		}
		else
		{
			GameObject.Find (names[0]).GetComponent<SpriteRenderer>().enabled = false;
		}
	}
	
	private bool IsControllerActive(Controller controller)
	{
		return (1<<(int)controller & playerControllerMask) != 0;
	}
	
	private void SetActiveController(Controller controller)
	{
		playerControllerMask |= (1 << (int)controller);
	}

	private Material GetTeamMaterial(Team team, bool isFirstPlayer)
	{
		if (team == Team.Red)
			return isFirstPlayer ? MaterialTeamRed : MaterialTeamRed2;
		else
            return isFirstPlayer ? MaterialTeamBlue : MaterialTeamBlue2;
	}

	private GameObject GetPlayerFromController(Controller controller)
	{
		return players.Where(p => p != null && p.GetComponent<PlayerInputController>().Controller == controller).FirstOrDefault();
	}
	
	private Team GetSmallestTeam()
	{
		var nbRedPlayers = players.Count(p => p != null && p.GetComponent<Player>().team == Team.Red);
		var nbBluePlayers = players.Count(p => p != null && p.GetComponent<Player>().team == Team.Blue);
		return nbBluePlayers < nbRedPlayers ? Team.Blue : Team.Red;
	}
	
	private List<GameObject> GetPlayersInTeam(Team team)
	{
		return players.Where(p => p != null && p.GetComponent<Player>().team == team).ToList();
	}

	private int GetNbPlayers()
	{
		return players.Count(p => p != null);
	}

	private void ResetPlayersPositions()
	{
		for (int i = 0; i < GetNbPlayers(); i++)
		{
			spawnController.MovePlayerToHisSpawn(players[i].GetComponent<Player>());
		}
	}
}
