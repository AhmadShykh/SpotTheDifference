using UnityEngine;

public class ShopHandler : MonoBehaviour
{
	public void PurchaseHint(int hintsPurchased)
    {
        Debug.Log("Hint Purchased Successful");

        PlayerPrefs.SetInt("Hints", PlayerPrefs.GetInt("Hints",3) +hintsPurchased);
    }
}
