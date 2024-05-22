using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class ClientSend : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private ServerSend serverSend;

    [Space(10)]

    // Reference to the TextMeshPro Input Fields
    [Header("Inputs fields")]
    public TMP_InputField inputField1;
    public TMP_InputField inputField2;
    public TMP_InputField inputField3;

    [Space(10)]

    // Reference to the Dropdown
    [Header("Dropdown")]
    public TMP_Dropdown dropdown;

    [Space(10)]

    // Reference to the Button
    [Header("Buttons")]
    public Button submitButton;

    [Space(10)]

    // Data storage
    [Header("Inputs data")]
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
        // Reset text from server
        serverSend.UpdateText("");

        // Get data from Input Fields
        inputData1 = inputField1.text.Trim();
        inputData2 = inputField2.text.Trim();
        inputData3 = inputField3.text.Trim();

        // Store or process the data as needed
        Debug.Log("URL: " + inputData1);
        Debug.Log("HEADER: " + inputData2);
        Debug.Log("BODY: " + inputData3);

        // Create call depends of the type
        switch (dropdown.options[dropdown.value].text)
        {
            case "Post":
                // Check if its JSON
                if (!IsValidJson(inputData3))
                {
                    serverSend.UpdateText("The body is not a JSON, please check it.");
                    return;
                }
                else
                {

                }
                break;
            case "Get":
                // Advice the user that the Get method does not require a body and stop the process
                if (inputData3.Length != 0)
                {
                    serverSend.UpdateText("ERROR: Get no requires a body.");
                    return;
                }
                else
                {

                }
                break;
            case "Put":
                // Check if its JSON
                if (!IsValidJson(inputData3))
                {
                    serverSend.UpdateText("The body is not a JSON, please check it.");
                    return;
                }
                else
                {

                }
                break;
            case "Delete":
                // Advice the user that the Delete method does not require a body and stop the process
                if (inputData3.Length != 0)
                {
                    serverSend.UpdateText("ERROR: Get no requires a body.");
                    return;
                }
                else
                {

                }
                break;
        }

        //// Optionally, clear the input fields after submission
        //inputField1.text = "";
        //inputField2.text = "";
        //if (dropdown.value != 2)
        //{
        //    inputField3.text = "";
        //}
    }

    public class DummyClass
    {
        public string dummy;
    }

    public static bool IsValidJson(string json)
    {
        try
        {
            JsonUtility.FromJson<DummyClass>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}