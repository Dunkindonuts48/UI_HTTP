## HTTP SERVER



### REMAINING TASKS

* [X] Open sockets in both sides and be able to communicate
* [ ] Test communication between two different **devices**
* [ ] Parse requests in Server
  * [ ] GET
    * [ ] If requested file exists, return it
    * [ ] Handle exceptions by return code errors (404, 405 etc)
  * [ ] POST
    * [ ] Put the attributes send in header body into the webpage and return it
    * [ ] Be able to create a new resource
  * [ ] HEAD
    * [ ] Return reply similar as GET without returning the body.  
  * [ ] PUT
    * [ ] 
  * [ ] DELETE
    * [ ] Delete a specified resource in the server
   
* [ ] Parse responses in Client
* [ ] Once a connection is received in server, create new thread that would keep waiting to a new connection
* [ ] Interactive


### BASIC IMPLEMENTATION TASK LIST

🚢 HTTP Client

The program that interacts as an HTTP client must be able to execute the following features:

Send HTTP requests, in a way that:
* [X] It is possible to choose the URL to which the request will be sent--
* [ ] Use any available HTTP verb in the request (GET, HEAD, POST, PUT, DELETE)
* [ ] Automatically add the necessary headers to the request so that it can be processed correctly
Add any other arbitrary header desired by the user
Specify the body of the request
Receive and display on screen the response message of the sent request
Inform about the request status
Be able to send successive requests, i.e., to send a second request it is not necessary to restart the program



🏗️ HTTP Server

The HTTP server must be able to do the following:

Support, at least, the following endpoints, when they are correctly called (correct verb, correct headers...):
An endpoint that returns static content (e.g., a static HTML file)
An endpoint that adds a new resource to the server according to the specified payload
An endpoint that allows viewing a list of resources
An endpoint that allows modifying a resource
An endpoint that allows deleting a resource
Return the appropriate error codes if the endpoints are not invoked correctly
Attend to multiple requests concurrently
Offer minimal configuration that allows choosing on which port the server starts
It is not necessary for the resources to be persisted; they can be managed in memory