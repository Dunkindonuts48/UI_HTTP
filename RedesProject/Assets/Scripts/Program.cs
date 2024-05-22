﻿namespace HTTP_NET_Project;


class Program
{
    static void Main(string[] args)
    {
        HTTPServer server = new HTTPServer();
        HTTPClient client = new HTTPClient(); 
        
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

        HTTPRequest catPost = new HTTPRequest(); 
        catPost.SetRequestLine("POST", "/png", 1.1f);
        catPost.SetHeader("Accept", "text/html, application/json, text/xml, image/jpeg");
        catPost.SetHeader("Accept-Language", "en-US, es-ES"); 
        catPost.SetDateHeader();
        catPost.SetConnectionHeader("keep-alive");
        catPost.SetHeader("Authorization", "Bearer admin_token");
        //catPost.SetBody("{\"name\": \"Mittens\",\"breed\": \"Siamese\",\"age\": 3, \"owner\": \"ALFREDO PEREZ FANTOVA\"}");
        catPost.SetBody("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");
        catPost.SetContentLength();
        
        client.SendHttpRequest(catPost);
        //client.Disconnect();
        
        Console.ReadKey(); 
    }
}