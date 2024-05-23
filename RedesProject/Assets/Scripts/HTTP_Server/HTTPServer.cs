using System.Net; 
using System.Net.Sockets;                                   // TCP socket connection for binding and listening
using System.Text;                                          // Allows UTF-8 and ASCII encoding
using System.Reflection;
using System.Security.Cryptography;
//using System.Text.Json;                                     // Import JsonSerializer
using System;  
using System.IO; 
using System.Threading; 
using System.Collections.Generic;

//using UnityEngine; 

using HTTP_NET_Project;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class HTTPServer
{
    // --------- CONNECTION VARIABLES ---------
    private string _serverName = "MyServer/1.0.0 (Windows)";
    private Uri _serverUri; //new Uri("www.monkey.com");
    private string _serverIP = "127.0.0.1";
    private int _serverPort = 1100;
    
    private Socket _socketListener;     
    //private string _rootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).FullName).FullName).FullName;
    private string _rootPath = Application.dataPath; 
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
    
    private readonly string[] _avaliableFiles = { "/index.html", "/example.json", "/fol/example.xml", "gustavo.jpg", "uploaded_image.png" };
    private readonly string[] _avaliableResources = { "/cats", "/users" , "/png", "/login"};
    
    // JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    // {
    //     PropertyNameCaseInsensitive = true,
    //     IgnoreReadOnlyProperties = true,
    //     AllowTrailingCommas = true,
    //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    // };
    

    public void Initialize()
    {
        // Starts initial 
        _userManager.CreateAdminUser();
        Debug.Log("dataPath: " + _rootPath);
        
        // Create default cats for the list
        listCats.Add(new Cat("Toby", "Persa", "male", 1, "nobody"));
        listCats.Add(new Cat("Whiskas", "Persa", "female", 31, "nobody"));
        
        // get localhost IP lists and select first one
        // IPHostEntry host = Dns.GetHostEntry("localhost");
        // IPAddress ipAddress = host.AddressList[0];
        IPAddress ipAddress = IPAddress.Parse(_serverIP);
        
        try
        {
            _socketListener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            _socketListener.Bind(new IPEndPoint(ipAddress, _serverPort));
            Debug.Log("Server listenign");
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
        
                ServerAssert("Waiting for communication...");
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
                
                //ServerAssert("Message received: \n" + data);
        
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
        catch (Exception excep)
        {
            ErrorAssert("Exception not caught handled: " + excep.Message + " in line " + excep.StackTrace);
        }
    }

    /// <summary>
    /// Its the main behaviour of the server defined .
    /// Receive the HTTP Request, handle the method, resource / file requested and endpoinst and return the correct response
    /// </summary>
    /// <param name="request"> Client request</param>
    /// <returns>The server response to the server request</returns>
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
        if (uri.StartsWith("/users/") || uri.StartsWith("/users")) //protected endpoints
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
            (headers["Connection"] != "close" && headers["Connection"] != "keep-alive") && headers["Connection"] != "Upgrade")
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
                if (uri.StartsWith("/users/"))      // if only /users/ , retreive all. otherwise if username specified, retreive it
                { 
                    string username = uri.Substring("/users/".Length);

                    if (username.Length <= 0)
                    {
                        // Return JSON with all cats
                        string json = ""; 
                        try
                        {
                            //json = JsonSerializer.Serialize(_userManager.GetUsersDictionary(), new JsonSerializerOptions { WriteIndented = true });
                            json = JsonConvert.SerializeObject(_userManager.GetUsersDictionary());         
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

                        break; 
                    }
                    
                    var user = _userManager.GetUser(username);
                    if (user != null)
                    {
                        response.SetStatusLine(httpVersion, 200);
                        string bodyJson = ""; 
                        try{ bodyJson = JsonConvert.SerializeObject(user);} catch (Exception e) { ErrorAssert("Unable to serialize user as json: " + e.Message);} 
                        //response.SetBody(JsonSerializer.Serialize(new { user = user}));
                        response.SetBody(bodyJson);
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 404);
                        response.SetBody("User not found");
                    }

                    break; 
                }
                //break; //------------------------------------------^

                if (uri.Contains("/cats/"))
                {
                    if (listCats.Count <= 0)
                    {
                        // No cats in list
                        response.SetStatusLine(1.1f, 200);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Content-Type", HTTPHeader._mimeTable[".json"]);
                        response.SetCacheControl(60);
                        if(method == "GET") response.SetBody("{\"success\":\"No cats stored\"}");     // Attach response content
                    }
                    else
                    {
                        string catName = uri.Substring("/cats/".Length).Trim();

                        if (catName.Length <= 0) // Uri = /cats/ so get all cats
                        {
                            // Return JSON with all cats
                            string json = "";

                            try
                            {
                                json = JsonConvert.SerializeObject(listCats);
                            }
                            catch (NotSupportedException jsonException)
                            {
                                ErrorAssert("Unable to transform cats list into json: " + jsonException.Message);
                                response = HTTPResponse.Get500InternalServerErrorHeader();
                                break;
                            }
                            catch (Exception exception)
                            {
                                ErrorAssert("Unable to transform cats list into json: " + exception.Message);
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

                            break; 
                        }
                        else
                        {
                            foreach (Cat cat in listCats)
                            {
                                if (cat.Name == catName)
                                {
                                    //string json = JsonSerializer.Serialize(cat);
                                    string json = JsonConvert.SerializeObject(cat); 
                                    response.SetStatusLine(1.1f, 200);
                                    response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                                    response.SetHeader("Content-Language", "en-US, es-ES");
                                    response.SetHeader("Content-Type", HTTPHeader._mimeTable[".json"]);
                                    response.SetBody(json);
                                    response.SetCacheControl(60);
                                    
                                    return response.ToString(); 
                                }
                            }
                            
                            response = HTTPResponse.Get404DefaultNotFoundHeader();
                            if (method == "HEAD") response.SetBody(""); 
                            response.SetContentLength();
                            break;
                        }
                        
                    }

                    break; 
                }

                if (File.Exists(findPath))
                {
                    string fileExtension = Path.GetExtension(uri); 
                    TestAssert("File exists and have extension: " + Path.GetExtension(uri));

                    // If accept same mime as asked in the uri (If ask for index.html, Accept: must contain text/html or all data types "Accept:*/*"
                    if (headers["Accept"].IndexOf(HTTPHeader._mimeTable[Path.GetExtension(uri).Trim()]) > -1 || headers["Accept"] == "*/*")
                    {
                        response.SetStatusLine(1.1f, 200);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Content-Type", HTTPHeader._mimeTable[Path.GetExtension(uri).Trim()]);
                        response.SetCacheControl(60);


                        // Only insert method in GET, not in HEAD
                        if (method == "GET")
                        {
                            if (fileExtension == ".html" || fileExtension == ".txt" || fileExtension == ".json" || fileExtension == ".xml")
                            {
                                response.SetBody(File.ReadAllText(findPath));
                            }
                            else
                            {
                                byte[] file = File.ReadAllBytes(findPath);
                                string fileToString = Convert.ToBase64String(file);
                                response.SetBody(fileToString);
                            }
                        }
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 406);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        if(method == "GET") response.SetBody("{\"406error\": \"Incorrect Accept: header attribute for file extension\"}");
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
                //Handle User Login 
                if (uri.StartsWith("/login") || uri.StartsWith("login"))
                {
                    //var loginRequest = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                    
                    Dictionary<string, string>? loginParams = mUtils.ParsePostAttributes(body);

                    if (loginParams == null)
                    {
                        response = HTTPResponse.Get500InternalServerErrorHeader();
                        ErrorAssert("body is empty or not formatted as <param1>=<value1>&<param2>=<value2>");
                        break; 
                    }
                    
                    if (loginParams == null || !(loginParams.ContainsKey("username") && loginParams.ContainsKey("password")))
                    {
                        // Incorrect login parameters format
                        response = HTTPResponse.Get400DefaultBadRequestHeader(
                            "Incorrect user and password formatting, expected username=user&password=12345"); 
                    }
                
                    string username = loginParams["username"];
                    string password = loginParams["password"];
                    
                    if (_userManager.ValidateUser(username, password))
                    {
                        SessionToken session = _userManager.CreateSession(username);
                        response.SetStatusLine(httpVersion, 200);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Content-Type", HTTPHeader._mimeTable[".plain"]);
                        response.SetCacheControl(0);
                        string json = "{\"token\":\""+session.Token+"\"}";   
                        response.SetBody(json);
                        response.SetContentLength();
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 401);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Content-Type", HTTPHeader._mimeTable[".plain"]);
                        response.SetCacheControl(0);
                        response.SetBody("Invalid username or password");
                    }
                    
                    break; 
                }
                //Handle User Creation 
                else if (uri.Contains("/users/"))
                {
                    User? user = null; 
                    try
                    {
                        user = JsonConvert.DeserializeObject<User>(body); 
                    }
                    catch (JsonException jsonException)
                    {
                        ServerAssert("Exception handled> " + jsonException.Message);
                        response = HTTPResponse.Get400DefaultBadRequestHeader();
                        break; 
                    }
                    catch (Exception exception)
                    {
                        ServerAssert("Exception handled> " + exception.Message);
                        response = HTTPResponse.Get400DefaultBadRequestHeader();
                        break; 
                    }
                    
                    
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

                    break; 
                } //------------------------------------------^
                else if (uri.Contains("/cats/"))
                {
                    // Reads JSON from the body to add a cat
                    Cat? catPost = null;
                    try
                    {
                        catPost = JsonConvert.DeserializeObject<Cat>(body);
                    }
                    catch (Exception e)
                    {
                        ErrorAssert(" Unable to parse JSON: " + e.Message + " stack trace: " + e.StackTrace);
                        response = HTTPResponse.Get400DefaultBadRequestHeader("Unable to parse json in body, cat must contain: name, breed, gender, age, owner");
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
                    
                    PNG pngImage = new PNG("another_upload.png", imageData);

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
                else
                {
                    response = HTTPResponse.Get404DefaultNotFoundHeader(); 
                }
                
                break;
            
            case "PUT": // UPDATE IN DB
                //Handle User Update ------------------------------------------_
                if (uri.StartsWith("/users")) // uri.StartsWith == "/user/"
                {
                    string username = uri.Substring("/user/".Length);

                    User user = null;



                    try
                    {
                        user = JsonConvert.DeserializeObject<User>(body);
                    }
                    catch (Exception exception)
                    {
                        response = HTTPResponse.Get400DefaultBadRequestHeader("Unable to parse json in body");
                        break; 
                    }

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
                        //catPut = JsonSerializer.Deserialize<Cat>(body, _jsonOptions);c
                        catPut = JsonConvert.DeserializeObject<Cat>(body);
                    }
                    catch (Exception e)
                    {
                        ErrorAssert("Unable to parse in json: " + e.Message + " stack trace: " + e.StackTrace);
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
                
                if (uri.Contains("/users/")) 
                {
                    string username = uri.Substring("/users/".Length);
                    
                    if (_userManager.DeleteUser(username))
                    {
                        response.SetStatusLine(httpVersion, 200);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Content-Type", HTTPHeader._mimeTable[".plain"]);
                        response.SetCacheControl(0);
                        response.SetBody("User deleted successfully");
                    }
                    else
                    {
                        response.SetStatusLine(httpVersion, 404);
                        response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                        response.SetHeader("Content-Language", "en-US, es-ES");
                        response.SetHeader("Content-Type", HTTPHeader._mimeTable[".plain"]);
                        response.SetCacheControl(0);
                        response.SetBody("User not found");
                    }
                } //------------------------------------------^
                else if (uri.Contains("/cats/"))
                {
                    string catName = uri.Substring("/cats/".Length);
                    
                    response.SetBody("{\"error\":\"Cat non finded\"}");
                    
                    foreach (Cat c in listCats)
                    {
                        if (c.Name == catName)
                        {
                            Cat deletedCat = c; 
                            listCats.Remove(c);
                            
                            response.SetStatusLine(httpVersion, 200);
                            response.SetHeader("Server", "MyServer/1.0.0 (Windows)");
                            response.SetHeader("Content-Language", "en-US, es-ES");
                            response.SetHeader("Content-Type", HTTPHeader._mimeTable[".plain"]);
                            response.SetCacheControl(0);
                            response.SetBody("Deleted: ");
                            //response.AppendBody("\n" + JsonSerializer.Serialize(c));

                            string json = ""; 
                            try
                            {
                                json = JsonConvert.SerializeObject(c);
                            }
                            catch(Exception exception)
                            {
                                ServerAssert("Error> Unable to serialize json, exception handled: " + exception.Message);
                                json = ""; 
                            }
                            
                            response.AppendBody(json);
                            response.SetContentLength();
                            return response.ToString();
                        }
                    }

                    return HTTPResponse.Get404DefaultNotFoundHeader().ToString(); 
                }
                else
                {
                    response = HTTPResponse.Get404DefaultNotFoundHeader();
                    break; 
                }
                
                break;
        }
        
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
        // Store in server UI pool that will be processed in next frame
        ServerSend.instance.messagePool += "\n" + msg; 
    }

    /// <summary>
    /// Testing assertions for debugging and normal behaviour check
    /// </summary>
    /// <param name="msg">Message to assert</param>
    public static void TestAssert(string msg)
    {
        Debug.Log("Test> " + msg);
    }

    /// <summary>
    /// Error assertion while the execition of the program
    /// </summary>
    /// <param name="msg">Message to assert</param>
    public static void ErrorAssert(string msg)
    {
        Debug.Log("Error> " + msg);
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