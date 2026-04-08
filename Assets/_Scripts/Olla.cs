using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Olla : MonoBehaviour
{
    public Transform dropPoint;
    public Transform spawnPoint;

    public List<GameObject> possibleDishes;

    public float cookTime = 3f;

    private int ingredientCount = 0;
    
    private bool isCooking = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip cookingLoopSound;
    public AudioClip cookingReadySound;
    public AudioClip invalidGasSound;

    [Header("Cooking Effects")]
    public Transform fireVisual;
    public float firePopDuration = 0.3f;
    public float fireRetractDuration = 0.35f;
    public float firePulseAmount = 0.12f;
    public float firePulseSpeed = 6f;
    public ParticleSystem smokeParticles;
    public Transform potShakeTarget;
    public float potShakeAmount = 0.015f;
    public float potShakeSpeed = 18f;
    public float potShakeSettleDuration = 0.35f;

    [Header("Dish Pop")]
    public float dishPopDuration = 0.28f;
    public float dishPopScale = 1.08f;
    public float textPopDuration = 0.22f;
    public float textPopScale = 1.15f;

    [Header("UI")]
    public GameObject floatingTextPrefab;

    private GameObject currentDish;
    private Vector3 fireBaseScale;
    private Vector3 potBaseLocalPosition;
    private Tween fireTween;
    private Tween firePulseTween;
    private Tween potShakeTween;
    private Tween potSettleTween;
    private Tween dishPopTween;
    private Tween textPopTween;

    public bool TryStartCooking()
    {
        if (ingredientCount == 0 || isCooking || IsDishSpawnLocked())
        {
            PlayInvalidGasSound();
            return false;
        }

        StartCoroutine(CookRoutine());
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCooking) return;

        Ingrediente ingredient = other.GetComponent<Ingrediente>();
        if (ingredient != null)
        {
            ingredientCount++;
            ingredient.OnDroppedInPot(dropPoint);
        }
    }

    private void Awake()
    {
        if (fireVisual != null)
        {
            fireBaseScale = fireVisual.localScale;
            fireVisual.localScale = Vector3.zero;
        }

        if (potShakeTarget == null)
            potShakeTarget = transform;

        if (potShakeTarget != null)
            potBaseLocalPosition = potShakeTarget.localPosition;
    }

    public void StartCooking()
    {
        TryStartCooking();
    }

    private IEnumerator CookRoutine()
    {
        isCooking = true;
        StartCookingEffects();
        PlayCookingLoopSound();

        yield return new WaitForSeconds(cookTime);

        SpawnDish();
        StopCookingLoopSound();
        StopCookingEffects();
        PlayCookingReadySound();

        ingredientCount = 0;
        isCooking = false;
    }

    private bool IsDishSpawnLocked()
    {
        if (currentDish == null)
            return false;

        DishSpawnLock lockBehaviour = currentDish.GetComponent<DishSpawnLock>();
        if (lockBehaviour == null)
            return false;

        return lockBehaviour.isActiveAndEnabled && lockBehaviour.IsLocked;
    }

    private void StartCookingEffects()
    {
        potSettleTween?.Kill();
        potSettleTween = null;

        if (fireVisual != null)
        {
            fireVisual.gameObject.SetActive(true);
            fireVisual.localScale = Vector3.zero;

            fireTween?.Kill();
            firePulseTween?.Kill();

            fireTween = fireVisual.DOScale(fireBaseScale, Mathf.Max(0.01f, firePopDuration))
                .SetEase(Ease.OutBack)
                .OnComplete(StartFirePulse);
        }

        if (potShakeTarget != null)
            potBaseLocalPosition = potShakeTarget.localPosition;

        if (smokeParticles != null)
            smokeParticles.Play();

        StartPotShake();
    }

    private void StopCookingEffects(bool instant = false)
    {
        firePulseTween?.Kill();
        firePulseTween = null;

        potShakeTween?.Kill();
        potShakeTween = null;

        if (fireVisual != null)
        {
            fireTween?.Kill();
            fireTween = null;

            if (instant)
            {
                fireVisual.localScale = Vector3.zero;
                fireVisual.gameObject.SetActive(false);
            }
            else
            {
                fireTween = fireVisual.DOScale(Vector3.zero, Mathf.Max(0.01f, fireRetractDuration))
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        if (fireVisual != null)
                            fireVisual.gameObject.SetActive(false);
                    });
            }
        }

        if (potShakeTarget != null)
        {
            potSettleTween?.Kill();
            potSettleTween = null;

            if (instant)
            {
                potShakeTarget.localPosition = potBaseLocalPosition;
            }
            else
            {
                potSettleTween = potShakeTarget.DOLocalMove(potBaseLocalPosition, Mathf.Max(0.01f, potShakeSettleDuration))
                    .SetEase(Ease.OutCubic);
            }
        }

        if (smokeParticles != null)
            smokeParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }

    private void StartFirePulse()
    {
        if (fireVisual == null || !isCooking)
            return;

        firePulseTween?.Kill();
        float pulseDuration = Mathf.Max(0.01f, 1f / Mathf.Max(0.01f, firePulseSpeed));
        float targetScale = 1f + firePulseAmount;
        firePulseTween = fireVisual
            .DOScale(fireBaseScale * targetScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StartPotShake()
    {
        if (potShakeTarget == null)
            return;

        potShakeTween?.Kill();
        float loopDuration = Mathf.Max(0.01f, 1f / Mathf.Max(0.01f, potShakeSpeed));
        potShakeTween = DOTween.To(
                () => 0f,
                phase =>
                {
                    float shakeX = Mathf.Sin(phase * Mathf.PI * 2f) * potShakeAmount;
                    float shakeZ = Mathf.Cos(phase * Mathf.PI * 2f * 0.85f) * potShakeAmount;
                    potShakeTarget.localPosition = potBaseLocalPosition + new Vector3(shakeX, 0f, shakeZ);
                },
                1f,
                loopDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void PopDishAndLabel(Transform dishTransform, Transform labelTransform)
    {
        if (dishTransform != null)
        {
            dishPopTween?.Kill();
            Vector3 dishBaseScale = dishTransform.localScale;
            dishTransform.localScale = Vector3.zero;
            dishPopTween = dishTransform
                .DOScale(dishBaseScale * dishPopScale, Mathf.Max(0.01f, dishPopDuration))
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    if (dishTransform != null)
                        dishTransform.DOScale(dishBaseScale, 0.08f).SetEase(Ease.InOutSine);
                });
        }

        if (labelTransform != null)
        {
            textPopTween?.Kill();
            Vector3 textBaseScale = labelTransform.localScale;
            labelTransform.localScale = Vector3.zero;
            textPopTween = labelTransform
                .DOScale(textBaseScale * textPopScale, Mathf.Max(0.01f, textPopDuration))
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    if (labelTransform != null)
                        labelTransform.DOScale(textBaseScale, 0.07f).SetEase(Ease.InOutSine);
                });
        }
    }

    private void OnDisable()
    {
        StopCookingLoopSound();
        StopCookingEffects(true);
        dishPopTween?.Kill();
        dishPopTween = null;
        textPopTween?.Kill();
        textPopTween = null;
    }

    private AudioSource GetAudioSource()
    {
        if (audioSource != null)
            return audioSource;

        if (TryGetComponent(out AudioSource cachedAudioSource))
        {
            audioSource = cachedAudioSource;
            return audioSource;
        }

        return null;
    }

    private void PlayCookingLoopSound()
    {
        AudioSource source = GetAudioSource();
        if (source == null || cookingLoopSound == null)
            return;

        source.Stop();
        source.loop = true;
        source.clip = cookingLoopSound;
        source.Play();
    }

    private void StopCookingLoopSound()
    {
        AudioSource source = GetAudioSource();
        if (source == null)
            return;

        if (source.clip == cookingLoopSound)
        {
            source.Stop();
            source.loop = false;
            source.clip = null;
        }
    }

    private void PlayCookingReadySound()
    {
        AudioSource source = GetAudioSource();
        if (source == null || cookingReadySound == null)
            return;

        source.PlayOneShot(cookingReadySound);
    }

    private void PlayInvalidGasSound()
    {
        AudioSource source = GetAudioSource();
        if (source == null || invalidGasSound == null)
            return;

        source.PlayOneShot(invalidGasSound);
    }

    private void SpawnDish()
    {
        if (possibleDishes.Count == 0) return;

        int index = Random.Range(0, possibleDishes.Count);
        GameObject dishPrefab = possibleDishes[index];

        currentDish = Instantiate(dishPrefab, spawnPoint.position, Quaternion.identity);

        Transform label = ShowDishName(currentDish);
        PopDishAndLabel(currentDish.transform, label);

        XRGrabInteractable grab = currentDish.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            DishSpawnLock lockBehaviour = currentDish.GetComponent<DishSpawnLock>();
            if (lockBehaviour == null)
            {
                lockBehaviour = currentDish.AddComponent<DishSpawnLock>();
            }

            lockBehaviour.Initialize(label);
        }
    }

    private Transform ShowDishName(GameObject dish)
    {
        ComidaData data = dish.GetComponent<ComidaData>();
        if (data == null || floatingTextPrefab == null) return null;

        GameObject textObj = Instantiate(floatingTextPrefab, dish.transform);
        textObj.transform.localPosition = Vector3.up * 0.4f;

        if (textObj.GetComponent<WorldTextBillboard>() == null)
        {
            textObj.AddComponent<WorldTextBillboard>();
        }

        TMPro.TextMeshPro text = textObj.GetComponent<TMPro.TextMeshPro>();
        if (text != null)
            text.text = data.dishName;

        return textObj.transform;
    }
}