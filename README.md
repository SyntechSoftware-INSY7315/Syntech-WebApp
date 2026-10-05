# Syntech-WebApp
ASP.NET Core MVC Web Application for Syntech Software

## Running the Syntech Software Web Application

### Prerequisites
Before running the application, make sure you have:
1. Visual Studio
2. .NET 10 SDK
3. Git & Git Bash
4. SQL Server Management Studio 22

### Step 1 - Clone the repository using Visual Studio
1. Open Visual Studio
2. On the Visual Studio start screen, select Clone a repository
3. Enter the GitHub repository URL: https://github.com/EMGPPT/insy7315-2026-poe-task-2-group-27.git
4. Choose a local folder where you want to store the project
5. Click Clone
6. Once the repository has been cloned, Visual Studio should open the project/solution
7. The solution file is located in: src/SyntechSoftwareWebApplication/
8. If the solution does not open automatically, open the .slnx file in this folder

### Step 2 - Configure User Secrets
The application uses ASP.NET Core User Secrets to store sensitive configuration values. These values are not stored in GitHub and must be configured locally. The required secrets include the database connection string, admin seed credentials, and OpenAI API key used by the AI Assistant

1. In Solution Explorer, right-click the Syntech.WebApp project
2. Select Manage User Secrets
3. Add the required configuration values to the secrets.json file:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db62847.public.databaseasp.net;Database=db62847;User Id=db62847;Password=9Km+@2TyPd7_;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  },

  "OpenAI": {
    "ApiKey": "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"
  },

  "AdminSeed": {
    "Name": "Syntech Administrator",
    "Email": "admin@syntech.local",
    "Password": "ChangeThisAdminPassword2026!"
  }
}

4. Save the secrets.json file

### Step 3 - Restore Dependencies
Visual Studio should automatically restore the required NuGet packages. If necessary, do the following:

1. Right-click the solution in Solution Explorer
2. Select Restore NuGet Packages

### Step 4 - Build the Application
1. In Visual Studio, select Build > Build Solution
2. Make sure the solution builds successfully before running the application

### Step 5 - Run the Web Application
1. Make sure Syntech.WebApp is selected as the startup project
2. Select Debug > Start Without Debugging
3. The web application should open in your default web browser
4. You can also press F5 to run the application with the Visual Studio debugger

### Step 6 - Database Access
The application uses the SQL Server database specified in the User Secrets connection string. To open the database, do the following:

1. Open SQL Server Management Studio 22
2. Enter the following information in connection properties:
  * Server Name: db62847.public.databaseasp.net
  * Authentication: SQL Server Authentication
  * User Name: db62847
  * Password: 9Km+@2TyPd7_
  * Encrypt: Mandatory
  * Trust Server Certificate: Yes
3. Click Connect
