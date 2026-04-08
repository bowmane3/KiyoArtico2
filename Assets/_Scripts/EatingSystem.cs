using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EatingSystem : MonoBehaviour
{
    public int dishesEaten = 0;
    public int dishesToEat = 5;
    public TextMeshPro dishesEatenText;
    public GameObject winPanel;

    void Start()
    {
        UpdateEatCount();
    }

    public void Eat()
    {
        dishesEaten++;
        Debug.Log("Dishes eaten: " + dishesEaten);
        UpdateEatCount();
    }

    private void UpdateEatCount()
    {
        dishesEatenText.text = dishesEaten + "/" + dishesToEat;

        if(dishesEaten >= dishesToEat)
        {
            winPanel.SetActive(true);
            Invoke("Time", 3f);
        }
    }
    private void Time()
    {

            SceneManager.LoadScene(0);
    }
}
