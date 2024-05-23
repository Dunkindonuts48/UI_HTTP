using HTTP_NET_Project;
using UnityEngine;


class Program : MonoBehaviour
{
    private static HTTPServer server = null;

    private static HTTPClient client = null; 
    //static void Main(string[] args)
    private void Awake()
    {
        server = new HTTPServer();
        client = new HTTPClient(); 
        
        // var test = mUtils.ParsePostAttributes("username=admin&password=admin_pass");
        // string pasword = test["password"]; 
        
        // Server starts listing at a socket
        server.InitializeThread();
        
        // Client initialize and search socket and send request 
        client.Initialize();
        client.ConnectSocket();
        
        // TODO Replace this HTTP request with the one filled in Unity
        
        // HTTPRequest httpRequestTest = new HTTPRequest(); 
        // //httpRequestTest.SetRequestLine("GET", "/fol/example.xml", 1.1f);
        // httpRequestTest.SetRequestLine("GET", "/index.html", 1.1f);
        // httpRequestTest.SetHeader("Accept", "text/html, application/json, text/xml");
        // httpRequestTest.SetHeader("Accept-Language", "en-US, es-ES"); 
        // httpRequestTest.SetDateHeader();
        // httpRequestTest.SetHeader("Authorization", "Bearer admin_token");
        // //httpRequestTest.SetBody("{\"Test\":\"This is a json send as body in order to test\"}");
        // httpRequestTest.SetBody("{\"user\":\"admin\", \"password\":\"admin\"}");
        // httpRequestTest.SetContentLength();

        // HTTPRequest catPost = new HTTPRequest(); 
        // catPost.SetRequestLine("POST", "/login", 1.1f);
        // catPost.SetHeader("Accept", "text/html, application/json, text/xml, image/jpeg");
        // catPost.SetHeader("Accept-Language", "en-US, es-ES"); 
        // catPost.SetDateHeader();
        // catPost.SetConnectionHeader("keep-alive");
        // catPost.SetHeader("Authorization", "Bearer admin_token");
        // catPost.SetBody("username=admin&password=admisn");
        // catPost.SetContentLength();
        //
        // client.SendHttpRequest(catPost);


        // HTTPRequest getIndex = new HTTPRequest(); 
        // getIndex.SetRequestLine("DELETE", "/users/admin", 1.1f);
        // getIndex.SetHeader("Accept", "text/html, application/json, text/xml, image/jpeg");
        // getIndex.SetHeader("Accept-Language", "en-US, es-ES"); 
        // getIndex.SetDateHeader();
        // getIndex.SetConnectionHeader("keep-alive");
        // getIndex.SetHeader("Authorization", "Bearer admin_token");
        // //getIndex.SetBody("username=admin&password=admin");
        // //getIndex.SetBody("{\"Urname\":\"Admin2\", \"Password\":\"hola\", \"Salt\":\"akjshdm\"}");
        // getIndex.SetBody("{\"Username\":\"Dani\",\"Password\":\"Danipass\",\"Salt\":\"asd\"}");
        // getIndex.SetBody("this is a test");
        // getIndex.SetContentLength();
        //
        // client.SendHttpRequest(getIndex);
        //client.Disconnect();
        
        //Console.ReadKey(); 
    }

    public static void SendHTTPRequest(HTTPRequest request)
    {
        client.SendHttpRequest(request); 
    }
}