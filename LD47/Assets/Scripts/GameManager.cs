using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get => _instance; }
    private static GameManager _instance;

    public Transform ball;
    public float resetMovementSpeed = 5f;

    public bool PlayerControllable { get => playerControllable; }
    private bool playerControllable = true;

    public int round = 1;
    public int score = 0;
    public int scoreLimit = 0;
    public float roundDuration = 6;

    public TextMeshProUGUI roundText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI finalScoreText;
    public GameObject gameOverPanel;

    private float roundTimer;
    private Goal[] goals;

    private enum GameState
    {
        Setup,
        RoundRunning,
        GoalScored,
        RoundEnded,
        GameOver
    }
    private GameState gameState = GameState.Setup;

    // replay stuff
    public int playerId = 0;
    private char input;
    private char currentCommand;
    private bool[] active = new bool[6];
    private char[] command = new char[6];
    private int[] duration = new int[6];
    private int[] inputIndex = new int[6];
    private char[,] inputCommand = new char[6, 255];
    private int[,] inputDuration = new int[6, 255];

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);

        //Init the Foes
        for(int i = 0 ; i < 6 ; i++ ){
            
            //Disable it
            active[i] = false;
            
            //Set its current SCB
            // if( i == 0 ){ foes.sprite[i]=&spr_foe0; }
            // else if( i == 1 ){ foes.sprite[i]=&spr_foe1; }
            // else if( i == 2 ){ foes.sprite[i]=&spr_foe2; }
            // else if( i == 3 ){ foes.sprite[i]=&spr_foe3; }
            // else if( i == 4 ){ foes.sprite[i]=&spr_foe4; }
            // else if( i == 5 ){ foes.sprite[i]=&spr_foe5; }
            // else if( i == 6 ){ foes.sprite[i]=&spr_foe6; }
            // else if( i == 7 ){ foes.sprite[i]=&spr_foe7; }
            
            //Hides offscreen
            // foes.x[i]=0;
            // foes.y[i]=130;
            // foes.speedX[i]=0;
            // foes.speedY[i]=0;
            
            // //Update the sprites too
            // foes.sprite[i]->hpos=foes.x[i];
            // foes.sprite[i]->vpos=foes.y[i]-1;
            
            //Clear the inputs recorded variables
            command[i] = (char)0;
            duration[i] = 0;
            //Clear the inputs recorded
            for(int j = 0 ; j < 255 ; j++ ){
                inputCommand[i, j] = (char)0;
                inputDuration[i, j] = 0;
            }
        }

        goals = FindObjectsOfType<Goal>();
        ActivateGoalAtRandom();
    }

    void Start()
    {
        StartCoroutine(InitRoundRoutine());
    }

    char GetInput()
    {
        if (Input.GetKey(KeyCode.W)) return 'w';
        if (Input.GetKey(KeyCode.S)) return 's';
        if (Input.GetKey(KeyCode.A)) return 'a';
        if (Input.GetKey(KeyCode.D)) return 'd';
        return '_';
    }

    void Update()
    {
        if (gameState == GameState.RoundRunning)
        {
            // check inputs and record it
            input = GetInput(); 

            //If the input is the same as before, and its current duration is below 255 (max duration time for a given command!)
            if (inputCommand[playerId, inputIndex[playerId]] == input && inputDuration[playerId, inputIndex[playerId]] < 255)
            {
                //Increase current command duration time
                inputDuration[playerId, inputIndex[playerId]]++;
            }
            //Else, create a new command entry
            else 
            {
                //Increase input table index pointer
                inputIndex[playerId]++;
                
                //And create a new command with the current player Input
                inputCommand[playerId, inputIndex[playerId]] = input;
                //And set it's duration to 0 frame (which will be replayed back as "1 frame" in fact :))
                inputDuration[playerId, inputIndex[playerId]] = 0;
            }
            
            //For each FOE
            for (int i = 0 ; i < 6 ; i++) {
                
                //If the foe is active
                if(active[i]){
                    
                    //Read it's current command if it's a "ghost", else use the player command :)
                    if( i == playerId ){
                        //Use current player input
                        currentCommand = input;
                    }
                    //Else, read the command back from the Foe table
                    else {
                        //Use the currently active command for the foe
                        currentCommand = command[i];
                        
                        //If the current command still have some duration left
                        if(duration[i] > 0 ){
                            //Simply decrease the current command remaining duration
                            --duration[i];
                        }
                        //Else, increase command reading index to the next command in list for the next frame
                        else {
                            //Move the index to the next input command to read
                            ++inputIndex[i];
                            //And store this command in the current "command"
                            command[i] = inputCommand[i, inputIndex[i]];
                            //And set the current "TimeToLive" of this command
                            duration[i] = inputDuration[i, inputIndex[i]];
                        }
                    }
                }
            }

            if (roundTimer >= 0f)
                roundTimer -= Time.deltaTime;
            else 
                StartCoroutine(EndRoundRoutine());
        }

        UpdateUI();
    }

    void ActivateGoalAtRandom()
    {
        int randomGoal = Random.Range(0, goals.Length);
        for (int i=0; i<goals.Length; i++)
        {
            goals[i].gameObject.SetActive(i == randomGoal);
        }
    }

    IEnumerator InitRoundRoutine()
    {
        yield return StartCoroutine(ResetBall());

        ActivateGoalAtRandom();

        round++;
        roundTimer = roundDuration;

        if (round > 30) 
        {
            scoreLimit += 2;
        } 
        // increase by 2 every 2 rounds
        else if(round > 15 && (round & 1) == 0)
        {
            scoreLimit += 2;
        } 
        // increase by 1 every round
        else
        {
            scoreLimit++;
        }

        playerControllable = true;

        gameState = GameState.RoundRunning;
    }

    IEnumerator EndRoundRoutine()
    {
        gameState = GameState.RoundEnded;
        playerControllable = false;

        if (IsGameOver())
        {
            GameOver();
        }
        else 
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(InitRoundRoutine());
        }

        yield return null;
    }

    IEnumerator ResetBall(float delay = 0f)
    {
        // disable physics
        ball.GetComponent<Collider2D>().enabled = false;
        ball.GetComponent<Rigidbody2D>().simulated = false;
        ball.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        
        yield return new WaitForSeconds(delay);
        
        float distToSpawn = 1f;
        Vector3 ballSpawn = Vector3.zero;
        while (distToSpawn > 0.01f)
        {
            ball.position = Vector3.Lerp(ball.position, ballSpawn, resetMovementSpeed * Time.deltaTime);
            
            distToSpawn = Vector3.Distance(ballSpawn, ball.position);

            yield return null;
        }

        ball.position = ballSpawn;

        // enable physics again
        ball.GetComponent<Collider2D>().enabled = true;
        ball.GetComponent<Rigidbody2D>().simulated = true;
    }

    public void GoalScored()
    {
        StartCoroutine(GoalScoredRoutine());
    }

    IEnumerator GoalScoredRoutine()
    {
        gameState = GameState.GoalScored;   
        score++;
        
        playerControllable = false;

        yield return StartCoroutine(ResetBall(1f));

        playerControllable = true;

        gameState = GameState.RoundRunning;
    }

    bool IsGameOver()
    {
        return score < scoreLimit;
    }

    void GameOver()
    {
        gameState = GameState.GameOver;
    }

    public void RestartGame()
    {
        gameState = GameState.RoundRunning;
        roundTimer = roundDuration;
        score = 0;
        round = 1;
        playerControllable = true;
        ball.GetComponent<Ball>().ResetBall();
        ActivateGoalAtRandom();
    }

    void UpdateUI()
    {
        scoreText.text = "SCORE: " + score + " / " + scoreLimit;
        roundText.text = "ROUND: " + round;
        countdownText.text = "" + Mathf.Round(roundTimer);

        gameOverPanel.SetActive(gameState == GameState.GameOver);
        if (gameState == GameState.GameOver)
            finalScoreText.text = "Final score: " + score;
    }
}
