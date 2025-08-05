using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeInput : MonoBehaviour
{
    EventSystem system;

    private void Start()
    {
        system = EventSystem.current;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject current = system.currentSelectedGameObject;

            if (current != null)
            {
                Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

                if (next != null)
                {
                    next.Select();
                }
            }

        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            GameObject current = system.currentSelectedGameObject;

            if (current != null)
            {
                if (current.GetComponent<TMP_InputField>() != null)
                {
                    Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

                    if (next != null)
                    {
                        if (next.TryGetComponent(out Button button))
                        {
                            button.Select();
                            button.onClick.Invoke();
                            Debug.Log("버튼눌림");
                        }
                    }
                }
                else if (current.TryGetComponent(out Button button))
                {
                    button.Select();
                    button.onClick.Invoke();
                    Debug.Log("버튼눌림");
                }
            }
        }
    }
}
