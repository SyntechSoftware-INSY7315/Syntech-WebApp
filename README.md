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
3. Enter the GitHub repository URL: https://github.com/SyntechSoftware-INSY7315/Syntech-WebApp.git
4. Choose a local folder where you want to store the project
5. Click Clone
6. Once the repository has been cloned, Visual Studio may open the project using the default main branch. The main branch currently contains the initial project template, so you must switch to the develop branch to access the completed application
7. Open File Explorer and navigate to the folder where you cloned the repository
8. Right-click inside the project folder and select Open Git Bash Here
9. Check the current branch by entering: git branch
10. The expected output should be: * main
11. Create and switch to the develop branch by entering: git switch develop
12. If Git reports that the develop branch does not exist locally, create a local develop branch that tracks the remote branch using: git checkout -b develop origin/develop
13. Verify that you are now on the develop branch by entering: git branch
14. Expected output: * develop, main
15. The asterisk (*) indicates your current branch
16. Once you have successfully switched to develop, pull the latest code by entering the following command: git pull origin develop
17. When the pull has completed successfully, close Git Bash and reopen Visual Studio
18. On the Visual Studio start screen, select the recently cloned SyntechSoftwareWebApplication solution if it appears in the recent projects list
19. Alternatively, select Open a project or solution and navigate to the following folder inside your cloned repository: src/SyntechSoftwareWebApplication/
20. Select and open the SyntechSoftwareWebApplication.slnx solution file

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
2. In the Visual Studio toolbar, locate the launch profile dropdown next to the green Start button and select IIS Express
3. Select Debug > Start Without Debugging or press Ctrl + F5
4. The web application should open in your default web browser
5. You can also press F5 to run the application with the Visual Studio debugger

### Step 6 - Log in as an Admin
If you wish to log in as an admin, please follow these instructions:

1. Launch the web application in Visual Studio (step 5)
2. Click the Login option on the navigation bar
3. Enter the following information to log in as an admin:
   * Email: admin@syntech.local
   * Password: ChangeThisAdminPassword2026!
4. Click the Sign In button

### Step 7 - Database Access
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
