using UnityEngine;

public class PrenderGas : MonoBehaviour
{
    public Olla pot;

    public void TurnOnGas()
    {
        pot.StartCooking();
    }
}