# MessageBusTestingApp

## Required applications

To run MessageBusTestingApp locally, you need to have these applications installed and setup correctly:
- Visual Studio 2022
- Windows Subsystem for Linux (WSL)
- Docker

### Visual Studio 2022 - New installation
1. Download Visual Studio 2022 from this [link](https://visualstudio.microsoft.com/downloads/)
   - Community edition is enough
2. Open downloaded file and let it download and install Visual Studio Installer
3. After the installation, a new window should open
4. In the `Workloads` tab mark `.NET desktop development`
5. Go to `Individual components` tab
6. Mark `.NET 6.0 Runtime (Long Term Support)` if it is unmarked
7. Click `Install`
8. Finish setup according to your preference

### Visual Studio 2022 - Existing installation
1. Open Visual Studio Installer
2. Click `Modify` for you currently installed Visual Studio 2022
3. A new window should open
4. In the `Workloads` tab mark `.NET desktop development` if it is unmarked
5. Go to `Individual components` tab
6. Mark `.NET 6.0 Runtime (Long Term Support)` if it is unmarked
7. Click `Modify` if you changed anything or `Close` if everything was set up correctly

### WSL - New installation
1. Follow the instructions on this [link](https://learn.microsoft.com/en-us/windows/wsl/install)

### Docker - New installation
1. Download Docker Desktop from this [link](https://www.docker.com/products/docker-desktop/)
2. Open downloaded file and follow installation wizard
   - On the tab `Configuration` mark `Use WSL 2 instead of Hyper-V (recommended)` if it is unmarked
   
## Project launch
1. Start Docker Desktop application (finish setup wizard if you are running application for the first time)
2. Open PowerShell and run this command `docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.11-management`
3. Open project in Visual Studio 2022
4. Contact author to gather connection string used to connect to Azure Service Bus
5. Replace `<ADD_CONNECTION_STRING>` with the gathered connection string in following files:
   - Receiver/appsettings.azure-service-bus.json
   - Receiver/appsettings.n-service-bus-azure-service-bus.json
   - Sender/appsettings.azure-service-bus.json
   - Sender/appsettings.n-service-bus-azure-service-bus.json
6. Right-click on the Solution
7. Select `Common Properties/Startup Project` in the menu
8. Mark `Multiple startup projects` if it is unmarked
9. Set `Start` action for Receiver and Sender project
10. Click `Apply` and close the window
11. Launch application via button with green play button

## NServiceBus - License
If you try to run NServiceBus and a new browser window opens with the question `Still in development?` follow these instructions:
   1. Fill in the form with your information
   2. In the part `I AM DOWNLOADING THIS LICENSE TO` select `Work on a project in development`
   3. Click `RENEW DEVELOPMENT LICENSE`
   4. Check email that you have used in the form
   5. Follow the instructions in the email from `Particular` to renew license
   6. Stop and launch again the project in Visual Studio 2022
