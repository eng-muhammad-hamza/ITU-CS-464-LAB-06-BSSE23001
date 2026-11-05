using UnityEngine;

public static class FreezeEffect
{
    static float until;

    public static bool Active => Time.time < until;
    public static float SecondsRemaining => Mathf.Max(0f, until - Time.time);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => until = 0f;

    public static void Clear() => until = 0f;

    public static void Begin(float seconds)
    {
        until = Time.time + seconds;
        foreach (GuardAI guard in Object.FindObjectsByType<GuardAI>(FindObjectsSortMode.None))
            guard.Stun(seconds);
    }
}