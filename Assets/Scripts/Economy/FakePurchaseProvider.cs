using UnityEngine;

// Stand-in store. No IAP SDK is installed; nothing here talks to a real store.
public class FakePurchaseProvider : IPurchaseProvider
{
    public async Awaitable<bool> PurchaseAsync(string productId)
    {
        Debug.Log($"[iap-stub] purchase {productId}");
        await Awaitable.WaitForSecondsAsync(0.5f);  // fake dialog
        return true;                // real SDK result goes here later
    }
}
