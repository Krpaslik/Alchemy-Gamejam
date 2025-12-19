using UnityEngine;

[RequireComponent(typeof(CarryableItem))]
public class CarryableRecipeItem : MonoBehaviour
{
    public CauldronRecipe recipe;

    // zavolej při "use"
    public void Toggle(PlayerInteractor player)
    {
        if (RecipeUI.Instance == null) return;

        // pokud už je otevřené pro tento item => zavři + dropni
        if (RecipeUI.Instance.IsOpen && RecipeUI.Instance.IsShowing(this))
        {
            RecipeUI.Instance.Hide(this);

            // drop item z ruky hráče
            var carrier = player.GetComponent<PlayerCarrier>();
            if (carrier != null)
                carrier.DropItem();

            return;
        }

        // jinak otevři
        RecipeUI.Instance.Show(recipe, this);
    }

    // bezpečnost: když by item nějak zmizel / dropnul se jinak, zavři okno
    void OnDisable()
    {
        if (RecipeUI.Instance != null && RecipeUI.Instance.IsShowing(this))
            RecipeUI.Instance.Hide(this);
    }
}
