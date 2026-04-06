using UnityEngine;
using DG.Tweening;

public class Ingrediente : MonoBehaviour
{
    private bool isUsed = false;
    private Tween consumeTween;

    [SerializeField] private float consumeDuration = 0.3f;

    public void OnDroppedInPot(Transform target)
    {
        if (isUsed) return;
        isUsed = true;

        consumeTween?.Kill();
        Sequence sequence = DOTween.Sequence();
        sequence.Join(transform.DOMove(target.position, consumeDuration).SetEase(Ease.InQuad));
        sequence.Join(transform.DOScale(Vector3.zero, consumeDuration).SetEase(Ease.InBack));
        consumeTween = sequence.OnComplete(() => Destroy(gameObject));
    }

    private void OnDisable()
    {
        consumeTween?.Kill();
        consumeTween = null;
    }
}