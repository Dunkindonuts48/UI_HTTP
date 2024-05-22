using System.Text;

namespace HTTP_NET_Project;

public abstract class HTTPHeader
{
    protected Dictionary<string, string> _headerLines  =  new Dictionary<string, string>();
    protected string _body                             = "";
    
    /// <summary>
    /// List containing all avaliable header attributes 
    /// </summary>
    public static List<string> headerAttributesKeys = new List<string> {
        "A-IM",
        "Accept",
        "Accept-Charset",
        "Accept-Datetime",
        "Accept-Encoding",
        "Accept-Language",
        "Access-Control-Request-Headers",
        "Access-Control-Request-Method",
        "Authorization",
        "Cache-Control",
        "Connection",
        "Content-Disposition",
        "Content-Encoding",
        "Content-Language",
        "Content-Length",
        "Content-Location",
        "Content-MD5",
        "Content-Range",
        "Content-Security-Policy",
        "Content-Type",
        "Cookie",
        "Date",
        "Expect",
        "Forwarded",
        "From",
        "Host",
        "If-Match",
        "If-Modified-Since",
        "If-None-Match",
        "If-Range",
        "If-Unmodified-Since",
        "Keep-Alive",
        "Last-Event-ID",
        "Last-Modified",
        "Link",
        "Location",
        "Max-Forwards",
        "Origin",
        "Pragma",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "Public-Key-Pins",
        "Range",
        "Referer",
        "Retry-After",
        "Sec-WebSocket-Accept",
        "Sec-WebSocket-Extensions",
        "Sec-WebSocket-Key",
        "Sec-WebSocket-Protocol",
        "Sec-WebSocket-Version",
        "Server",
        "Set-Cookie",
        "Strict-Transport-Security",
        "TE",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade",
        "User-Agent",
        "Vary",
        "Via",
        "Warning",
        "WWW-Authenticate",
        "X-Content-Type-Options",
        "X-Frame-Options",
        "X-XSS-Protection"
    };
    
    public static Dictionary<string, string> _mimeTable = new Dictionary<string, string> {
                { ".html",      "text/html" },
                { ".css",       "text/css" },
                { ".js",        "text/javascript" },
                { ".vue",       "text/html" },
                { ".xml",       "text/xml" },
                { ".atom",      "application/atom+xml" },
                { ".fastsoap",  "application/fastsoap" },
                { ".gzip",      "application/gzip" },
                { ".json",      "application/json" },
                { ".map",       "application/json" },
                { ".pdf",       "application/pdf" },
                { ".ps",        "application/postscript" },
                { ".soap",      "application/soap+xml" },
                { ".sql",       "application/sql" },
                { ".xslt",      "application/xslt+xml" },
                { ".zip",       "application/zip" },
                { ".zlib",      "application/zlib" },
                { ".aac",       "audio/aac" },
                { ".ac3",       "audio/ac3" },
                { ".mp3",       "audio/mpeg" },
                { ".ogg",       "audio/ogg" },
                { ".ttf",       "font/ttf" },
                { ".bmp",       "image/bmp" },
                { ".emf",       "image/emf" },
                { ".gif",       "image/gif" },
                { ".jpg",       "image/jpeg" },
                { ".jpm",       "image/jpm" },
                { ".jpx",       "image/jpx" },
                { ".jrx",       "image/jrx" },
                { ".png",       "image/png" },
                { ".svg",       "image/svg+xml" },
                { ".tiff",      "image/tiff" },
                { ".wmf",       "image/wmf" },
                { ".http",      "message/http" },
                { ".s-http",    "message/s-http" },
                { ".mesh",      "model/mesh" },
                { ".vrml",      "model/vrml" },
                { ".csv",       "text/csv" },
                { ".plain",     "text/plain" },
                { ".richtext",  "text/richtext" },
                { ".rtf",       "text/rtf" },
                { ".rtx",       "text/rtx" },
                { ".sgml",      "text/sgml" },
                { ".strings",   "text/strings" },
                { ".url",       "text/uri-list" },
                { ".H264",      "video/H264" },
                { ".H265",      "video/H265" },
                { ".mp4",       "video/mp4" },
                { ".mpeg",      "video/mpeg" },
                { ".raw",       "video/raw" }
            };


    // ----------------- HEADER METHODS ---------------
    
    /// <summary>
    /// Sets the specified header as key, with the value provided and is saved as a header attribute.
    /// </summary>
    /// <param name="key">Header attribute name</param>
    /// <param name="value">Value of the header attribute</param>
    public void SetHeader(string key, string value)
    {
        if (!headerAttributesKeys.Contains(key))
        {
            Console.ForegroundColor = ConsoleColor.Red; 
            Console.WriteLine("Incorrect header attribute: " + key);
        }
        if (_headerLines.ContainsKey(key)) {
            _headerLines.Remove(key); 
        }
        _headerLines.Add(key, value);
    }

    /// <summary>
    /// Sets the connection header attributte of the HTTP Request.
    /// Can only have two possible values: "close" or "keep-alive", other values will fail.
    /// </summary>
    /// <param name="connectionStatus">Connection status value. Can be either "close" or "keep-alive"</param>
    public void SetConnectionHeader(string connectionStatus)
    {
        if (_headerLines.ContainsKey("Connection"))
        {
            _headerLines.Remove("Connection"); 
        }
        
        if (connectionStatus != "close" && connectionStatus != "keep-alive" && connectionStatus != "upgrade")
        {
#if DEBUG_MODE
            HTTPClient.ClientAssert("Connection status value is not correct: " + connectionStatus);
#endif
            return; 
        }
        _headerLines.Add("Connection", connectionStatus);
    }
    
