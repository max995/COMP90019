/*
 * The GameManager controls logic of the game, singleton
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Current turn is player's turn or AI's turn
    [HideInInspector] public bool playerTurn;
    // Game over signal
    [HideInInspector] public bool gameOver;
    // AI gonna rotation when it's true
    [HideInInspector] public bool AICelebrate;
    //    [HideInInspector] SpriteRenderer sr ;
    [HideInInspector] public Vector3[] trueAnchorPos;
    [HideInInspector] public bool firstBlock;
    [HideInInspector] public int Unblocked_Reds;
    [HideInInspector] public int Total_RealPath_Blocks;
    [HideInInspector] public int Total_RealPath_Blocks_Narrative;
    [HideInInspector] public int Total_FalsePath_Blocks;
    [HideInInspector] public int Total_Blocks;
    [HideInInspector] public int Task1_a;
    [HideInInspector] public int Task1_b;
    [HideInInspector] public int Real_Path_Replan;


    public GameObject gridTile;
    public GameObject Anchor;
    public GameObject OnAnchor;
    public GameObject[] GeneratorsImages;
    public GameObject[] pickupTiles;
    public GameObject[] counterTiles;
    public GameObject[] counterOnShuttleTiles;
    public GameObject AI;

    // Stores what happened in the game
    [HideInInspector] public string gameLog;
    [HideInInspector] public string redToken;


    // Red, White, Blue, Yellow Generators
    [HideInInspector] public List<GameObject> generators = new List<GameObject>();
    // Parking position of each generator above
    [HideInInspector] public List<Vector3> parkingPos = new List<Vector3>();
    // The center position of each anchor
    [HideInInspector] public List<Vector3> anchorPositions = new List<Vector3>();
    // The colors of deposited counters, deposited[x][y] == -1 means empty grid
    [HideInInspector] public List<List<int>> deposited = new List<List<int>>();
    //recof all the deposited red token
    [HideInInspector] public List<Vector3> depositRed = new List<Vector3>();
    // blocked[x][y] == true: (x, y) has been blocked
    [HideInInspector] public List<List<bool>> blocked = new List<List<bool>>();
    [HideInInspector] public List<Vector3> blockedTile = new List<Vector3>();
    // Use to prevent counter turned over immediately after deposit
    [HideInInspector] public List<List<bool>> readyToTurnOver = new List<List<bool>>();
    // Store GameObjects of counters deposited on the board
    [HideInInspector] public List<List<GameObject>> countersOnBoard = new List<List<GameObject>>();
    [HideInInspector] public Vector3 anchor_a1;
    [HideInInspector] public Vector3 anchor_a2;
    [HideInInspector] public Vector3 anchor_a3;
    [HideInInspector] public Vector3 anchor_a4;
    [HideInInspector] public List<Vector3> path_current;
    [HideInInspector] public List<Vector3> path_a;
    [HideInInspector] public List<Vector3> path_b;
    [HideInInspector] public int pathChange;

    // Board Generator, creates the board and generators
    private BoardGenerator boardScript;
    // AI Manager, controls AI moves
    private AIManager aiScript;

    //private AutoHuma ah;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Initialize the whole game
    private void Initialize()
    {
        gameLog = "";
        redToken = "";
       
        gameOver = false;
        AICelebrate = false;
        firstBlock = false;
        Unblocked_Reds = 0;
        Total_RealPath_Blocks=0;
        Total_FalsePath_Blocks=0;
        Total_RealPath_Blocks_Narrative = 0;
        Total_Blocks=0;
        Task1_a = 0;
        Task1_b = 0;
        pathChange = 0;
        Real_Path_Replan = 0;
        SetPlayerTurn(false);
        boardScript = GetComponent<BoardGenerator>();
        boardScript.SetupScene();
        anchor_a1 = new Vector3();
        anchor_a2 = new Vector3();
        anchor_a3 = new Vector3();
        anchor_a4 = new Vector3();
        //Methods.instance.Task1Anchor(anchorPositions, out Task1_a, out Task1_b);
        //Debug.Log("a cost is:" + Task1_a);
        //Debug.Log("b cost is:" + Task1_b);
        aiScript = GetComponent<AIManager>();
        aiScript.InitialiseAIs();
        InitialiseDeposited();
        //intillise before using
        Methods.instance.Task1Anchor(GameManager.instance.anchorPositions, out GameManager.instance.Task1_a, out GameManager.instance.Task1_b);
        
        //Methods.instance.Task1Anchor(anchorPositions, out Task1_a, out Task1_b);
        //Debug.Log("a cost is:" + Task1_a);
        //Debug.Log("b cost is:" + Task1_b);
        //sr = GetComponent<SpriteRenderer>();
        //test();
        //ah = GetComponent<AutoHuma>();
        //ah.InitialiseAH();
        trueAnchorPos = new Vector3[2];
        gameLog += "--- Game Start ---\n";
        gameLog += "--- Color: 0-RED , 1- Yellow, 2- Blue---\n";

        
    }


    



    // Start AI's turn
    public IEnumerator TurnSwitch()
    {
        if (!gameOver)
        {
            for (int i = 0; i < generators.Count; i++)
            {
                if (!generators[i].GetComponent<GeneratorManager>().visitThisTurn)
                {
                    generators[i].GetComponent<GeneratorManager>().idleTurnCount += 1;
                }
            }
            for (int i = 0; i < generators.Count; i++)
            {
                generators[i].GetComponent<GeneratorManager>().visitThisTurn = false;
            }
            yield return StartCoroutine(GetComponent<UIManager>().ShowAITurn());

            aiScript.AITurn();
            
         
        }
    }

    // Check if there is a red path between two anchors
    public bool CheckGameOver()
    {
        foreach (Vector3 position in anchorPositions)
        {
            if (Methods.instance.BFStoAnotherAnchor(new Vector3(position.x - 0.5f, position.y - 0.5f, 0f)))
            {
                gameOver = true;
                return true;
            }
        }
        return false;
    }

    // Show AI Win and write the result to gameLog
    public void GameOverAIWin()
    {
        gameLog += "--- AI Win ---\n";
        gameLog += "AI Turn Count: " + aiScript.turnCount + "\n";
        gameLog += "Remaining Time For AI: " + (GetComponent<UIManager>().AITimeLimit - (Time.time - GetComponent<UIManager>().startTime)) + " seconds\n";
        gameOver = true;
        AICelebrate = true;
        GetComponent<UIManager>().ShowAIWinText();
        gameLog += "Deposit in total real path is " + GameManager.instance.Total_RealPath_Blocks + "\n";
        gameLog+= "Deposit in total real narrative is " + GameManager.instance.Total_RealPath_Blocks_Narrative + "\n";
        gameLog += "Deposit in total blocks is " + GameManager.instance.Total_Blocks + "\n";
        gameLog += "Deposit in total false blocks is " + GameManager.instance.Total_FalsePath_Blocks + "\n";
        //gameLog += "the ratio is:" + Methods.instance.Ratio(GameManager.instance.Total_RealPath_Blocks, GameManager.instance.Total_Blocks)+ "\n";

        //calculate
        //Methods.instance.Ratio(GameManager.instance.Total_RealPath_Blocks,GameManager.instance.Total_Blocks);
        //Debug.Log("the ratio is:"+ Methods.instance.Ratio(GameManager.instance.Total_RealPath_Blocks, GameManager.instance.Total_Blocks));
        Methods.instance.TurnAllWhiteCounterOver();
        SendToServer();
    }

    // Show Player Win and write the result to gameLog
    public void GameOverPlayerWin()
    {
        gameLog += "Time Out!\n";
        gameLog += "--- Player Win ---\n";
        gameLog += "Deposit red before the first block:" + GameManager.instance.Unblocked_Reds + "\n";
        gameLog += "Deposit in total real path is " + GameManager.instance.Total_RealPath_Blocks + "\n";
        gameLog += "Deposit in total real narrative is " + GameManager.instance.Total_RealPath_Blocks_Narrative + "\n";
        gameLog += "Deposit in total blocks is " + GameManager.instance.Total_Blocks + "\n";
        gameLog += "Deposit in total blocks is " + GameManager.instance.Total_FalsePath_Blocks + "\n";
        //gameLog += "the ratio is:" + Methods.instance.Ratio(GameManager.instance.Total_RealPath_Blocks, GameManager.instance.Total_Blocks)+ "\n";
        gameLog += "AI Turn Count: " + aiScript.turnCount + "\n";
        
        gameOver = true;
        GetComponent<UIManager>().ShowPlayerWinText();
        Methods.instance.TurnAllWhiteCounterOver();
        SendToServer();
    }

    // Initializes lists which use to record the game state
    private void InitialiseDeposited()
    {
        deposited.Clear();
        depositRed.Clear();
        readyToTurnOver.Clear();
        blocked.Clear();
        countersOnBoard.Clear();
        for (int x = 0; x < GameParameters.instance.gridSize; x++)
        {
            deposited.Add(new List<int>());
            readyToTurnOver.Add(new List<bool>());
            blocked.Add(new List<bool>());
            countersOnBoard.Add(new List<GameObject>());
            for (int y = 0; y < GameParameters.instance.gridSize; y++)
            {
                deposited[x].Add(-1);
                readyToTurnOver[x].Add(false);
                blocked[x].Add(false);
                countersOnBoard[x].Add(null);
            }
        }
    }

    public void SetPlayerTurn(bool turn)
    {
        playerTurn = turn;
    }

    private void DestroyObjects(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }

    // Clear objects and initialize a new game
    public void RestartGame()
    {
        foreach (GameObject obj in generators)
        {
            Destroy(obj);
        }
        GameObject[] objects;
        objects = GameObject.FindGameObjectsWithTag("PickUp");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Counter");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("WhiteCounter");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Shuttle");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("OnShuttle");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("WhiteOnShuttle");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Floor");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Anchor");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Board");
        DestroyObjects(objects);
        Methods.instance.InitializeMethods();
        Initialize();
        GetComponent<UIManager>().Initialize();
        StartCoroutine(TurnSwitch());
    }

    // Sends the gameLog to Server after gameover
    private void SendToServer()
    {
        StartCoroutine(SendLogToServer());
    }

    IEnumerator SendLogToServer()
    {
        WWWForm form = new WWWForm();
        form.AddField("time", DateTime.Now.ToString() + "\n");
        form.AddField("log", gameLog);    
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:1234/logFile.php", form))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Upload complete!");
            }
        }
    }

    // Called when the scene loaded
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            RestartGame();
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
