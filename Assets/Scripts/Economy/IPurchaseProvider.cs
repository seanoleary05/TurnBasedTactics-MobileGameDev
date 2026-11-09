using UnityEngine;

public interface IPurchaseProvider     // real store SDK sits behind
{ Awaitable<bool> PurchaseAsync(string productId); }