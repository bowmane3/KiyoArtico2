using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Edible : MonoBehaviour
{
    private XRGrabInteractable grab;
    private bool isEaten = false;
    private AudioSource lastMouthAudioSource;

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
            AudioSource mouthAudio = other.GetComponent<AudioSource>();
            if (mouthAudio == null)
                mouthAudio = other.GetComponentInParent<AudioSource>();

            lastMouthAudioSource = mouthAudio;
            Eat();
        }
    }

    public void Eat()
    {
        if (isEaten) return;
        isEaten = true;

        ComidaData data = GetComponent<ComidaData>();

        if (data != null && data.eatSound != null && lastMouthAudioSource != null)
        {
            lastMouthAudioSource.PlayOneShot(data.eatSound);
        }

        Destroy(gameObject, 0.2f);
    }
}