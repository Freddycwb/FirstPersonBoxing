using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextField : MonoBehaviour
{
    private TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }
    public void Manage()
    {
        inputField.text = inputField.text.ToUpper();
    }
}
