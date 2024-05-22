using System.Net; 
using System.Net.Sockets;
using System.Text; // Allows UTF-8 and ASCII encoding
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text.Json;
using UnityEngine;
using System; 
using HTTP_NET_Project;
using System.IO;
using System.Threading;
using System.Collections.Generic;


public class HTTPServer
{
    // --------- CONNECTION VARIABLES ---------
    private string _serverName = "MyServer/1.0.0 (Windows)";
    private Uri _serverUri; //new Uri("www.monkey.com");
    private string _serverIP = "127.0.0.1";
    private int _serverPort = 1100;
    
    private Socket _socketListener;     
    private string _rootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).FullName).FullName).FullName;
    private bool _closeConnection = false; 
    
    // --------- MULTITASKING VARIABLES ---------
    private Thread[] _threads = new Thread[10];
    private uint _numThreads = 0; 
    
    // --------- ENDPOINT VARIABLES ---------
    private List<Cat> listCats = new List<Cat>();
    private List<PNG> listPNGs = new List<PNG>();

    // --------- USER VARIABLES ---------
    private UserManager _userManager = new UserManager(); //---------------------------.    
    
    // --------- RESOURCES AND FILES LISTING VARIABLES ---------
    private readonly string _publicFolderRelativePath = "/public"; 
    
    private readonly string[] _avaliableFiles = { "/index.html", "/example.json", "/fol/example.xml", "gustavo.jpg" };
    private readonly string[] _avaliableResources = { "/cats", "/users" , "/png"};
    
    JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyProperties = true,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    

    public void Initialize()
    {
        // Starts initial 
        _userManager.CreateAdminUser();
        
        // Create default cats for the list
        listCats.Add(new Cat("Toby", "Persa", "male", 1, "nobody"));
        listCats.Add(new Cat("Whiskas", "Persa", "female", 31, "nobody"));
        
        // get localhost IP lists and select first one
        // IPHostEntry host = Dns.GetHostEntry("localhost");
        // IPAddress ipAddress = host.AddressList[0];
        IPAddress ipAddress = IPAddress.Parse(_serverIP);
        
        try
        {
            // Not problem because is working on Multithreading
            while (true)
            {
                _socketListener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
                _socketListener.Bind(new IPEndPoint(ipAddress, _serverPort));
                _socketListener.Listen(1); 
        
                ServerAssert("Waiting for a connection at " + ipAddress.ToString() + ":" + _serverPort);

                Socket connectionSocket = _socketListener.Accept();
                connectionSocket.Blocking = true; 

                while (!_closeConnection)
                {
                    // --------- Receive the request ---------
                    string data = ""; 
                    byte[] bytes = null;
                    string response = ""; 
            
                    while (true)
                    {
                        bytes = new byte[1024];
                        int numBytesReceived = connectionSocket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, numBytesReceived);
                
                        // if (numBytesReceived == 0) break;
                        if (data.IndexOf("<eof>") > -1) { break; }
                    }
                    // Create a response to the current request, handling errors and other situations
                    response = HandleResponse(data);
                
            
                    ServerAssert("Message received: " + data);
            
                    // --------- Send the response ---------
                    byte[] responseBytes = Encoding.ASCII.GetBytes(response); 
                    int responseBytesSend = 0;
                    while (responseBytesSend < responseBytes.Length)
                    {
                        responseBytesSend += connectionSocket.Send(responseBytes, responseBytesSend, responseBytes.Length - responseBytesSend, SocketFlags.None);
                    }
            
                    ServerAssert("Response sended");
                }
            }
        }
        catch (Exception excep)
        {
            ErrorAssert("Exception not caught handled: " + excep.Message + " in line " + excep.StackTrace);
        }
    }

    private string HandleResponse(string request)
    {
        // Clean <eof>
        string cleanedRequest = mUtils.GetUntilOrEmpty(request, "<eof>");
        if (cleanedRequest.Length > 0) { request = cleanedRequest;}

        HTTPRequest rq = new HTTPRequest();
        rq.ParseHeader(cleanedRequest); 
        
        ServerAssert("Request received: \n" + request);
        
        HTTPResponse response = new HTTPResponse(); 
        
        // Interpret the request received by the client
        string[] lines = request.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        Dictionary<string, string> headers = mUtils.ParseHeaders(request);

        string body = HTTPRequest.GetBodyFromRequest(request); //lines.Last().Trim(); //mUtils.GetUntilOrEmpty(lines.Last(), "<eof>"); 
        
        string[] headerFirstLine = lines[0].Split(" ");
        string method = headerFirstLine[0];
        
        string uri = headerFirstLine[1].Trim();
        string findPath = _rootPath + _publicFolderRelativePath + uri;
        
        float httpVersion = mUtils.ParseHttpVersion(headerFirstLine[2]);
        
        
        //Token Validation for Protected Endpoints // ------------------------------------_
        if (uri.StartsWith("/users/") || uri.StartsWith("/users") || uri.StartsWith("/login")) //protected endpoints
        {
            if (!headers.ContainsKey("Authorization") || !headers["Authorization"].StartsWith("Bearer "))
            {
                response = HTTPResponse.Get401Unathorized("{\"error\":\"Not Authorization header or not Bearer declared\"}");
                return response.ToString();
            }
            
            string token = headers["Authorization"].Substring("Bearer ".Length).Trim();
            
            if (!_userManager.ValidateSession(token))
            {
                response.SetStatusLine(httpVersion, 401);
                response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                response.SetHeader("Content-Language", "en-US, es-ES");
                response.SetHeader("Content-Type", HTTPHeader._mimeTable[Path.GetExtension(uri).Trim()]);
                response.SetCacheControl(0);
                response.SetBody("{\"error\":\"Invalid or expired token\"}");
                response.SetContentLength();
                return response.ToString();
            }
        } //------------------------------------^
        
        // TODO Delete this, only for testing
        TestAssert("--------- AUTHORIZED CORRECTLY----------");
        
        #region VALIDATION OF REQUEST
        // Wrong connection header  ----> 404 BAD REQUEST
        if (!headers.ContainsKey("Connection") ||
            (headers["Connection"] != "close" && headers["Connection"] != "keep-alive"))
        {
            return HTTPResponse.Get400DefaultBadRequestHeader("{\"error\":\"No connection header specified\"}").ToString(); 
        }
        
        // If Connection: close, stop the server for waiting more responses
        if (headers.ContainsKey("Connection") &&
            headers["Connection"].Contains("close", StringComparison.CurrentCultureIgnoreCase))
        {
            _closeConnection = true; 
        }
        
        // If Content.Length mismatches the actual body length ---> 411 length fault
        if ((!headers.ContainsKey("Content-Length") && body.Length > 0 )||
            ((body.Length * sizeof(char)) != int.Parse(headers["Content-Length"])))
        {
            ErrorAssert("Body error. length calcualted: " + body.Length * sizeof(char) + " actual body: " + body);
            return HTTPResponse.Get411DefaultLengthRequiredRequestHeader().ToString(); 
        }

        if (uri.Length >= 50)
        {
            return HTTPResponse.Get414DefaultUriTooLongRequestHeader().ToString(); 
        }

        // if http version is lower than 1.1, response client to upgrade it
        if (httpVersion < 1.1)
        {
            return HTTPResponse.Get101DefaultSwitchingHttpVersionHeader().ToString(); 
        }
        
        // Check if resource exist and can be accesed  by that method
        if (!mUtils.StringContainsOne(uri, _avaliableFiles) && !mUtils.StringContainsOne(uri, _avaliableResources))
        {
            return HTTPResponse.Get404DefaultNotFoundHeader().ToString(); 
        }
        
        #endregion
        
        // HANDLE THE RESPONSE TO THE REQURST 
        switch (method)
        {
            case "HEAD":
            case "GET":
                //Handle User Retrieval ------------------------------------------_
                if (uri.StartsWith("/user"))
                { 
                    string username = uri.Substring("/user/".Length);
                    var user = _userManager.GetUser(username);
                    if (user != null)
                    {
                        response.SetStatusLine(httpVersion, 200);
                        response.SetBody(JsonSerializer.Serialize(new { Username = user.Username }));
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 404);
                        response.SetBody("User not found");
                    }

                    break; // {DANI} Added new
                }
                //break; //------------------------------------------^

                if (uri.Contains("/cats") || uri.Contains("/cats/") || uri.Contains("cats/"))
                {
                    if (listCats.Count <= 0)
                    {
                        // No cats in list
                        response.SetStatusLine(1.1f, 200);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Content-Type", HTTPHeader._mimeTable[Path.GetExtension(uri).Trim()]);
                        response.SetCacheControl(60);
                        if(method == "GET") response.SetBody("{\"success\":\"No cats stored\"}");     // Attach response content
                    }
                    else
                    {
                        // Return JSON with all cats
                        string json = ""; 
                        try
                        {
                            json = JsonSerializer.Serialize(listCats, new JsonSerializerOptions { WriteIndented = true });
                        }
                        catch (NotSupportedException e)
                        {
                            ErrorAssert("Unable to transform cats list into json: " + e.Message);
                            response = HTTPResponse.Get500InternalServerErrorHeader();
                            break;
                        }

                        if (json.Length <= 1)
                        {
                            response = HTTPResponse.Get500InternalServerErrorHeader();
                            break;
                        }
                        
                        response.SetStatusLine(1.1f, 200);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Content-Type", HTTPHeader._mimeTable[".json"]);
                        response.SetCacheControl(60);
                        if(method == "GET") response.SetBody(json);     // Attach response content
                    }

                    break; 
                }

                if (File.Exists(findPath))
                {
                    string fileExtension = Path.GetExtension(uri); 
                    ServerAssert("File exists and have extension: " + Path.GetExtension(uri));

                    // If accept same mime as asked in the uri (If ask for index.html, Accept: must contain text/html or all data types "Accept:*/*"
                    if (headers["Accept"].IndexOf(HTTPHeader._mimeTable[Path.GetExtension(uri).Trim()]) > -1 || headers["Accept"] == "*/*")
                    {
                        response.SetStatusLine(1.1f, 200);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Content-Type", HTTPHeader._mimeTable[Path.GetExtension(uri).Trim()]);
                        response.SetCacheControl(60);
                        //if(method == "GET") response.SetBody(File.ReadAllText(findPath));     // Attach response content
                        if (uri.Trim().Contains("text/html") || uri.Trim().Contains("application/json") || uri.Trim().Contains("text/xml"))
                        {
                            response.SetBody(File.ReadAllText(findPath));
                        }
                        else
                        {
                            byte[] file = File.ReadAllBytes(findPath);
                            string fileToString = Convert.ToBase64String(file);
                            response.SetBody(fileToString);
                            //response.SetBody(file.ToString());
                        }
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 406);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Accept", "text/html");
                        if(method == "GET") response.SetBody("{\"406error\": \"Incorrect Accept: heade attribute for file extension\"}");
                    }
                }
                else //if (file not exists) 
                {
                    ServerAssert("File dont exists in " + findPath);
                    
                    HTTPResponse notFound404Response = HTTPResponse.Get404DefaultNotFoundHeader();
                    if(method == "HEAD") notFound404Response.SetBody("");
                    return notFound404Response.ToString();
                }
                break; 
            
            case "POST":
                // TODO Handle post header
                //Handle User Login ------------------------------------------_
                if (uri.StartsWith("/login") || uri.StartsWith("login"))
                {
                    var loginRequest = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

                    // // [DANI] How to parse attributes in POST format
                    //loginRequest = mUtils.ParsePostAttributes(body); 

                    if (loginRequest.ContainsKey("username") && loginRequest.ContainsKey("password"))
                    {
                        string username = loginRequest["username"];
                        string password = loginRequest["password"];
                        if (_userManager.ValidateUser(username, password))
                        {
                            SessionToken session = _userManager.CreateSession(username);
                            response.SetStatusLine(httpVersion, 200);
                            response.SetBody(JsonSerializer.Serialize(new { token = session.Token }));
                        }
                        else
                        {
                            response.SetStatusLine(httpVersion, 401);
                            response.SetBody("Invalid username or password");
                        }
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 400);
                        response.SetBody("Invalid request format");
                    }
                }
                //Handle User Creation 
                else if (uri.StartsWith("/users") || uri.StartsWith("users"))
                {
                    User user = JsonSerializer.Deserialize<User>(body);
                    if (_userManager.CreateUser(user.Username, user.Password))
                    {
                        response.SetStatusLine(httpVersion, 201);
                        response.SetBody("{\"success\":\"User created\"}");
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 409);
                        response.SetBody("{\"error\":\"User already exists\"}");
                    }
                } //------------------------------------------^
                else if (uri.Contains("/cats") || uri.Contains("cats"))
                {
                    // Read the body of the request
                    //string jsonTestPost = "{\"name\": \"Mittens\",\"breed\": \"Siamese\",\"age\": 3, \"color\": \"black\", \"owner\": \"ALFREDO PEREZ FANTOVA\"}";

                    // Reads JSON from the body to add a cat
                    Cat? catPost = null;
                    try
                    {
                        catPost = JsonSerializer.Deserialize<Cat>(body, _jsonOptions);
                    }
                    catch (JsonException excep)
                    {
                        ErrorAssert("Unable to parse JSON, body was: " + body);
                        response = HTTPResponse.Get400DefaultBadRequestHeader();
                        break; 
                    }
                    catch (NotSupportedException excep)
                    {
                        ErrorAssert("JSON parsing not supported");
                        response = HTTPResponse.Get500InternalServerErrorHeader();
                        break; 
                    }
                    
                    listCats.Add(catPost);

                    response.SetStatusLine(httpVersion, 201);
                    response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                    response.SetHeader("Content-Language", "en-US, es-ES");
                    response.SetHeader("Content-Type", HTTPHeader._mimeTable[".json"]);
                    response.SetCacheControl(60);
                    response.SetBody("{\"success\":\"Cat added succesfully\"}");
                    return response.ToString();
                }
                else if (uri.Contains("/png") || uri.Contains("png"))
                {
                    byte[] imageData = Convert.FromBase64String(body);
                    PNG pngImage = new PNG("uploaded_image.png", imageData);

                    // Save the image or process it
                    string imagePath = Path.Combine(_rootPath + _publicFolderRelativePath, pngImage.FileName);
                    File.WriteAllBytes(imagePath, pngImage.Data);

                    listPNGs.Add(pngImage);

                    response.SetStatusLine(httpVersion, 200);
                    response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                    response.SetHeader("Content-Language", "en-US, es-ES");
                    response.SetHeader("Content-Type", HTTPHeader._mimeTable[".png"]);
                    response.SetCacheControl(0);
                    response.SetBody("{\"success\":\"Image uploaded successfully\"}");
                    return response.ToString();
                }
                
                break;
            
            case "PUT": // UPDATE IN DB
                
                //Handle User Update ------------------------------------------_
                if (uri.StartsWith("/users")) // uri.StartsWith == "/user/"
                {
                    string username = uri.Substring("/user/".Length);
                    User user = JsonSerializer.Deserialize<User>(body);
                    if (_userManager.UpdateUser(username, user.Password))
                    {
                        response.SetStatusLine(httpVersion, 200);
                        response.SetBody("User updated successfully");
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 404);
                        response.SetBody("User not found");
                    }
                }//------------------------------------------^
                else if (uri.Contains("/cats") || uri.Contains("/cats/"))
                {
                    string catName = uri.Substring("/cats/".Length);

                    // Parse into JSOn the cat passed as JSON in the request body
                    Cat? catPut = null;
                    try
                    {
                         catPut = JsonSerializer.Deserialize<Cat>(body, _jsonOptions);
                    }
                    catch (JsonException jsonException)
                    {
                        ErrorAssert("Json error, body don't contain json: body=" + body + " Exception: " + jsonException.Message);
                        response = HTTPResponse.Get400DefaultBadRequestHeader();
                    }
                    catch (NotSupportedException notSupportedException)
                    {
                        ErrorAssert("Json parsing error: " + notSupportedException.Message);
                        response = HTTPResponse.Get500InternalServerErrorHeader();
                        break; 
                    }

                    response.SetBody("{\"error\":\"Unable to find resource\"}");
                    
                    for(int i = 0; i < listCats.Count; i++)
                    {
                        if (listCats[i].Name == catName)
                        {
                            listCats[i] = catPut;
                            response.SetBody("{\"success\":\"Cat added succesfully\"}");
                        }
                    }

                    response.SetStatusLine(httpVersion, 200);
                    response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                    response.SetHeader("Content-Language", "en-US, es-ES");
                    response.SetHeader("Content-Type", HTTPHeader._mimeTable[".json"]);
                    response.SetCacheControl(0);
                    response.SetContentLength();
                     return response.ToString();
                }
                break; 
            case "DELETE": // DELETE IN DB
                // TODO Handle delete header
                /*
                    //Handle User Deletion ------------------------------------------_
                    if (uri.StartsWith("/user/"))
                    {
                        var username = uri.Substring("/user/".Length);
                        if (_userManager.DeleteUser(username))
                        {
                            response.SetStatusLine(httpVersion, 200);
                            response.SetBody("User deleted successfully");
                        }
                        else
                        {
                            response.SetStatusLine(httpVersion, 404);
                            response.SetBody("User not found");
                        }
                    }
                    break; //------------------------------------------^

                string jsonTest = "{\"name\": \"Mittens\"}";

                Cat catDelete = JsonSerializer.Deserialize<Cat>(jsonTest); 

                foreach (Cat c in listCats)
                {
                    if (c.Name == catDelete.Name)
                    {
                        listCats.Remove(c);
                    }
                }

                response.SetStatusLine(httpVersion, 200);
                break;
                */
                if (uri.StartsWith("/users") || uri.Contains("/users")) 
                {
                    string username = uri.Substring("/users/".Length);
                    if (_userManager.DeleteUser(username))
                    {
                        response.SetStatusLine(httpVersion, 200);
                        response.SetBody("User deleted successfully");
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 404);
                        response.SetBody("User not found");
                    }
                } //------------------------------------------^
                else if (uri == "/cats")
                {
                    string catName = uri.Substring("/cats/".Length);
                    string jsonTestDelete = "{\"name\": \"Mittens\"}";

                    Cat catDelete = JsonSerializer.Deserialize<Cat>(jsonTestDelete);

                    response.SetBody("{\"error\"}");
                    foreach (Cat c in listCats)
                    {
                        if (c.Name == catDelete.Name)
                        {
                            listCats.Remove(c);
                        }
                    }

                    response.SetStatusLine(httpVersion, 200);
                }
                break;
        }

        // Close connection when header Connection: close is received
        // if (headers.ContainsKey("Connection") && headers["Connection"] == "close")
        // {
        //     _closeConnection = true; 
        // }

        return response.ToString(); 
    }
    

    /// <summary>
    /// Initialize a thread for the initialization of the server.
    /// This is usefull for testing server in same device as client
    /// </summary>
    public void InitializeThread()
    {
        _threads[_numThreads] = new Thread(Initialize); 
        _threads[_numThreads].Start();
        _numThreads++; 
    }
    
    
    /// <summary>
    /// Assert or log into the console by adding Server> after the msg in order to differenciate messages from client and server
    /// </summary>
    /// <param name="msg">Message string to assert</param>
    public static void ServerAssert(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Magenta; 
        Console.WriteLine("Server> " + msg);
    }

    public static void TestAssert(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(msg); 
    }

    public static void ErrorAssert(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red; 
        Console.WriteLine("Error> " + msg);
    }
    
    // ---/---/---/---/ GETTERS AND SETTERS \---\---\---\---\
    
    public string ServerName
    {
        get { return _serverName;  }
    }
}



// https://www.c-sharpcorner.com/article/socket-programming-in-C-Sharp/
// // https://www.c-sharpcorner.com/article/socket-programming-in-C-Sharp/
// TODO Delete this links


// Response informations
// https://diego.com.es/respuesta-http