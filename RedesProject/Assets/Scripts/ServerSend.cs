using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerSend : MonoBehaviour
{
    // Reference to the TextMeshPro Text Component
    public TMP_Text textMeshPro;

    // Method to update the TextMeshPro text
    public void UpdateText(string newText)
    {
        textMeshPro.text = newText;
    }
}
