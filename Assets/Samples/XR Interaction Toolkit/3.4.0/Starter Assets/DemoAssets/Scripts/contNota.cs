using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class contNota : MonoBehaviour
{
    public InputActionReference inputActionReference;
    InputAction action;
    public GameObject notaBrazo;

    // Start is called before the first frame update
    void Start()
    {
        action = inputActionReference.action;
        action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        bool hold = action.IsPressed();
        bool pressed = action.WasPressedThisFrame();
        bool release = action.WasReleasedThisFrame();

        if (hold || pressed)
        {
            notaBrazo.SetActive(true);
        }

        if (release)
        {
            notaBrazo.SetActive(false);
        }
    }
}
