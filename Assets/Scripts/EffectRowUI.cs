using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectRowUI : MonoBehaviour
{
    [SerializeField] Image EffectSprite;
    [SerializeField] TMP_Text EffectTime;

    public void Set(Sprite sprite, float timeLeft)
    {
        if (EffectSprite) EffectSprite.sprite = sprite;
        if (EffectTime) EffectTime.text = $"{FormatTime(timeLeft)}";
    }

    static string FormatTime(float t)
    {
        t = Mathf.Max(0f, t);
        int s = Mathf.FloorToInt(t);
        return $"{s}s";
    }
}
