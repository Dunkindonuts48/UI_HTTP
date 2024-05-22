#define DEBUG_MODE

using System; 
using System.Text;
using System.Collections.Generic;

using HTTP_NET_Project;

public class HTTPRequest : HTTPHeader
{
    
    private string _requestLine = "";
    
    private static readonly string[] _avaliableRequestMethods = { "GET", "HEAD", "POST", "PUT", "DELETE" }; 
    private static readonly float[] _avaliableHttpVersions = { 1.0f, 1.1f, 2.0f };    /// TODO Replace with string format
    
    // ---------------- GETTERS AND SETTERS -------------
    /// <summary>
    /// Sets the request line of the HTTP response to the specified method, URI and with the speified http version
    /// </summary>
    /// <param name="method">method to make the reqyuest to the specified URI</param>
    /// <param name="uri">Uri direction to make the request to</param>
    /// <param name="httpVersion">http version to use in the tranfer</param>
    public void SetRequestLine(string method, string uri, float httpVersion)
    {
//         if (!_avaliableHttpVersions.Contains(httpVersion))
//         {
//             _requestLine = "";
// #if DEBUG_MODE
//             Console.WriteLine("Not avaliable http version: " + httpVersion); 
// #endif
//             return; 
//         }
        _requestLine = method + " " + uri + " HTTP/" + httpVersion.ToString(); 
    }

    
    // ---------------- UTILITIES METHODS -------------
            
    /// <summary>
    /// Parse the request as string into HTTPRequest class instance. Is able to take the request line, header attributes and the body from the string.
    /// Can fail if the blank spaces, \n or \r\n are not correctly within the request  
    /// </summary>
    /// <param name="request">Request in string format to parse into HTTPRequest</param>
    /// <returns>bool success, true if succesfully has parsed the header or false if is incorrectly formated</returns>
    public bool ParseHeader(string request)
    {
        this.Clear();

        request = request.Trim(); 
        string[] lines = request.Split(new[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        string[] header = lines[0].Split("\n", StringSplitOptions.RemoveEmptyEntries);
        
        // Error check
        if (lines.Length <= 1 || header.Length <= 1)
        {
            _requestLine = "";
            _headerLines.Clear();
            _body = "";
            return false; 
        }
        
        _requestLine = header[0];
        _headerLines = mUtils.ParseHeaders(String.Join("\n",header));
        _body = lines[1];

        return (_requestLine.Length > 1 && _headerLines.Count >= 1); 
    }
    
    public static string GetBodyFromRequest(string request)
    {
        string body; 

        request = request.Trim(); 
        string[] lines = request.Split(new[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        string[] header = lines[0].Split("\n", StringSplitOptions.RemoveEmptyEntries);
        
        // Error check
        if (lines.Length <= 1 || header.Length <= 1)
        {
            return "";
        }
        
        return lines[1];
    }

    public string GetUri()
    {
        if (_requestLine.Length <= 1) {
            return ""; 
        }

        string[] headerFirstLine = _requestLine.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        return headerFirstLine[1].Trim();
    }

    public string GetMethod()
    {
        if (_requestLine.Length <= 1) {
            return ""; 
        }
        
        string[] headerFirstLine = _requestLine.Split(" ");
        return headerFirstLine[0].Trim();
    }

    public override string ToString()
    {
        if (_requestLine.Length < 1) {
            return ""; 
        }
        
        // If no connection header, set Connection:close as default 
        if (!_headerLines.ContainsKey("Connection")) {
            SetConnectionHeader("close");
        }
        
        SetDateHeader();
        SetContentLength();

        string headerLinesText = "";
        
        foreach (KeyValuePair<string, string> entry in _headerLines)
        {
            headerLinesText += entry.Key + ": " + entry.Value + "\r\n";
        }

        return _requestLine + "\r\n" + headerLinesText + "\r\n" + _body + "<eof>"; 
    }
    

    /// <summary>
    /// Empty all the header parts and leave it empty
    /// </summary>
    public void Clear()
    {
        _requestLine = "";
        _headerLines.Clear();
        _body = ""; 
    }
}
