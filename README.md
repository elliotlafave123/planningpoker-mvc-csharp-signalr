# Planning Poker Application

An open-source Planning Poker tool for development teams to collaboratively estimate story effort. This application allows team members to assign story points to tasks in an interactive and engaging way, facilitating consensus and improving project planning accuracy.

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Clone the Repository](#clone-the-repository)
  - [Editing Connection Strings](#editing-connection-strings)
  - [Updating the Database](#updating-the-database)
  - [Running the Application Locally](#running-the-application-locally)
- [Deployment](#deployment)
  - [Deploying to IIS](#deploying-to-iis)
- [Contributing](#contributing)
- [License](#license)

---

## Features

- **Real-time Voting**: Team members can vote on story points in real-time using SignalR.
- **Round Management**: Hosts can start and end rounds, assign round names, and manage voting sessions.
- **Voting Anonymity**: Votes are hidden until all participants have voted or the round ends.
- **User Management**: Easily add team members to sessions with unique game links.

---

## Prerequisites

- **.NET 6 SDK**: Ensure you have .NET 6 SDK installed.
- **SQL Server**: For database storage (Express or full version).
- **Visual Studio 2022** or **Visual Studio Code**: For development and running the application.
- **IIS** (Internet Information Services): If deploying on a Windows server.

---

## Getting Started

### Clone the Repository

1. **Clone the Repository**:

   ```bash
   git clone https://github.com/yourusername/PlanningPoker.git
   ```

2. **Navigate to the Project Directory**:

   ```bash
   cd PlanningPoker
   ```

### Editing Connection Strings

The application uses `appsettings.json` and `appsettings.Development.json` files to store configuration settings, including database connection strings.

1. **Locate Configuration Files**:

   - **Production Environment**: `appsettings.json`
   - **Development Environment**: `appsettings.Development.json`

2. **Edit the Connection Strings**:

   Open the respective file and locate the `ConnectionStrings` section.

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=PlanningPoker;Trusted_Connection=True;MultipleActiveResultSets=true"
     },
     // Other settings...
   }
   ```

3. **Update the Connection String**:

   - Replace `YOUR_SERVER_NAME` with the name of your SQL Server instance.
   - If using SQL Server Authentication, include `User ID` and `Password` in the connection string.

   **Example using SQL Server Authentication**:

   ```json
   "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=PlanningPoker;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=true"
   ```

### Updating the Database

The project includes the necessary Entity Framework Core migrations. You just need to apply them to your database.

1. **Open a Command Prompt or Terminal**:

   Navigate to the project directory where the `.csproj` file is located (you should already be there).

2. **Restore Dependencies**:

   ```bash
   dotnet restore
   ```

3. **Update the Database**:

   Apply the existing migrations to create or update the database schema.

   ```bash
   dotnet ef database update
   ```

   This command will create the database (if it doesn't exist) and apply all migrations included in the project.

### Running the Application Locally

1. **Ensure Prerequisites Are Met**:

   - .NET 6 SDK is installed.
   - SQL Server is running and accessible.

2. **Run the Application**:

   ```bash
   dotnet run
   ```

3. **Access the Application**:

   Open your web browser and navigate to `https://localhost:5001` or `http://localhost:5000` (depending on your SSL configuration).

---

## Deployment

### Deploying to IIS

To deploy the Planning Poker application to IIS on a Windows server, follow these steps:

#### **1. Publish the Application**

Use the `dotnet publish` command to compile the application and prepare it for deployment.

```bash
dotnet publish --configuration Release --output "publish"
```

This will create a `publish` folder containing the compiled application files.

#### **2. Install .NET Core Hosting Bundle**

On the server where IIS is installed:

- Download the [.NET 6 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/6.0) (since the application targets .NET 6).
- Run the installer and follow the prompts.
- Restart the server after installation.

#### **3. Create an IIS Website**

- Open **IIS Manager** (`inetmgr`).
- Right-click on **Sites** and select **Add Website**.
- **Configure Site Settings**:
  - **Site name**: `PlanningPoker` (or your preferred name).
  - **Physical path**: Browse to the `publish` folder you created earlier.
  - **Binding**: Set the appropriate `Host name`, `IP address`, and `Port`.
- Click **OK** to create the site.

#### **4. Set Application Pool Identity**

- In IIS Manager, click on **Application Pools**.
- Find the application pool for your site (it may be created automatically).
- Set the **.NET CLR Version** to **No Managed Code**.
- Ensure the **Identity** is set to `ApplicationPoolIdentity`.

#### **5. Configure Permissions**

- Navigate to your application's physical path.
- Right-click the folder and select **Properties**.
- Go to the **Security** tab and click **Edit**, then **Add**.
- Enter `IIS APPPOOL\YourAppPoolName` and click **Check Names**.
  - For example, if your application pool is named `PlanningPoker`, enter `IIS APPPOOL\PlanningPoker`.
- Grant **Read** and **Execute** permissions.
- Click **OK** to apply the changes.

#### **6. Configure the `web.config` File**

Ensure the `web.config` file in your application's root directory is correctly configured.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified"/>
    </handlers>
    <aspNetCore processPath=".\PlanningPoker.exe" arguments="" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="InProcess">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production"/>
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

- Update `processPath` to point to your application's executable if necessary.

#### **7. Verify Firewall Settings**

Ensure that the port you're using is open in the Windows Firewall.

#### **8. Restart IIS**

In IIS Manager, select your server in the left-hand pane and click **Restart** in the **Actions** pane.

#### **9. Test the Deployment**

- Open a web browser on the server and navigate to your site's URL.
- If accessible internally, test externally if applicable.

#### **10. Troubleshooting**

- **Enable Logging**: Set `stdoutLogEnabled="true"` in `web.config` and check the logs if issues arise.
- **Check Event Viewer**: Look for any errors related to IIS or your application.

---

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

1. **Fork the Repository**

2. **Create a Feature Branch**

   ```bash
   git checkout -b feature/YourFeature
   ```

3. **Commit Your Changes**

   ```bash
   git commit -am "Add new feature"
   ```

4. **Push to the Branch**

   ```bash
   git push origin feature/YourFeature
   ```

5. **Open a Pull Request**

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

**Note**: This application is intended for educational and collaborative purposes. Ensure that you comply with your organization's policies and guidelines when deploying and using this tool.
