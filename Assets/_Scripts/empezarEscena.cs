using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class empezarEscena : MonoBehaviour
{
    public GameObject[] objetos;
    public GameObject Iniciar, FadeIn;
    public GameObject player, inicial, previa;
    public InputActionReference inputActionReference;
    InputAction action;

    // Start is called before the first frame update
    void Start()
    {
        player.transform.position = previa.transform.position;
        action = inputActionReference.action;
        // activar input !!!!!
        action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        bool hold = action.IsPressed();
        bool pressed = action.WasPressedThisFrame();
        bool release = action.WasReleasedThisFrame();
        FadeIn.transform.rotation = Quaternion.Euler(0, 0, 0);
        Iniciar.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        if (pressed)
        {
            FadeIn.SetActive(true);
            Invoke("Empezar", 3f);
        }
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    FadeIn.SetActive(true);
        //    Invoke("Empezar", 3f);
        //}
    }

    public void Empezar()
    {
        Iniciar.SetActive(true);
        FadeIn.SetActive(false);
        player.transform.position = inicial.transform.position;
        foreach (GameObject obj in objetos)
        {
            obj.SetActive(false);
        }

        int indice = Random.Range(0, objetos.Length);

        objetos[indice].SetActive(true);
    }
}
