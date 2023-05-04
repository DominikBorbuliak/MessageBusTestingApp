# MessageBusTestingApp

This application was created as a project that is part of bachelor thesis and was used to analyze different message bus solutions for the [Xolution](https://www.xolution.sk/).

## System requirements
- One of the following windows versions:
   - Windows 11 64-bit (Home, Pro, Enterprise or Education) 21H2 or higher
   - Windows 10 64-bit (Home or Pro) 21H1 (build 19043) or higher
   - Windows 10 64-bit (Enterprise or Education) 20H2 (build 19042) or higher
- RAM: 5 GB
- Display resolution: 1024 by 768 or higher

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
4. Contact author (514127@mail.muni.cz) to gather connection string used to connect to Azure Service Bus
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

## Message bus solutions
Testing application allows you to analyze these message bus solutions:
1. [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
   - Native message bus hosted on Azure Portal
2. [RabbitMQ](https://www.rabbitmq.com/)
   - Native message bus hosted locally on docker container 
3. [NServiceBus with Azure Service Bus transport](https://docs.particular.net/transports/azure-service-bus/)
   - Message bus abstraction from Particular Software
   - Physical message bus is hosted on Azure Portal
4. [NServiceBus with RabbitMQ transport](https://docs.particular.net/transports/rabbitmq/)
   - Message bus abstraction from Particular Software 
   - Physical message bus is hosted locally on docker container

## NServiceBus - License
If you try to run NServiceBus and a new browser window opens with the question `Still in development?` follow these instructions:
   1. Fill in the form with your information
   2. In the part `I AM DOWNLOADING THIS LICENSE TO` select `Work on a project in development`
   3. Click `RENEW DEVELOPMENT LICENSE`
   4. Check email that you have used in the form
   5. Follow the instructions in the email from `Particular` to renew license
   6. Stop and launch the project again in Visual Studio 2022

## Solution structure
Solution consists of 5 projects:
1. Receiver
   - Console application that simulates application which is responsible for processing messages
   - Contains configuration files for all Receiver types
2. Sender
   - Console application that simulates application which is responsible for sending messages
   - Contains configuration files for all Sender types
3. Services
   - Class library that stores whole logic for each Sender and Receiver type
4. Utils
   - Class library that stores utility classes
   - Contains utility class for console input and output operations
   - Contains utility class for operations with enumerations
5. Utils.Tests
   - Contains tests for utility classes
   
## NuGet packages used
Each message bus solution offers NuGet packages that provide various clients and interfaces used to communicate with message queues, and options for these clients and their methods. These clients, interfaces and options are used in the project Services within the folder Services.

### Usage of clients and interfaces
1. Azure Service Bus
   - `ServiceBusClient` is used to create additional clients provided via NuGet. Find more on this [link](https://learn.microsoft.com/en-us/javascript/api/@azure/service-bus/servicebusclient?view=azure-node-latest)
   - `ServiceBusProcessor` is used to process incoming messages that do not require response. Find more on this [link](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebusprocessor?view=azure-dotnet)
   - `ServiceBusSessionProcessor` is used to process incoming message that require response and to process response for sent messages that do not wait for such response. Find more on this [link](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebussessionprocessor?view=azure-dotnet)
   - `ServiceBusSessionReceiver` is used to wait for response to sent message. Find more on this [link](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebussessionreceiver?view=azure-dotnet)
2. RabbitMQ
   - `ConnectionFactory` is used to create `IConnection` and setup options for the communication with message queues
   - `IConnection` is used to create `IModel`
   - `IModel` represents channel, and it is used to declare message queues, set up options for the `AsyncEventingBasicConsumer` and send messages to queue
   - `AsyncEventingBasicConsumer` is used to process incoming messages
   - Find more information about these models and interfaces on this [link](https://www.rabbitmq.com/dotnet-api-guide.html#major-api-elements)
3. NServiceBus
   - `EndpointConfiguration` is used to setup current endpoint/application, specify transport, set up recoverability options, create queues and create `IEndpointInstance`
   - `TransportExtensions<T>` is used to setup transport and to connect with message bus
   - `IEndpointInstance` is used to stop processing of messages and to send messages to queue
   - `IHandleMessages<T>` is used to define message handler
   - Find more information about these models and interfaces on this [link](https://docs.particular.net/transports/azure-service-bus/configuration)
   
### Configuration of clients and their methods
You can test various options by modifying them directly in the code and then re-running the application. To learn more about these options, visit the following links:
1. Azure Service Bus
   - [ServiceBusClientOptions](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebusclientoptions?view=azure-dotnet)
   - [ServiceBusSenderOptions](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebussenderoptions?view=azure-dotnet)
   - [ServiceBusProcessorOptions](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebusprocessoroptions?view=azure-dotnet)
   - [ServiceBusSessionProcessorOptions](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebussessionprocessoroptions?view=azure-dotnet)
   - [ServiceBusReceiverOptions](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebusreceiveroptions?view=azure-dotnet)
2. RabbitMQ
   - [.NET/C# Client API Guide](https://www.rabbitmq.com/dotnet-api-guide.html#major-api-elements)
3. NServiceBus with Azure Service Bus transport
   - [Configuration](https://docs.particular.net/transports/azure-service-bus/configuration)
4. NServiceBus with RabbitMQ transport
   - [Connection settings](https://docs.particular.net/transports/rabbitmq/connection-settings)

## Application walkthrough

1. 2 console windows will open with the welcome screen after the application launch
   - One console window represents Receiver and the other one represents Sender
   - You can distinguish them by the text in the tab
   - You need to press anything to continue in the application
   - ![image](https://user-images.githubusercontent.com/46026094/235990885-5dcb9bf4-efbe-4fbe-bfd6-b83249e2267a.png)
2. A menu with message bus solutions will appear after the welcome screen
   - You need to pick a specific message bus solution that you want to analyze in both windows
   - Nothing bad happens when you select different solution in each window, but for the best analysis it is necessary to select the same solution in both windows
   - ![image](https://user-images.githubusercontent.com/46026094/235993156-3e024c32-d52f-41db-9208-442816888e39.png)
3. Receiver will only print information about current activities after the solution selection
   - You can press `ESC` to exit application anytime
   - You can press `C` to clear console output anytime
4. Sender can be used to launch predefined functions that simulate certain situations
   - These functions are described below
   - ![image](https://user-images.githubusercontent.com/46026094/235998572-2a848d3d-c283-4745-8c3d-31d1ae5ddb6d.png)

### << Send Only - 1 Custom Message - Simple >>
- Simulates sending of the plain text
- You need to provide text message
- Message will be sent without serialization

### << Send Only - 1 Custom Message - Advanced >>
- Simulates sending of the complex object
- You need to provide information about the person and their address (no meaningful texts are required)
- Message will be sent with serialization

### << Send Only - N Random Messages - Simple >>
- Simulates sending of multiple plain texts at the same time
- You need to provide number of messages that you want to send
- The texts will be randomly generated
- Messages will be sent without serialization

### << Send Only - N Random Messages - Advanced >>
- Simulates sending of multiple complex objects at the same time
- You need to provide number of messages that you want to send
- Complex objects will be randomly generated
- Message will be sent with serialization

### << Send Only - Simulate Exception Thrown in Receiver >>
- Simulates the exception thrown in receiver for messages that do not need to get response
- You need to provide the attempt on which the exception won't be thrown and the text of the exception
- You can write number 0 or less to simulate a message that will always throw exception

### << Send & Reply - Wait - Surface area and Volume of Rectangular Prism >>
- Simulates sending of a message that needs to get a response, while Sender is waiting synchronously for the response
- You need to provide dimensions of rectangular prism
- Volume and surface area will be calculated in the Receiver and send back to Sender

### << Send & Reply - Wait - Simulate N Clients >>
- Simulates sending a message that needs to get response from multiple clients at the same time, while Sender is waiting synchronously for the response
- You need to provide number of the clients, name for each client (to distinguish them in receiver) and the time in milliseconds which receiver will spend with processing
- Receiver will asynchronously sleep for specified time when receive message and will send a response when it finishes processing

### << Send & Reply - Wait - Simulate Exception Thrown in Receiver >>
- Simulates the exception thrown in receiver for messages that needs to get a response, while Sender is waiting synchronously for the response
- You need to provide dimensions of rectangular prism, attempt on which exception won't be thrown and the text of the exception
- You can write number 0 or less to simulate a message that will always throw exception
- Volume and surface area will be calculated in the Receiver and send back to Sender when exception won't be thrown

### << Send & Reply - No Wait - Surface area and Volume of Rectangular Prism >>
- Simulates sending of a message that needs to get a response, while Sender is not waiting for the response
- You need to provide dimensions of rectangular prism
- Volume and surface area will be calculated in the Receiver and send back to Sender
- Sender has attached handler that handles gathered response and prints it to the console

### << Send & Reply - No Wait - Simulate N Clients >>
- Simulates sending a message that needs to get response from multiple clients at the same time, while Sender is not waiting for the response
- You need to provide number of the clients, name for each client (to distinguish them in receiver) and the time in milliseconds which receiver will spend with processing
- Receiver will asynchronously sleep for specified time when receive message and will send a response when it finishes processing
- Sender has attached handler that handles gathered response and prints it to the console

### << Send & Reply - No Wait - Simulate Exception Thrown in Receiver >>
- Simulates the exception thrown in receiver for messages that needs to get response, while Sender is not waiting for the response
- You need to provide dimensions of rectangular prism, attempt on which exception won't be thrown and the text of the exception
- You can write number 0 or less to simulate a message that will always throw exception
- Volume and surface area will be calculated in the Receiver and send back to Sender when exception won't be thrown
- Sender has attached handler that handles gathered response and prints it to the console
