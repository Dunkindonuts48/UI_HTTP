using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class ClientSend : MonoBehaviour
{
    // Reference to the TextMeshPro Input Fields
    public TMP_InputField inputField1;
    public TMP_InputField inputField2;
    public TMP_InputField inputField3;

    // Reference to the Dropdown
    public TMP_Dropdown dropdown;

    // Reference to the Button
    public Button submitButton;

    // Data storage
    private string inputData1;
    private string inputData2;
    private string inputData3;
    
    void Start()
    {
        // Assign the Button click event
        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        // Get data from Input Fields
        inputData1 = inputField1.text.Trim();
        inputData2 = inputField2.text.Trim();


        // Check the selected option in the dropdown
        if (dropdown.value == 2) // Assuming the third option (index 2) ignores the third input field
        {
            inputData3 = "";
        }
        else
        {
            inputData3 = inputField3.text.Trim();
        }

        // Store or process the data as needed
        Debug.Log("URL: " + inputData1);
        Debug.Log("HEADER: " + inputData2);
        Debug.Log("BODY: " + inputData3);

        //// Optionally, clear the input fields after submission
        //inputField1.text = "";
        //inputField2.text = "";
        //if (dropdown.value != 2)
        //{
        //    inputField3.text = "";
        //}
    }
}