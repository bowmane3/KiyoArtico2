using UnityEngine;

public class Ingrediente : MonoBehaviour
{
    private bool isUsed = false;

    public void OnDroppedInPot(Transform target)
    {
        if (isUsed) return;
        isUsed = true;

        StartCoroutine(ShrinkAndDestroy(target));
    }

    private System.Collections.IEnumerator ShrinkAndDestroy(Transform target)
    {
        float duration = 0.3f;
        float time = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            transform.position = Vector3.Lerp(startPos, target.position, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}