using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
 
    [SerializeField] int targetScore = 10;
    [SerializeField] int lives = 3;
    [SerializeField] float respawnDelay = 0.6f;
    [SerializeField] float spawnProtection = 1.5f;
    [SerializeField] Health player;
    [SerializeField] Transform respawnPoint;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    int score;
 
    void Awake() => Instance = this;
    void Start() => UpdateUI();
 
    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
        if (score >= targetScore) Win();
    }
 
    public void UpdateUI()
    {
        scoreText.text = $"Score: {score} / {targetScore}";
        livesText.text = $"Lives: {lives}   Health: {Mathf.Max(player.Current, 0)} / {player.Max}";
    }
 
    public void PlayerDied()
    {
        lives--;
        UpdateUI();
        if (lives <= 0) Lose();
        else StartCoroutine(Respawn());
    }
 
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        Rigidbody body = player.GetComponent<Rigidbody>();
        body.position = respawnPoint.position;
        body.linearVelocity = Vector3.zero;
        player.Restore(spawnProtection);
    }
 
    void Win()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }
 
    void Lose()
    {
        losePanel.SetActive(true);
        Time.timeScale = 0f;
    }
 
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}