    /// <summary>
    /// Sets the Date header attribute by getting the current time of the device in UTC and correctly format it
    /// </summary>
    public void SetDateHeader()
    {
        if (_headerLines.ContainsKey("Date")) {
            _headerLines.Remove("Date"); 
        }

        // Get time and format as Date header stablished
        string dateHeaderValue = DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");
        
        _headerLines.Add("Date", dateHeaderValue);
    }

    /// <summary>
    /// Sets the Content-Length header attribute by calculating the header's body size in bytes
    /// </summary>
    public void SetContentLength()
    {
        if (_headerLines.ContainsKey("Content-Length")) {
            _headerLines.Remove("Content-Length"); 
        }
        int sizeInBytes = _body.Length * sizeof(char); 
        _headerLines.Add("Content-Length", sizeInBytes.ToString());
    }
    
    /// <summary>
    /// Sets the Cache-Control: max-age=... header attribute that defines the time that the item can be cached
    /// If maxAge parameter is set as 0, attribute will be assigned as "no-cache", not max-age=0.
    /// If maxAge parameter is set to under than 0, attribute will be asigned as "no-store", not max-age=-x
    /// </summary>
    /// <param name="maxAge">max age time in seconds. By default 3600 seconds (1 hour)</param>
    public void SetCacheControl( uint maxAge = 3600)
    {
        if (_headerLines.ContainsKey("Cache-Control")) {
            _headerLines.Remove("Cache-Control"); 
        }
        
        if (maxAge == 0)
        {
            _headerLines.Add("Cache-Control", "no-cache");
            return; 
        }
        else if (maxAge < 0)
        {
            _headerLines.Add("Cache-Control", "no-store");
            return;
        }

        _headerLines.Add("Cache-Control", $"max-age={maxAge}"); 
    }
    
    /// <summary>
    /// Sets the server name header line
    /// </summary>
    public void SetServerHeader()
    {
        if (_headerLines.ContainsKey("Server"))
        {
            _headerLines.Remove("Server"); 
        }
        
        _headerLines.Add("Server", "MyServer/1.0.0 (Windows)");
    }
    
    
    // ----------------- BODY GETTERS AND SETTERS ---------------
    
    /// <summary>
    /// Sets the body of the HTTP header
    /// </summary>
    /// <param name="body">Body in string to set</param>
    public void SetBody(string body) {
        _body = body; 
    }



    /// <summary>
    /// Append the appended to the current body concatenated in the end of the current setted body
    /// </summary>
    /// <param name="appended">New concatenated body to the current one </param>
    public void AppendBody(string appended)
    {
        _body += appended; 
    }

    /// <summary>
    /// Getter of _body part of the response
    /// </summary>
    /// <returns></returns>
    public string GetBody()
    {
        return _body; 
    }


    public static bool ValidateHeader(string headerName)
    {
        return headerAttributesKeys.Contains(headerName); 
    }

    public enum CacheDirective
    {
        NON_VALID = -1, 
        MAX_AGE = 0, 
        NO_CACHE, 
        NO_STORE, 
        MUST_REVALIDATE, 
    }

    public CacheDirective GetCacheDirective()
    {
        if (!_headerLines.ContainsKey("Cache-Control")){
            return CacheDirective.NON_VALID; 
        }
        
        // max-age=1000,must-revalidate
        string cacheControl = _headerLines["Cache-Control"];

        if (cacheControl.IndexOf("must-revalidate") > -1)
        {
            return CacheDirective.MUST_REVALIDATE; 
        }
        if (cacheControl.IndexOf("max-age") > -1)
        {
            return CacheDirective.MAX_AGE; 
        }
        if (cacheControl.IndexOf("no-cache") > -1)
        {
            return CacheDirective.NO_CACHE; 
        }
        if (cacheControl.IndexOf("no-store") > -1)
        {
            return CacheDirective.NO_STORE; 
        }

        return CacheDirective.NON_VALID; 
    }
    
    /// <summary>
    /// Return max-age Cache-Control value if Cache-Control header attribute is set and contains it and  not "must-revalidate" or "no-store" and can be parsed into int
    /// </summary>
    /// <returns>max-age Cache-Control value</returns>
    public int GetCacheMaxAge()
    {
        if (!_headerLines.ContainsKey("Cache-Control"))
        {
            return 0; 
        }
        // max-age=1000,must-revalidate
        string cacheControl = _headerLines["Cache-Control"];

        if (cacheControl.IndexOf("max-age") > -1 && cacheControl.IndexOf("must-revalidate") < 0 && cacheControl.IndexOf("no-store") < 0)
        {
            string[] parts = cacheControl.Split("=", StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2) {return 0;}

            int maxAgeSeconds = 0;
            if (int.TryParse(parts[1], out maxAgeSeconds))
            {
                return maxAgeSeconds; 
            }
            else
            {
                return 0; 
            }
            
            
        }

        return 0; 
    }
    
    
    /// <summary>
    /// Return the header built in bytes
    /// </summary>
    /// <returns>Bytes of the header converted</returns>
    public byte[] ToBytes()
    {
        return Encoding.ASCII.GetBytes(this.ToString()); 
    }
}