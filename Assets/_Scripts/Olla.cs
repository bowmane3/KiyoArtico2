using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Olla : MonoBehaviour
{
    public Transform dropPoint;
    public Transform spawnPoint;

    public List<GameObject> possibleDishes;

    public float cookTime = 3f;

    private int ingredientCount = 0;
    private bool isCooking = false;
    private bool hasFinished = false; //usar pa pop up tipo zeldas

    [Header("Animation")]
    public Animator potAnimator;

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

    [Header("UI")]
    public GameObject floatingTextPrefab;

    private GameObject currentDish;
    private Coroutine cookingEffectsRoutine;
    private Coroutine potSettleRoutine;
    private Coroutine firePopRoutine;
    private Coroutine fireRetractRoutine;
    private Vector3 fireBaseScale;
    private Vector3 potBaseLocalPosition;

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
        if (ingredientCount == 0 || isCooking) return;

        StartCoroutine(CookRoutine());
    }

    private IEnumerator CookRoutine()
    {
        isCooking = true;
        StartCookingEffects();

        if (potAnimator != null)
            potAnimator.SetTrigger("Cook");

        yield return new WaitForSeconds(cookTime);

        SpawnDish();
        StopCookingEffects();

        ingredientCount = 0;
        isCooking = false;
        hasFinished = true;
    }

    private void StartCookingEffects()
    {
        if (potSettleRoutine != null)
        {
            StopCoroutine(potSettleRoutine);
            potSettleRoutine = null;
        }

        if (fireVisual != null)
        {
            fireVisual.gameObject.SetActive(true);
            fireVisual.localScale = Vector3.zero;

            if (firePopRoutine != null)
                StopCoroutine(firePopRoutine);
            firePopRoutine = StartCoroutine(FirePopRoutine());
        }

        if (potShakeTarget != null)
            potBaseLocalPosition = potShakeTarget.localPosition;

        if (smokeParticles != null)
            smokeParticles.Play();

        if (cookingEffectsRoutine != null)
            StopCoroutine(cookingEffectsRoutine);

        cookingEffectsRoutine = StartCoroutine(CookingEffectsRoutine());
    }

    private void StopCookingEffects(bool instant = false)
    {
        if (cookingEffectsRoutine != null)
        {
            StopCoroutine(cookingEffectsRoutine);
            cookingEffectsRoutine = null;
        }

        if (fireVisual != null)
        {
            if (firePopRoutine != null)
                StopCoroutine(firePopRoutine);
            if (fireRetractRoutine != null)
                StopCoroutine(fireRetractRoutine);

            if (instant)
            {
                fireVisual.localScale = Vector3.zero;
                fireVisual.gameObject.SetActive(false);
                fireRetractRoutine = null;
            }
            else
            {
                fireRetractRoutine = StartCoroutine(FireRetractRoutine());
            }
        }

        if (potShakeTarget != null)
        {
            if (potSettleRoutine != null)
                StopCoroutine(potSettleRoutine);

            if (instant)
            {
                potShakeTarget.localPosition = potBaseLocalPosition;
                potSettleRoutine = null;
            }
            else
            {
                potSettleRoutine = StartCoroutine(SettlePotShakeRoutine());
            }
        }

        if (smokeParticles != null)
            smokeParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }

    private IEnumerator CookingEffectsRoutine()
    {
        while (firePopRoutine != null)
            yield return null;

        while (isCooking)
        {
            float time = Time.time;

            if (fireVisual != null)
            {
                float pulse = 1f + Mathf.Sin(time * firePulseSpeed) * firePulseAmount;
                fireVisual.localScale = fireBaseScale * pulse;
            }

            if (potShakeTarget != null)
            {
                float shakeX = Mathf.Sin(time * potShakeSpeed) * potShakeAmount;
                float shakeZ = Mathf.Cos(time * (potShakeSpeed * 0.85f)) * potShakeAmount;
                potShakeTarget.localPosition = potBaseLocalPosition + new Vector3(shakeX, 0f, shakeZ);
            }

            yield return null;
        }
    }

    private IEnumerator FirePopRoutine()
    {
        fireVisual.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, firePopDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            fireVisual.localScale = Vector3.Lerp(Vector3.zero, fireBaseScale, eased);
            yield return null;
        }

        fireVisual.localScale = fireBaseScale;
        firePopRoutine = null;
    }

    private IEnumerator FireRetractRoutine()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fireRetractDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            fireVisual.localScale = Vector3.Lerp(fireBaseScale, Vector3.zero, eased);
            yield return null;
        }

        fireVisual.localScale = Vector3.zero;
        fireVisual.gameObject.SetActive(false);
        fireRetractRoutine = null;
    }

    private IEnumerator SettlePotShakeRoutine()
    {
        Vector3 startPos = potShakeTarget.localPosition;
        float duration = Mathf.Max(0.01f, potShakeSettleDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            potShakeTarget.localPosition = Vector3.Lerp(startPos, potBaseLocalPosition, eased);
            yield return null;
        }

        potShakeTarget.localPosition = potBaseLocalPosition;
        potSettleRoutine = null;
    }

    private void OnDisable()
    {
        StopCookingEffects(true);
    }

    private void SpawnDish()
    {
        if (possibleDishes.Count == 0) return;

        int index = Random.Range(0, possibleDishes.Count);
        GameObject dishPrefab = possibleDishes[index];

        currentDish = Instantiate(dishPrefab, spawnPoint.position, Quaternion.identity);

        Transform label = ShowDishName(currentDish);

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