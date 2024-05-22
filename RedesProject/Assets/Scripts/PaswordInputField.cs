using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class PaswordInputField : MonoBehaviour
{
    public TMP_InputField inputField1;
    public TMP_InputField inputField2;
    public GameObject gam; 
    private string inputData1;
    private string inputData2;
    // Reference to the Button
    public Button submitButton;
    // Start is called before the first frame update
    void Start()
    {
        // Assign the Button click event
        submitButton.onClick.AddListener(OnSubmit);
    }

    // Update is called once per frame
    void OnSubmit()
    {
        Debug.Log("SUB");
        // Get data from Input Fields
        inputData1 = inputField1.text.Trim();
        inputData2 = inputField2.text.Trim();

        Debug.Log("USER: " + inputData1);
        Debug.Log("PASSWORD: " + inputData2);

        if (inputData1 == "A" && inputData2 == "B")
        {
            gam.SetActive(false);
        }
    }

}
