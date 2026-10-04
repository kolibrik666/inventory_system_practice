using InventorySystem;
using TMPro;

using UnityEngine;

public sealed class InventoryDetailView : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private TMP_Text _value;

    public void Render(InventoryDetailData data)
    {
        _label.text = data.Label;
        _value.text = data.Value;
    }
}
