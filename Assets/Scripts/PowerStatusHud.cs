using System.Text;
using TMPro;
using UnityEngine;

public class PowerStatusHud : MonoBehaviour
{
    TMP_Text label;
    DashAbility dash;
    readonly StringBuilder builder = new StringBuilder();

    void Awake() => label = GetComponent<TMP_Text>();
    void Start() => dash = FindFirstObjectByType<DashAbility>();

    void Update()
    {
        builder.Clear();
        if (dash != null && dash.IsActive) builder.AppendLine(dash.IsReady ? $"DASH {dash.SecondsRemaining:0}s - press SPACE" : $"DASH {dash.SecondsRemaining:0}s - recharging");
        if (FreezeEffect.Active) builder.AppendLine($"FREEZE {FreezeEffect.SecondsRemaining:0.0}s");
        if (GameManager.Instance != null && GameManager.Instance.CoinsDoubled) builder.AppendLine($"COINS x2 {GameManager.Instance.DoubleSecondsRemaining:0.0}s");
        if (GameManager.Instance != null && GameManager.Instance.SlowSecondsRemaining > 0f) builder.AppendLine($"SLOW-MO {GameManager.Instance.SlowSecondsRemaining:0.0}s");
        label.text = builder.ToString();
    }
}