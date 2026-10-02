using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextToggle : Toggle
{
    [SerializeField] TextMeshProUGUI _text;

    [SerializeField] bool _useCustomColors = true;
    [SerializeField] ColorBlock _colors = ColorBlock.defaultColorBlock;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        var colors = _useCustomColors ? _colors : base.colors;
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
    }
}
