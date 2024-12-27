# Changes in this fork

- Added dark mode
- Streamlined broker service
- Upgraded to MQTTNet 4.3.7
- Added IIS compatibility
- Removed port configuration as that is managed by IIS
- Added file logging (https://github.com/adams85/filelogger)
- Added further telemetry to dashboard (application lifetime, disconnections, subscriptions, etc)
- Added QoS option to publish panel
- Fixed memory leak, max records can be configured on appsettings.json
- Added example REST endpoint for telemetry data export
  
# Mqtt Broker w/ Dashboard

A simple Server-side Application hosting a Mqtt Broker and Dashboard UI for real-time monitoring using [ASP.NET Blazor Server](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) to quickly build and test custom Mqtt infrastructure.

Access Dashboard UI in browser: http://localhost:5000, and use protocol "mqtt://" to access mqtt server (ex: mqtt://localhost:1883), a mqtt client such as [MQTTX](https://github.com/emqx/MQTTX) is recommended for debugging.

![Dashboard](https://github.com/user-attachments/assets/e2ca48b3-cbeb-44b1-9759-a8440074ec1a)

![Information](https://github.com/user-attachments/assets/0b767d61-c682-41c8-9066-103ce56b8621)

## Run in CLI

1. Install [Microsoft .NET SDK 6.0](https://dotnet.microsoft.com/download)

2. Clone Project from GitHub

3. Start Host from CLI (in Project Root Folder)

    `$ dotnet run`

4. Access Dashboard UI in Browser: http://localhost:5000

## Run in Docker Container

1. Install [Docker Desktop](https://docs.docker.com/desktop)

2. Clone Project from GitHub

3. Run as Docker Service (in Project Root Folder):

   `$ docker-compose up -d`

4. Access Dashboard UI in Browser: http://localhost:5000 

## Host in IIS
Configure application pool normally as any .NET app.

The MQTT server will be acessible on the IP binding/hostname you configure on IIS with the MQTT port you configured on appsettings.json (ex: mqtt://localhost:1883).

## Configuration 

~~Port configuration is stored in "HostConfig.json" and loaded at startup.~~

~~- Tcp Port: 1883 (regular _Mqtt over Tcp_)~~

~~- Http Port: 5000~~

All configuration is available on appsettings.json

 ## Endpoints
- "/" Serves Dashboard UI
  
- "/GetConnectedClients" Example telemetry endpoint
  
- "/swagger"
  
## Dependencies

- [MQTTnet](https://github.com/chkr1011/MQTTnet) Mqtt Library that supports Mqtt over WebSockets
- [MudBlazor](https://mudblazor.com) Material Design UI Framework for Dashboard Web Frontend
- [Json.NET](https://www.newtonsoft.com/json) Json Library to load/save Config File
- [Karambolo.Extensions.Logging.File](https://github.com/adams85/filelogger)  Lightweight implementation of the Microsoft.Extensions.Logging.ILoggerProvider interface for file logging
