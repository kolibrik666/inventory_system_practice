using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextButton : ButtonHintExtended, IPointerExitHandler
{
    /*[SerializeField] Transform _transform;
    RectTransform _rectTransform => _transform as RectTransform;*/

    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] TextMeshProUGUI _textOther;

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
        _textOther.CrossFadeColor(color * colors.colorMultiplier, instant ? 0f : colors.fadeDuration, true, true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if(interactable)
        //if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition)) return;
        DoStateTransition(SelectionState.Normal, true);
    }
}
