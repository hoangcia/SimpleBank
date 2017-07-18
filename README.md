# SimpleBank
### Descriptions:

This is a simple application written by AspDotNet Core and Entity Framework Core (dotnet core v1.1) with Visual Studio 2017,
to demonstrate a simple bank system. The application also handles the concurrency problem when editing an account information.

Features:

* Sign in
* Sign up
* Withdraw
* Deposit
* Transfer to another account

The project is structured as 2 folders: **src** and **test**.
The **src** folder contains all source code implementing bank business.
The **test** folder contains all unit test written by using XUnit

### How to run:

1. Clone this project files to your local machine.
2. Open Command Propmpt window, go to project folder then type `dotnet ef database update` to apply database migrations
3. Open solution file **SimpleBank.sln** in Visual Studio 2017
4. Press **Ctrl + Shift + B** to build the solution
5. Press **Ctrl + F5** to run the application
6. To run Unit Tests, go to **Test** -> **Window** -> **Test Explorer**, then choose which test you want to run

Use some default users with password **1234abc** to login:
0011@pc.com 
1122@pc.com 
2233@pc.com 
3344@pc.com 

## HAPPY CODING!
