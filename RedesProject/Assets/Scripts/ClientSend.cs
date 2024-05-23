using System;
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

        
        // string[] uri = inputData1.Split("/", StringSplitOptions.RemoveEmptyEntries);
        // uri[0] = uri[0].Trim();
        // if (uri[0] == "localhost")
        // {
        //     uri[0] = "127.0.0.1"; 
        // }

        HTTPRequest request = new HTTPRequest();
        if(!request.ParseHeader(dropdown.options[dropdown.value].text.ToUpper() +  inputData1 + "HTTP/1.1" + "\n" + inputData2 + "\r\n\r\n" + inputData3))
        {
            Debug.Log("__________ERRRORRRR--_____________");
            return; 
        } 
        
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
        
        Program.SendHTTPRequest(request);
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