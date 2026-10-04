using TMPro;

using UnityEngine;

[DisallowMultipleComponent]
public sealed class InteractionTipView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private void Awake()
    {
        if (_text == null)
        {
            Debug.LogError("InteractionTipView requires a text reference.", this);
            enabled = false;
            return;
        }

        Hide();
    }

    private void OnDisable()
    {
        Hide();
    }

    public void Show(string prompt)
    {
        if (_text == null || !isActiveAndEnabled) return;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Hide();
            return;
        }

        if (_text.text != prompt) _text.text = prompt;
        _text.enabled = true;
    }

    public void Hide()
    {
        if (_text != null) _text.enabled = false;
    }
}
