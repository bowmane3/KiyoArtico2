using UnityEngine;

public class PrenderGas : MonoBehaviour
{
    public Olla pot;

    public void TurnOnGas()
    {
        if (pot == null)
        {
            Debug.LogWarning("PrenderGas: No pot assigned.", this);
            return;
        }

        pot.TryStartCooking();
    }
}