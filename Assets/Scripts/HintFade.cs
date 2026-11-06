using UnityEngine;

public class HintFade : MonoBehaviour
{
    [SerializeField] float showSeconds = 6f;

    void Update()
    {
        if (Time.timeSinceLevelLoad > showSeconds) gameObject.SetActive(false);
    }
}