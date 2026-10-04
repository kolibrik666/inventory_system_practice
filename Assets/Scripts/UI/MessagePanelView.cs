using System.Globalization;

using HaulSystem;
using TMPro;

using UnityEngine;

[DisallowMultipleComponent]
public sealed class MessagePanelView : MonoBehaviour, IHaulRoundView
{
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _restartHint;

    public bool IsConfigured => _scoreText != null && _restartHint != null && _scoreText != _restartHint;

    public void Show(HaulResult result)
    {
        string award = result.Award == HaulAward.None ? "NO MEDAL" : result.Award.ToString().ToUpperInvariant();
        _scoreText.text = $"<b>SCORE: {result.Score.ToString("N0", CultureInfo.InvariantCulture)}</b> · {award}";
        _restartHint.text = "<b>PRESS ANY KEY OR MOUSE BUTTON TO RESTART</b>";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
