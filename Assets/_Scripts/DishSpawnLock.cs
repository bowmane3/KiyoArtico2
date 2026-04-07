using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DishSpawnLock : MonoBehaviour
{
    private XRGrabInteractable grab;
    private Rigidbody rb;
    private Transform labelTransform;

    private bool isUnlocked;
    public bool IsLocked => !isUnlocked;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnSelectEntered);
        }

        LockInPlace();
    }

    private void OnDisable()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
        }
    }

    public void Initialize(Transform label)
    {
        labelTransform = label;
    }

    private void LockInPlace()
    {
        if (rb == null || isUnlocked) return;

        if (!rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        UnlockDish();
    }

    private void UnlockDish()
    {
        if (isUnlocked) return;

        isUnlocked = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }
        else
        {
            Debug.LogWarning("DishSpawnLock: No Rigidbody found on " + gameObject.name);
        }

        if (labelTransform != null)
        {
            Destroy(labelTransform.gameObject);
        }
    }

    private void LateUpdate()
    {
        if (isUnlocked && rb != null && rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.WakeUp();
        }
    }
}
