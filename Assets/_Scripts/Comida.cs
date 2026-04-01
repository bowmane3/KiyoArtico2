using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Edible : MonoBehaviour
{
    public AudioSource audioSource;

    private XRGrabInteractable grab;
    private bool isEaten = false;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEaten) return;

        if (!grab.isSelected) return;

        if (other.CompareTag("Mouth"))
        {
            Eat();
        }
    }

    public void Eat()
    {
        if (isEaten) return;
        isEaten = true;

        ComidaData data = GetComponent<ComidaData>();

        if (data != null && data.eatSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(data.eatSound);
        }

        Destroy(gameObject, 0.2f);
    }
}