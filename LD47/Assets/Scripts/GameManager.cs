using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CommandHistoryEntry 
{
    public InputCommand inputCommand;
    public float inputDuration;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get => _instance; }
    private static GameManager _instance;

    public Transform ball;
    public float resetMovementSpeed = 5f;

    public bool PlayerControllable { get => playerControllable; }
    private bool playerControllable = true;

    private int round = 0;
    private int score = 0;
    private int scoreLimit = 0;
    public float roundDuration = 6;

    public TextMeshProUGUI roundText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI finalScoreText;
    public GameObject gameOverPanel;
    public GameObject titleScreen;
    public TextMeshProUGUI roundTransitionText;

    private float roundTimer;

    private enum GameState
    {
        Menu,
        RoundRunning,
        GoalScored,
        RoundEnded,
        GameOver
    }
    private GameState gameState = GameState.Menu;

    // replay stuff
    private int playerId = 255;
    private InputCommand input;
    private InputCommand currentCommand;
    private Dictionary<int, List<CommandHistoryEntry>> inputCommand = new Dictionary<int, List<CommandHistoryEntry>>();

    public BotController[] bots;
    private int botsMax = 0;
    public Transform[] spawnPoints;

    public Goal[] goals;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        // init the bots
        for (int i = 0; i < bots.Length; i++)
        {
            bots[i].ResetBot();

            // clear the inputs recorded
            inputCommand.Add(i, new List<CommandHistoryEntry>());
        }
    }

    public void StartGame()
    {
        ActivateGoalAtRandom();

        StartCoroutine(InitRoundRoutine());

        titleScreen.SetActive(false);
    }

    InputCommand GetInput()
    {
        InputCommand c = new InputCommand();
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) c.InputY = 1;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) c.InputY = -1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) c.InputX = -1;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) c.InputX = 1;

        if (Input.GetKey(KeyCode.Space)) c.InputDash = true;

        return c;
    }

    void Update()
    {
        if (gameState == GameState.RoundRunning && playerId < 255)
        {
            input = GetInput();

            // if the input is the same as before
            if (bots[playerId].inputIndex < inputCommand[playerId].Count &&
                inputCommand[playerId][bots[playerId].inputIndex].inputCommand == input)
            {
                // increase current command duration time
                inputCommand[playerId][bots[playerId].inputIndex].inputDuration += Time.deltaTime;
            }
            else
            {
                bots[playerId].inputIndex++;
                inputCommand[playerId].Add(new CommandHistoryEntry {
                    inputCommand = input,
                    inputDuration = 0
                });
            }

            for (int i = 0; i < bots.Length; i++)
            {
                // if the foe is active
                if (bots[i].active)
                {
                    // read it's current command if it's a "ghost", else use the player command :)
                    if (i == playerId)
                    {
                        // use current player input
                        currentCommand = input;
                    }
                    // else, read the command back from the Foe table
                    else
                    {
                        // use the currently active command for the foe
                        currentCommand = bots[i].command;

                        // if the current command still have some duration left
                        if (bots[i].duration > 0)
                        {
                            // simply decrease the current command remaining duration
                            bots[i].duration -= Time.deltaTime;
                        }
                        // else, increase command reading index to the next command in list for the next frame
                        else
                        {
                            // move the index to the next input command to read
                            bots[i].inputIndex++;

                            if (bots[i].inputIndex < inputCommand[i].Count)
                            {
                                // set the next input command
                                bots[i].command = inputCommand[i][bots[i].inputIndex].inputCommand;
                                bots[i].duration = inputCommand[i][bots[i].inputIndex].inputDuration;
                            }
                        }
                    }
                
                    // dashing
                    if (currentCommand.InputDash)
                    {
                        if (bots[i].dashTime <= 0)
                        {
                            bots[i].dashTime = 0.5f;
                        }
                    }
                    else
                    {
                        if (bots[i].dashTime > 0)
                            bots[i].dashTime -= Time.deltaTime;
                    }

                    // moving left
                    if (currentCommand.InputX < 0)
                    {
                        bots[i].MoveX(-1);
                    }
                    // moving right
                    else if (currentCommand.InputX > 0)
                    {
                        bots[i].MoveX(1);
                    }

                    // moving up
                    if (currentCommand.InputY > 0)
                    {
                        bots[i].MoveY(1);
                    }
                    // moving down
                    else if (currentCommand.InputY < 0)
                    {
                        bots[i].MoveY(-1);
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

        // first round, tutorial top
        if (round == 0) 
        {   
            randomGoal = 0;
        } 
        else if (round == 1) 
        {   
            randomGoal = 1;
        }

        for (int i = 0; i < goals.Length; i++)
        {
            goals[i].gameObject.SetActive(i == randomGoal);
        }
    }

    IEnumerator InitRoundRoutine()
    {
        yield return StartCoroutine(ResetBall());

        if (playerId != 255)
            yield return new WaitForSeconds(1f);

        ActivateGoalAtRandom();

        round++;
        roundTimer = roundDuration;

        // adjust scorelimit
        if (round > 30)
        {
            scoreLimit += 2;
        }
        // increase by 2 every 2 rounds
        else if (round > 15 && (round & 1) == 0)
        {
            scoreLimit += 2;
        }
        // increase by 1 every round
        else
        {
            scoreLimit++;
        }

        // reset the bots
        for (int i = 0; i < bots.Length; i++)
        {
            bots[i].ResetBot();
        }

        // increase the playerID so he'll control another bot this time!
        playerId++;
        if (playerId > 5)
        {
            playerId = 0;
        }

        bots[playerId].ChangeAppearanceToPlayer();

        if (botsMax < 6) 
        { 
            botsMax++;
        }

        // activate bots to the current maximum
        for (int i = 0; i < botsMax; i++)
        {
            bots[i].active = true;
            bots[i].transform.position = spawnPoints[i].position;
        }

        if (round > 1)
            yield return new WaitForSeconds(1f);

        // show round end text
        roundTransitionText.gameObject.SetActive(true);
        roundTransitionText.text = "GET READY";
        roundTransitionText.GetComponent<Animator>().Play("round_over");

        yield return new WaitForSeconds(2f);

        playerControllable = true;

        gameState = GameState.RoundRunning;

        roundTransitionText.text = "GO!";

        yield return new WaitForSeconds(1f);

        roundTransitionText.gameObject.SetActive(false);
    }

    IEnumerator EndRoundRoutine()
    {
        gameState = GameState.RoundEnded;
        playerControllable = false;

        ball.GetComponent<Rigidbody2D>().simulated = false;
        ball.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        if (IsGameOver())
        {
            GameOver();
        }
        else
        {
            // show round end text
            roundTransitionText.gameObject.SetActive(true);
            roundTransitionText.text = "ROUND OVER";
            roundTransitionText.GetComponent<Animator>().Play("round_over");

            yield return new WaitForSeconds(1f);

            // move active bots to it's spawnposition
            for (int i=0; i<bots.Length; i++)
            {
                if (bots[i].active)
                    yield return StartCoroutine(bots[i].MoveToSpawn(spawnPoints[i].position));
                else
                    yield return null;
            }

            StartCoroutine(InitRoundRoutine());
        }
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
        float animStep = 0f;
        while (distToSpawn > 0.01f)
        {
            animStep += 2f * Time.deltaTime;
            ball.position = Vector3.Lerp(ball.position, ballSpawn, animStep);

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
        if (gameState == GameState.RoundRunning)
            StartCoroutine(GoalScoredRoutine());
    }

    IEnumerator GoalScoredRoutine()
    {
        gameState = GameState.GoalScored;
        score++;

        bots[playerId].dashTime = 0;

        playerControllable = false;

        yield return StartCoroutine(ResetBall(0.5f));

        if (gameState == GameState.GoalScored) {
            playerControllable = true;

            gameState = GameState.RoundRunning;
        }
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
        gameState = GameState.RoundEnded;
        roundTimer = roundDuration;
        score = 0;
        round = 0;
        scoreLimit = 0;
        playerControllable = true;
        ball.GetComponent<Ball>().ResetBall();

        botsMax = 0;
        playerId = 255;

        // init the bots
        for (int i = 0; i < bots.Length; i++)
        {
            bots[i].ResetBot();

            // clear the inputs recorded
            inputCommand[i].Clear();
        }

        StartCoroutine(InitRoundRoutine());
    }

    void UpdateUI()
    {
        scoreText.text = "SCORE: " + score + " / " + scoreLimit;
        roundText.text = "ROUND: " + round;
        countdownText.text = System.Math.Round(roundTimer, 2).ToString("0.0");

        gameOverPanel.SetActive(gameState == GameState.GameOver);
        if (gameState == GameState.GameOver)
            finalScoreText.text = "YOUR SCORE: " + score;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
