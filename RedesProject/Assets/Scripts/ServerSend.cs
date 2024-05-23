using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerSend : MonoBehaviour
{
    // Reference to the TextMeshPro Text Component
    public TMP_Text textMeshPro;

    public string messagePool = "";
    private object _lock = new object();  
    
    public static ServerSend instance = null; 
    
    public void Awake()
    {
        // Singleton
        if (instance != null)
        {
            Debug.Log("Server send is a singleton");
            return; 
        }

        instance = this;
    }

    // Method to update the TextMeshPro text
    public void UpdateText(string newText)
    {
        textMeshPro.text += newText;
    }

    public void Update()
    {
        lock (_lock)
        {
            if (messagePool.Length > 0)
            {
                UpdateText(messagePool);
                messagePool = ""; 
            }
        }
    }
}
