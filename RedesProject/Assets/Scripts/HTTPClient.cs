using System.Text;
using System.Net;
using System.Net.Sockets;       // Networking library
using System.Text.Json;
using HTTP_NET_Project; // Allows UTF-8 and ASCII encoding


public class HTTPClient
{
    private HttpListener _listener;
    private bool _connected = false;

    // Connection data
    private Uri _serverUri;
    private string _serverIP = "127.0.0.1";
    private int _serverPort = 1100;
    
    private int _clientPort = 1100;
    private Socket _socket;

    // ------------ SESSION CONTROL ------------
    private string _sessionToken; //-------------------------.
    
    // ------------ CACHE CONTROL ------------
    private DateTime _startTime; 
    private Dictionary<string, CacheItem<string>> _cache = new Dictionary<string, CacheItem<string>>(); 

    public void Initialize()
    {
        _serverUri = new Uri("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName");
        
         /* Uri uri = new Uri("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName");
           Console.WriteLine($"AbsolutePath: {uri.AbsolutePath}"); Console.WriteLine($"AbsoluteUri: {uri.AbsoluteUri}"); Console.WriteLine($"DnsSafeHost: {uri.DnsSafeHost}"); Console.WriteLine($"Fragment: {uri.Fragment}");
           Console.WriteLine($"Host: {uri.Host}");
           Console.WriteLine($"HostNameType: {uri.HostNameType}"); Console.WriteLine($"IdnHost: {uri.IdnHost}");
           Console.WriteLine($"IsAbsoluteUri: {uri.IsAbsoluteUri}");
           Console.WriteLine($"IsDefaultPort: {uri.IsDefaultPort}"); Console.WriteLine($"IsFile: {uri.IsFile}");
           Console.WriteLine($"IsLoopback: {uri.IsLoopback}");
           Console.WriteLine($"IsUnc: {uri.IsUnc}");
           Console.WriteLine($"LocalPath: {uri.LocalPath}"); Console.WriteLine($"OriginalString: {uri.OriginalString}"); Console.WriteLine($"PathAndQuery: {uri.PathAndQuery}"); Console.WriteLine($"Port: {uri.Port}");
           Console.WriteLine($"Query: {uri.Query}");
           Console.WriteLine($"Scheme: {uri.Scheme}");
           Console.WriteLine($"Segments: {string.Join(", ", uri.Segments)}"); Console.WriteLine($"UserEscaped: {uri.UserEscaped}"); Console.WriteLine($"UserInfo: {uri.UserInfo}");
         */

        _startTime = DateTime.UtcNow; 
        
    }
    
    public void ConnectSocket()
    {
        if (_socket != null && _socket.Connected) {
            ClientAssert("Cannot reconnect to socket");
            return; 
        }
        // Create socket to start TCP connection
        Socket auxSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //auxSocket.Connect(host: _serverUri.Host, port: _serverPort);  
        IPAddress serverIPAddress = IPAddress.Parse(_serverIP); 
        auxSocket.Connect(serverIPAddress, _serverPort);
        ClientAssert("Connecting to: " + serverIPAddress + ":" + _serverPort);

        if (auxSocket.Connected)
        {
            // Save socket
            _socket = auxSocket;
            _socket.NoDelay = true;
            _connected = true; 
            
            ClientAssert("Socket connected");
            return; 
        }
        
        ClientAssert("Unable to connect socket -> TCP connection failed");
    }

    /// <summary>
    /// Sends an http sequest to the server and return the server response when all the response is received
    /// </summary>
    /// <param name="request">Request to send to the server</param>
    /// <returns>Response in string that is sended back by the server</returns>
    public string SendHttpRequest(HTTPRequest request)
    {
        //add session token to header //------------------------------------_
        if (!string.IsNullOrEmpty(_sessionToken))
        {
            request.SetHeader("Authorization", "Bearer " + _sessionToken);
        } //-------------------------------------------------------------^

        // --------------- CHECK IF SAVED IN CACHE ---------------
        string inCache = CheckAndRetreiveFromCache(request.GetUri());
        if (inCache != "")
        {
            return inCache; 
        }
        
        
        byte[] requestBytes = request.ToBytes(); 
        int bytesSent = 0;
        while (bytesSent < requestBytes.Length)
        {
            bytesSent += _socket.Send(requestBytes, bytesSent, requestBytes.Length - bytesSent, SocketFlags.None);
        }
        
        // --------------- RECEIVE SERVER RESPONSE ---------------
        byte[] responseBytes = new byte[256];
        string responseString = ""; 
        ClientAssert("Waiting response");
        while (true)
        {
            int bytesReceived = _socket.Receive(responseBytes);
            
            responseString += Encoding.ASCII.GetString(responseBytes, 0, bytesReceived);
            if (responseString.IndexOf("<eof>") > -1) { break; }
            if (bytesReceived == 0) { break; }
        }

        // Trim from end of string received the <eof>
        responseString = mUtils.GetUntilOrEmpty(responseString, "<eof>"); 
        ClientAssert("Response received: \n" + responseString);
        
        HTTPResponse response = new HTTPResponse();
        response.ParseHeader(responseString);

        //Check if the response contains a session token and store it
        if (response.GetStatusLine().Contains("200 OK") && response.GetBody().Contains("token"))
        {
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(response.GetBody());
            if (jsonResponse.ContainsKey("token"))
            {
                _sessionToken = jsonResponse["token"];
            }
        } //------------------------------------^
        
        
        // --------------- CACHE ELEMENTS ---------------
        
        if (response.GetCacheDirective() == HTTPHeader.CacheDirective.MAX_AGE)
        {
            int cacheTime = response.GetCacheMaxAge();   
            _cache.Add(request.GetUri(), CacheItem<String>.ExpirableCacheItem(response.ToString(), cacheTime));
        }
        
        return responseString; 
    }

    public void Disconnect()
    {
        if (_socket != null) {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
    }

    /// <summary>
    /// Check if the specified uri is stored in cache. If stored, return the http header stored,
    /// if its not stored or has expired, return "" empty string
    /// </summary>
    /// <param name="uri">uri requested to check if its stored in cache </param>
    /// <returns>Cache of a the specified uri or "" empty string if has expired or has no cache of it</returns>
    public string CheckAndRetreiveFromCache(string uri)
    {
        if (StoredInCache(uri))
        {
            if (!_cache[uri].HasExpired())
            {
                return _cache[uri].GetData(); 
            }
            else
            {
                _cache.Remove(uri);
            }
        }
        
        return ""; 
    }

    public bool StoredInCache(string url)
    {
        return _cache.ContainsKey(url); 
    }
    
    public void DeleteCache()
    {
        _cache.Clear();
    }

    
    public static void ClientAssert(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Cyan; 
        Console.WriteLine("Client> " + msg); 
    }
}


// ------------------------

// Header examples: 
//https://diego.com.es/headers-del-protocolo-http