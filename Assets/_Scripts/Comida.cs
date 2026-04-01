using UnityEngine;

public class Comida : MonoBehaviour
{
    public AudioSource audioSource;

    private bool isEaten = false;

    public void Eat()
    {
        if (isEaten) return;
        isEaten = true;

        ComidaData data = GetComponent<ComidaData>();

        if (data != null && data.eatSound != null)
        {
            audioSource.PlayOneShot(data.eatSound);
        }

        Destroy(gameObject, 0.2f);
    }
}