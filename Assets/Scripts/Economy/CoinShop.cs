using UnityEngine;

// Buy-pack button handler. Lives on the Result panel only, never mid-battle.
// Gameplay depends on IPurchaseProvider, so a real billing class is a one-line swap here.
public class CoinShop : MonoBehaviour
{
    IPurchaseProvider store = new FakePurchaseProvider();

    public async void OnBuyPack()   // Button handler: async void ok
    {
        if (await store.PurchaseAsync("coins_500")) Wallet.Instance?.Earn(500);
    }
}
