using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextButton : Button
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextMeshProUGUI _textOther;
    [SerializeField] private bool _useCustomColors = true;
    [SerializeField] private ColorBlock _colors = ColorBlock.defaultColorBlock;

    protected TextMeshProUGUI PrimaryText => _text;
    protected TextMeshProUGUI SecondaryText => _textOther;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        ColorBlock colors = _useCustomColors ? _colors : base.colors;
        Color color = state switch
        {
            SelectionState.Normal => colors.normalColor,
            SelectionState.Highlighted => colors.highlightedColor,
            SelectionState.Pressed => colors.pressedColor,
            SelectionState.Selected => colors.selectedColor,
            SelectionState.Disabled => colors.disabledColor,
            _ => throw new System.NotImplementedException(),
        };
        _text.CrossFadeColor(color * colors.colorMultiplier, instant ? 0f : colors.fadeDuration, true, true);
        _textOther.CrossFadeColor(color * colors.colorMultiplier, instant ? 0f : colors.fadeDuration, true, true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (interactable)
        {
            DoStateTransition(SelectionState.Normal, true);
        }
    }
}
