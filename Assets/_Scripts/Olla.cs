using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("UI")]
    public GameObject floatingTextPrefab;

    private GameObject currentDish;

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

    public void StartCooking()
    {
        if (ingredientCount == 0 || isCooking) return;

        StartCoroutine(CookRoutine());
    }

    private IEnumerator CookRoutine()
    {
        isCooking = true;

        if (potAnimator != null)
            potAnimator.SetTrigger("Cook");

        yield return new WaitForSeconds(cookTime);

        SpawnDish();

        ingredientCount = 0;
        isCooking = false;
        hasFinished = true;
    }

    private void SpawnDish()
    {
        if (possibleDishes.Count == 0) return;

        int index = Random.Range(0, possibleDishes.Count);
        GameObject dishPrefab = possibleDishes[index];

        currentDish = Instantiate(dishPrefab, spawnPoint.position, Quaternion.identity);

        StartCoroutine(HoverEffect(currentDish.transform));

        ShowDishName(currentDish);
    }

    private IEnumerator HoverEffect(Transform obj)
    {
        float time = 0f;

        while (obj != null)
        {
            time += Time.deltaTime;
            obj.position += Vector3.up * Mathf.Sin(time * 2f) * 0.002f;
            yield return null;
        }
    }

    private void ShowDishName(GameObject dish)
    {
        ComidaData data = dish.GetComponent<ComidaData>();
        if (data == null || floatingTextPrefab == null) return;

        GameObject textObj = Instantiate(floatingTextPrefab, dish.transform);
        textObj.transform.localPosition = Vector3.up * 0.2f;

        TMPro.TextMeshPro text = textObj.GetComponent<TMPro.TextMeshPro>();
        if (text != null)
            text.text = data.dishName;
    }
}