#define DEBUG_MODE
using System; 
using System.Collections.Generic;

 
using System.Text;

using HTTP_NET_Project;

public class HTTPResponse : HTTPHeader
{
    private static Dictionary<int, string> _statusCodes = new Dictionary<int, string>
        {
            { 100, "Continue" },
            { 101, "Switching Protocols" },
            { 102, "Processing" },
            { 103, "Early Hints" },

            { 200, "OK" },
            { 201, "Created" },
            { 202, "Accepted" },
            { 203, "Non-Authoritative Information" },
            { 204, "No Content" },
            { 205, "Reset Content" },
            { 206, "Partial Content" },
            { 207, "Multi-Status" },
            { 208, "Already Reported" },
            { 226, "IM Used" },

            { 300, "Multiple Choices" },
            { 301, "Moved Permanently" },
            { 302, "Found" },
            { 303, "See Other" },
            { 304, "Not Modified" },
            { 305, "Use Proxy" },
            { 306, "Switch Proxy" },
            { 307, "Temporary Redirect" },
            { 308, "Permanent Redirect" },

            { 400, "Bad Request" },
            { 401, "Unauthorized" },
            { 402, "Payment Required" },
            { 403, "Forbidden" },
            { 404, "Not Found" },
            { 405, "Method Not Allowed" },
            { 406, "Not Acceptable" },
            { 407, "Proxy Authentication Required" },
            { 408, "Request Timeout" },
            { 409, "Conflict" },
            { 410, "Gone" },
            { 411, "Length Required" },
            { 412, "Precondition Failed" },
            { 413, "Payload Too Large" },
            { 414, "URI Too Long" },
            { 415, "Unsupported Media Type" },
            { 416, "Range Not Satisfiable" },
            { 417, "Expectation Failed" },

            { 421, "Misdirected Request" },
            { 422, "Unprocessable Entity" },
            { 423, "Locked" },
            { 424, "Failed Dependency" },
            { 425, "Too Early" },
            { 426, "Upgrade Required" },
            { 427, "Unassigned" },
            { 428, "Precondition Required" },
            { 429, "Too Many Requests" },
            { 431, "Request Header Fields Too Large" },

            { 451, "Unavailable For Legal Reasons" },

            { 500, "Internal Server Error" },
            { 501, "Not Implemented" },
            { 502, "Bad Gateway" },
            { 503, "Service Unavailable" },
            { 504, "Gateway Timeout" },
            { 505, "HTTP Version Not Supported" },
            { 506, "Variant Also Negotiates" },
            { 507, "Insufficient Storage" },
            { 508, "Loop Detected" },

            { 510, "Not Extended" },
            { 511, "Network Authentication Required" }
        };
    
    private string _statusLine                       = "";
    // private Dictionary<string, string> _headerLines  =  new Dictionary<string, string>();
    // private string _body                             = "";
    
    private static float[] _avaliableHttpVersions = { 0.9f, 1.0f, 1.1f, 1.2f, 2.0f };
    
    // ------------------------- PUBLIC ACCESS ATTRIBUTES -------------------------  

    /// <summary>
    /// Builds the status line for the httpVersion and the statusCode specified.
    /// In case that statusCode is not recognized, assign 500 -> Internal server error status code. 
    /// </summary>
    /// <param name="httpVersion">Specify http version as text</param>
    /// <param name="statusCode">Status code for the status line of the HTTP response header</param>
    public void SetStatusLine(float httpVersion, int statusCode)
    {
        if (!_statusCodes.ContainsKey(statusCode))      // If status line don't exist, error 500 -> Internal server error
        {
            _statusLine = "HTTP/" + httpVersion + " " + statusCode.ToString() + " " + _statusCodes[500];
            return; 
        }

        _statusLine = "HTTP/" + httpVersion + " " + statusCode.ToString() + " " + _statusCodes[statusCode]; 
    }
    
    

    // ------------------------- STATIC METHODS -----------------------
    
#region DEFAULT HEADERS
    public static HTTPResponse Get404DefaultHeader()
    {
        HTTPResponse response404 = new HTTPResponse(); 
        
        response404.SetStatusLine(1.1f, 404);
        response404.SetHeader("Content-Type", _mimeTable[".html"]);
        response404.SetBody(HTTPResponse.default404Response); 
        response404.SetContentLength();
        response404.SetDateHeader();
        response404.SetHeader("Connection", "close");
        
        return response404; 
    }
    
    public static string default404Response =
        "<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    " +
        "<meta charset=\"UTF-8\">\n    " +
        "<title>404 Not Found</title>\n</head>\n<body>\n    " +
        "<h1>404 Not Found</h1>\n    " +
        "<p>The requested URL /page-not-found was not found on this server or its not listed as an avaliable file / resource.</p>\n</body>\n</html>";


    public static HTTPResponse Get400DefaultBadRequestHeader(string body = "")
    {
        HTTPResponse response = new HTTPResponse(); 
        response.SetStatusLine(1.1f, 400);
        response.SetHeader("Content-Type", "text/html; charset=utf-8");
        response.SetConnectionHeader("keep-alive");
        response.SetDateHeader();
        response.SetCacheControl(0);
        response.SetBody(body);
        response.SetContentLength();
        return response; 
    }

