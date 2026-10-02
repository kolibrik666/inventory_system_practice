using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHintExtended : Button
{
    public Transform _hint;
    public TextMeshProUGUI _hintText;

    Transform _defaultHint;
    string _hintToShow = string.Empty;
    bool _checkButtonOnlyForInteraction;

    public Transform DefaultHint => _defaultHint;
    public bool IsHintActive => _hint.gameObject.activeInHierarchy;

    protected override void Awake()
    {
        base.Awake();
        if (_hint != null) _hint.gameObject.SetActive(false);
        _defaultHint = _hint;
    }

    public void SwitchTransformToShow(Transform tooltip)
    {
        bool wasActive = IsHintActive;
        if (wasActive) _hint.gameObject.SetActive(false);
        _hint = tooltip;
        _hint.gameObject.SetActive(wasActive);
    }

    public void SetInteractableWithHint(bool interact, string hintToShow = null, bool checkButtonOnlyForInteraction = false)
    {
        interactable = interact;
        _hintToShow = hintToShow ?? string.Empty;
        _checkButtonOnlyForInteraction = checkButtonOnlyForInteraction;
        if (interactable && IsHintActive) ShowHint(false);
        else if (IsHintActive) ShowHint(true);
    }

    private void ShowHint(bool show)
    {
        if (_hintToShow == string.Empty)
        {
            _hint.gameObject.SetActive(false);
            return;
        }
        _hintText.text = _hintToShow;
        _hint.gameObject.SetActive(show);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_hint as RectTransform);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if ((_checkButtonOnlyForInteraction ? !interactable : !IsInteractable())
            && _hintToShow != string.Empty && _hint != null)
        {
            ShowHint(true);
        }
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (_hint != null && _hint.gameObject.activeInHierarchy)
        {
            ShowHint(false);
        }
        base.OnPointerExit(eventData);
    }
}
