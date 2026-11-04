using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Objective")]
    [SerializeField] int targetScore = 10;

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
    [SerializeField] TMP_Text resultText;
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
        UpdateUI();
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
        if (score >= targetScore) Win();
    }

    public void SetBonusTotal(int total) => bonusTotal = total;

    public void AddBonus(int amount)
    {
        if (gameAlreadyFinished) return;
        bonusCollected += amount;
        if (bonusText == null) return;
        bonusText.gameObject.SetActive(true);
        bonusText.text = $"BonusCoins: {bonusCollected}/{bonusTotal}";
        PunchText(bonusText);
    }

    public void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score} / {targetScore}";
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
        if (lives <= 0) Lose();
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
            resultText.text = $"Score: {score} / {targetScore}\nBonusCoins: {bonusCollected}/{bonusTotal}";
        winPanel.SetActive(true);
        Time.timeScale = 0f;

        Vector3 celebrationCenter = (player != null ? player.transform.position : Vector3.zero) + Vector3.up;
        JuiceFx.Burst(celebrationCenter, JuiceFx.GoldColor, 70, 9f, 0.25f, 1.6f, true, 1.2f);
        JuiceFx.Burst(celebrationCenter, JuiceFx.SpeedColor, 50, 8f, 0.22f, 1.6f, true, 1.2f);
        JuiceFx.Burst(celebrationCenter, JuiceFx.HeartColor, 50, 7f, 0.22f, 1.6f, true, 1.2f);
        JuiceFx.Ring(celebrationCenter, JuiceFx.GoldColor, 7f, 0.9f, true);
    }

    void Lose()
    {
        if (gameAlreadyFinished) return;
        gameAlreadyFinished = true;
        losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}