using UnityEngine;
using UnityEngine.UI;

public class IngredientIconUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    void Reset()
    {
        // automaticky se pokusí najít Image v dětech
        if (icon == null) icon = GetComponentInChildren<Image>();
    }

    public void Set(ItemTypeData item)
    {
        if (icon == null)
        {
            Debug.LogError("[IngredientIconUI] Missing 'icon' Image reference on prefab!", this);
            return;
        }

        if (item == null)
        {
            Debug.LogError("[IngredientIconUI] ItemTypeData is NULL (ingredient entry empty).", this);
            icon.enabled = false;
            return;
        }

        icon.sprite = item.icon;
        icon.enabled = (icon.sprite != null);
    }
}