    public static HTTPResponse Get401Unathorized(string body = "{\"error\":\"Unathorized\"}")
    {
        HTTPResponse response = new HTTPResponse(); 
        response.SetStatusLine(1.1f, 401);
        response.SetHeader("Content-Type", "text/html; charset=utf-8");
        response.SetConnectionHeader("keep-alive");
        response.SetDateHeader();
        response.SetCacheControl(0);
        response.SetBody(body);
        return response; 
    }

    public static HTTPResponse Get404DefaultNotFoundHeader()
    {
        HTTPResponse notFoundResponse = new HTTPResponse(); 
        notFoundResponse.SetStatusLine(1.1f, 404);
        notFoundResponse.SetServerHeader();
        notFoundResponse.SetDateHeader();
        notFoundResponse.SetConnectionHeader("keep-alive");
        notFoundResponse.SetHeader("Content-Type", _mimeTable[".html"]);
        notFoundResponse.SetBody(HTTPResponse.default404Response);
        notFoundResponse.SetContentLength();
        return notFoundResponse; 
    }

    public static HTTPResponse Get411DefaultLengthRequiredRequestHeader()
    {
        HTTPResponse response = new HTTPResponse(); 
        response.SetStatusLine(1.1f, 411);
        response.SetConnectionHeader("close");
        response.SetDateHeader();
        response.SetBody("");
        response.SetContentLength();
        return response;
    }

    public static HTTPResponse Get414DefaultUriTooLongRequestHeader()
    {
        HTTPResponse response = new HTTPResponse(); 
        response.SetStatusLine(1.1f, 414);
        response.SetServerHeader();
        response.SetConnectionHeader("keep-alive");
        response.SetDateHeader();
        response.SetBody("{\"error\":\"Uri is too long, maximum length 50 characters\"}");
        response.SetContentLength();
        return response;
    }

    /// <summary>
    /// Gets the 101 header to swictch between HTTP protocols
    /// </summary>
    /// <param name="newHttpVersion">New http version to change to</param>
    /// <returns>101 Switching Protocols HTTP header or 500 internal server error if triying to upgrade to a non-existing HTTP version</returns>
    public static HTTPResponse Get101DefaultSwitchingHttpVersionHeader(float newHttpVersion = 1.1f)
    {
        // if (!_avaliableHttpVersions.Contains(newHttpVersion))
        // {
        //     return Get500InternalServerErrorHeader(); 
        // }

        HTTPResponse upgradeResponse = new HTTPResponse(); 
        upgradeResponse.SetStatusLine(newHttpVersion,  10);
        upgradeResponse.SetHeader("Server", "MyServer/1.0.0 (Windows)");
        upgradeResponse.SetConnectionHeader("upgrade");
        upgradeResponse.SetHeader("Upgrade", "HTTP/" + newHttpVersion); 
        upgradeResponse.SetDateHeader();
        upgradeResponse.SetBody("");
        upgradeResponse.SetContentLength();
        return upgradeResponse;
    }

    public static HTTPResponse Get500InternalServerErrorHeader()
    {
        HTTPResponse serverErrorResponse = new HTTPResponse(); 
        serverErrorResponse.SetStatusLine(1.1f,  500);
        serverErrorResponse.SetHeader("Server", "MyServer/1.0.0 (Windows)");
        serverErrorResponse.SetConnectionHeader("close"); 
        serverErrorResponse.SetDateHeader();
        serverErrorResponse.SetCacheControl(0);
        serverErrorResponse.SetBody("");
        serverErrorResponse.SetContentLength();
        return serverErrorResponse;
    }
    
#endregion
    
    // ------------------------- BASIC METHOD -------------------------

    public bool ParseHeader(string request)
    {
        this.Clear();

        request = request.Trim(); 
        string[] lines = request.Split(new[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        string[] header = lines[0].Split("\n", StringSplitOptions.RemoveEmptyEntries);
        
        // Error check
        if (lines.Length <= 1 || header.Length <= 1)
        {
            _statusLine = "";
            _headerLines.Clear();
            _body = "";
            return false; 
        }
        
        _statusLine = header[0];
        _headerLines = mUtils.ParseHeaders(String.Join("\n",header));
        _body = lines[1];

        return true; 
    }
    
    public override string ToString()
    {
        SetDateHeader();
        SetServerHeader();
        
        if (!_headerLines.ContainsKey("Content-Length")) {
            SetContentLength();
        }

        if (!_headerLines.ContainsKey("Cache-Control"))
        {
            _headerLines.Add("Cache-Control", "no-store");
        }
        
        string headerLinesText = "";
        foreach (KeyValuePair<string, string> entry in _headerLines)
        {
            headerLinesText += entry.Key + ": " + entry.Value + "\r\n";
        }

        return _statusLine + "\n" + headerLinesText + "\r\n" + _body + "<eof>";
    }
    

    /// <summary>
    /// Clear the HTTPResponse variables
    /// </summary>
    public void Clear()
    {
        _headerLines.Clear();
        _statusLine = "";
        _body = ""; 
    }

    public string GetStatusLine()
    {
        return _statusLine; 
    }
    
}