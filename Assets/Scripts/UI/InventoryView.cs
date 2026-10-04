using System;
using System.Collections.Generic;
using System.Globalization;

using InventorySystem;
using SciptableObjects;
using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class InventoryView : MonoBehaviour, IInventoryView
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Transform _itemsRoot;
    [SerializeField] private InventoryItemView _itemPrefab;
    [SerializeField] private Transform _detailsRoot;
    [SerializeField] private InventoryDetailView _detailPrefab;
    [SerializeField] private GameObject _detailsPanel;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _weight;
    [SerializeField] private TMP_Text _useHint;
    [SerializeField] private TMP_Text _equipHint;

    private readonly List<InventoryItemView> _rows = new();
    private readonly List<InventoryDetailView> _details = new();

    public bool IsVisible => _canvasGroup != null && _canvasGroup.interactable;

    public event Action<Guid> ItemHovered;
    public event Action<Guid> ItemClicked;
    public event Action<bool> VisibilityChanged;

    public void SetVisible(bool visible)
    {
        bool wasVisible = IsVisible;
        _canvasGroup.alpha = visible ? 1 : 0;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;

        if (!visible && EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        if (wasVisible != visible)
        {
            VisibilityChanged?.Invoke(visible);
        }
    }

    public void ShowRows(IReadOnlyList<InventoryRowData> rows)
    {
        for (int index = 0; index < rows.Count; index++)
        {
            if (index == _rows.Count)
            {
                InventoryItemView row = Instantiate(_itemPrefab, _itemsRoot);
                row.Hovered += OnItemHovered;
                row.Clicked += OnItemClicked;
                _rows.Add(row);
            }

            _rows[index].gameObject.SetActive(true);
            _rows[index].Render(rows[index]);
        }

        for (int index = rows.Count; index < _rows.Count; index++)
        {
            _rows[index].gameObject.SetActive(false);
        }
    }

    public void ShowSelection(Guid previousId, Guid currentId)
    {
        foreach (InventoryItemView row in _rows)
        {
            if (!row.gameObject.activeSelf) continue;
            if (row.EntryId != previousId && row.EntryId != currentId) continue;
            row.SetSelected(row.EntryId == currentId);
        }
    }

    public void ShowDetails(ItemData item, IReadOnlyList<InventoryDetailData> details, bool canConsume, bool canEquip, bool equipped)
    {
        _detailsPanel.SetActive(item != null);
        _itemIcon.sprite = item != null ? item.ItemIcon : null;
        _itemIcon.enabled = _itemIcon.sprite != null;
        _description.text = item != null ? item.ItemDesc : string.Empty;
        _useHint.gameObject.SetActive(canConsume);
        _equipHint.gameObject.SetActive(canEquip);
        _equipHint.text = equipped ? "SPACE) <b>UNEQUIP</b>" : "SPACE) <b>EQUIP</b>";

        for (int index = 0; index < details.Count; index++)
        {
            if (index == _details.Count)
            {
                _details.Add(Instantiate(_detailPrefab, _detailsRoot));
            }

            _details[index].gameObject.SetActive(true);
            _details[index].Render(details[index]);
        }

        for (int index = details.Count; index < _details.Count; index++)
        {
            _details[index].gameObject.SetActive(false);
        }
    }

    public void ShowWeight(float current, float capacity)
    {
        _weight.text = $"{current.ToString("0.0##", CultureInfo.InvariantCulture)}/{capacity.ToString("0.0##", CultureInfo.InvariantCulture)}";
    }

    private void OnItemHovered(Guid id)
    {
        ItemHovered?.Invoke(id);
    }

    private void OnItemClicked(Guid id)
    {
        ItemClicked?.Invoke(id);
    }

    private void OnDestroy()
    {
        foreach (InventoryItemView row in _rows)
        {
            if (row == null) continue;
            row.Hovered -= OnItemHovered;
            row.Clicked -= OnItemClicked;
        }
    }
}
