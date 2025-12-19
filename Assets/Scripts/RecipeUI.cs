using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeUI : MonoBehaviour
{
    public static RecipeUI Instance { get; private set; }

    [Header("Root")]
    public GameObject root;

    [Header("Ingredients")]
    public Transform ingredientsParent;
    public IngredientIconUI ingredientPrefab;

    [Header("Result")]
    public Image resultIcon;
    public Text resultCountText; // volitelné (může být null)

    [Header("Text")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    CarryableRecipeItem _currentOwner;

    void Awake()
    {
        Instance = this;
        if (root != null) root.SetActive(false);
    }

    public bool IsOpen => root != null && root.activeSelf;
    public bool IsShowing(CarryableRecipeItem owner) => _currentOwner == owner;

    public void Show(CauldronRecipe recipe, CarryableRecipeItem owner)
    {
        if (recipe == null) return;

        _currentOwner = owner;

        // 1) vyčistit staré ingredience
        foreach (Transform c in ingredientsParent)
            Destroy(c.gameObject);

        // 2) vygenerovat nové ikonky ingrediencí
        foreach (var ing in recipe.ingredients)
        {
            var ui = Instantiate(ingredientPrefab, ingredientsParent);
            ui.Set(ing);
        }

        // 3) nastavit výsledek
        if (resultIcon != null)
        {
            resultIcon.sprite = recipe.resultType != null ? recipe.resultType.icon : null;
            resultIcon.enabled = (resultIcon.sprite != null);
        }

        // 4) texty
        if (titleText != null) titleText.text = recipe.title;
        if (descriptionText != null) descriptionText.text = recipe.description;

        root.SetActive(true);
    }

    public void Hide(CarryableRecipeItem owner = null)
    {
        // pokud chceš, aby šel zavřít jen ten “správný” owner:
        if (owner != null && _currentOwner != owner) return;

        if (root != null) root.SetActive(false);
        _currentOwner = null;
    }
}
