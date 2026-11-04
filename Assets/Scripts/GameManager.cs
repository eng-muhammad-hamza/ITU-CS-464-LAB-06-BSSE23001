using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Objective { CollectCoins, ReachGoal, CollectKeys }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Objective")]
    [SerializeField] Objective objective = Objective.CollectCoins;
    [SerializeField] int targetScore = 10;
    [SerializeField] string objectiveLabel = "Reach the goal!";
    [SerializeField] int keysRequired = 3;

    [Header("Timer (0 = no timer)")]
    [SerializeField] float timeLimit = 0f;
    [SerializeField] float bonusTimeReward = 10f;

    [Header("Lives and respawn")]
    [SerializeField] int lives = 3;
    [SerializeField] float respawnDelay = 0.6f;
    [SerializeField] float spawnProtection = 1.5f;
    [SerializeField] Health player;
    [SerializeField] Transform respawnPoint;

    [Header("UI")]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text bonusText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text loseReasonText;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;

    [Header("Optional red-coin bonus (leave empty to disable)")]
    [SerializeField] Spawner spawner;

    int score;
    int bonusCollected;
    int bonusTotal;
    int redCoinTriggerScore;
    bool redCoinAlreadySpawned;
    bool gameAlreadyFinished;
    float timeLeft;
    int keysCollected;
    public bool ExitUnlocked => objective != Objective.CollectKeys || keysCollected >= keysRequired;

    public bool IsFinished => gameAlreadyFinished;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (spawner != null) redCoinTriggerScore = Random.Range(1, Mathf.Max(2, targetScore));
        if (bonusText != null) bonusText.gameObject.SetActive(false);
        timeLeft = timeLimit;
        if (timerText != null)
        {
            timerText.gameObject.SetActive(timeLimit > 0f);
            UpdateTimerText();
        }
        UpdateUI();
    }

    void Update()
    {
        if (timeLimit <= 0f || gameAlreadyFinished) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateTimerText();
            Lose("Time's up!");
            return;
        }
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        if (timerText == null) return;
        timerText.text = $"Time: {timeLeft:0.0}";
        timerText.color = timeLeft < 10f ? JuiceFx.DamageColor : Color.white;
    }

    public void AddTime(float seconds)
    {
        if (gameAlreadyFinished || timeLimit <= 0f) return;
        timeLeft += seconds;
        UpdateTimerText();
        PunchText(timerText);
    }

    public void ReachGoal()
    {
        if (objective == Objective.CollectCoins || !ExitUnlocked) return;
        Win();
    }

    public void AddScore(int amount)
    {
        if (gameAlreadyFinished) return;
        score += amount;
        UpdateUI();
        PunchText(scoreText);
        if (spawner != null && !redCoinAlreadySpawned && score >= redCoinTriggerScore)
        {
            redCoinAlreadySpawned = true;
            spawner.SpawnRed();
        }
        if (objective == Objective.CollectCoins && score >= targetScore) Win();
    }

    public void SetBonusTotal(int total) => bonusTotal = total;

    public void AddBonus(int amount)
    {
        if (gameAlreadyFinished) return;
        bonusCollected += amount;

        if (bonusTotal > 0 && bonusCollected == bonusTotal && timeLimit > 0f)
        {
            AddTime(bonusTimeReward);
            if (player != null)
                JuiceFx.FloatingText(player.transform.position + Vector3.up * 1.6f,
                                     $"ALL! +{bonusTimeReward:0}s", JuiceFx.TimeColor, 20f);
        }

        if (bonusText == null) return;
        bonusText.gameObject.SetActive(true);
        bonusText.text = $"BonusCoins: {bonusCollected}/{bonusTotal}";
        PunchText(bonusText);
    }

    public void UpdateUI()
    {
        if (scoreText != null)
        {
            if (objective == Objective.CollectCoins) scoreText.text = $"Score: {score} / {targetScore}";
            else if (objective == Objective.CollectKeys) scoreText.text = $"Keys: {keysCollected} / {keysRequired}";
            else scoreText.text = objectiveLabel;
        }
        if (livesText != null && player != null)
            livesText.text = $"Lives: {lives}   Hearts: {Mathf.Max(player.Current, 0)} / {player.Max}";
    }

    static void PunchText(TMP_Text text)
    {
        if (text != null && text.TryGetComponent(out UiPunch punch)) punch.Punch();
    }

    public void PlayerDied()
    {
        if (gameAlreadyFinished) return;
        lives--;
        UpdateUI();
        if (lives <= 0) Lose("Out of lives!");
        else StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        if (gameAlreadyFinished) yield break;
        Rigidbody body = player.GetComponent<Rigidbody>();
        body.position = respawnPoint.position;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        player.Restore(spawnProtection);
    }

    void Win()
    {
        if (gameAlreadyFinished) return;
        gameAlreadyFinished = true;
        if (resultText != null)
        {
            string summary;
            if (objective == Objective.CollectCoins) summary = $"Score: {score} / {targetScore}";
            else if (objective == Objective.CollectKeys) summary = $"Keys: {keysCollected} / {keysRequired}\nLives left: {lives}";
            else summary = timeLimit > 0f ? $"Time left: {timeLeft:0.0} s" : "Goal reached!";
            if (bonusTotal > 0) summary += $"\nBonusCoins: {bonusCollected}/{bonusTotal}";
            resultText.text = summary;
        }
        winPanel.SetActive(true);
        Time.timeScale = 0f;

        Vector3 celebrationCenter = (player != null ? player.transform.position : Vector3.zero) + Vector3.up;
        JuiceFx.Burst(celebrationCenter, JuiceFx.GoldColor, 70, 9f, 0.25f, 1.6f, true, 1.2f);
        JuiceFx.Burst(celebrationCenter, JuiceFx.SpeedColor, 50, 8f, 0.22f, 1.6f, true, 1.2f);
        JuiceFx.Burst(celebrationCenter, JuiceFx.HeartColor, 50, 7f, 0.22f, 1.6f, true, 1.2f);
        JuiceFx.Ring(celebrationCenter, JuiceFx.GoldColor, 7f, 0.9f, true);
    }

    void Lose(string reason)
    {
        if (gameAlreadyFinished) return;
        gameAlreadyFinished = true;
        if (loseReasonText != null) loseReasonText.text = reason;
        losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings) next = 0;
        SceneManager.LoadScene(next);
    }

    public void AddKey()
    {
        if (gameAlreadyFinished) return;
        keysCollected++;
        UpdateUI();
        PunchText(scoreText);
        if (player != null)
        {
            string message = keysCollected >= keysRequired ? "EXIT OPEN!" : $"KEY {keysCollected}/{keysRequired}";
            JuiceFx.FloatingText(player.transform.position + Vector3.up * 1.6f, message, JuiceFx.KeyColor, 20f);
        }
    }
}