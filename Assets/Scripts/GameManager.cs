using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] int targetScore = 10;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject winPanel;
    [SerializeField] TMP_Text bonusText;
    [SerializeField] TMP_Text resultText;
    [SerializeField] Spawner spawner;
    int score;
    int bonus;
    int bonusTotal;
    int redTriggerScore;
    bool redSpawned;

    void Awake() => Instance = this;

    void Start()
    {
        redTriggerScore = Random.Range(1, targetScore);
        Debug.Log($"Red coin will spawn at score {redTriggerScore}");
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();

        if (!redSpawned && score >= redTriggerScore)
        {
            redSpawned = true;
            spawner.SpawnRed();
        }

        if (score >= targetScore) Win();
    }

    public void SetBonusTotal(int total) => bonusTotal = total;

    public void AddBonus(int amount)
    {
        bonus += amount;
        bonusText.gameObject.SetActive(true);
        bonusText.text = $"BonusCoins: {bonus}/{bonusTotal}";
    }

    void UpdateUI() => scoreText.text = $"Score: {score} / {targetScore}";

    void Win()
    {
        resultText.text = $"Score: {score} / {targetScore}\nBonusCoins: {bonus}/{bonusTotal}";
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}