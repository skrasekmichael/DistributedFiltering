# Distributed Filtering

A project for the [Distributed System Enviroment](https://www.fit.vut.cz/study/course/268340/.en) course (2023) at [BUT FIT](https://www.fit.vut.cz/.en) 
introducing distributed computing using [Microsoft Orleans](https://learn.microsoft.com/en-us/dotnet/orleans/overview) framework.

The application provides a means to distribute work - image filtering - across a cluster. A cluster consists of workers (clients) connected to a server. 
The server handles worker disconnection (via heartbeat every 2 seconds) and automatically redistributes unfinished work to other available workers. 
The client will be assigned work on connection, if available.

The server provides an API to:
- initiate a job (apply bilateral filter/add gausian noise) - divide an image into batches of work and distribute them to workers.
- retrive status/progress of the current job and currently working clients
- cancel current job

*Note: server runs swagger for API manipulation.*

## Running
*Premise: the application has been build* (`dotnet build DistributedFiltering.sln`).

Run server with the Orleans cluster:
```
dotnet DistributedFiltering.Server.dll --Cluster:Address=192.168.0.1
```
Server will initiate silo (cluster) at the address `192.168.0.1`, when unspecified, server will use address from the `appsettings.json` (by default `127.0.0.1`).
This will also run ASP.NET Core Web API with metioned endpoints.

Run client:
```
dotnet DistributedFiltering.Client.dll --Cluster:Address=192.168.0.1
```
Client tries to connect to the Orleans cluster at the address `192.168.0.1`, when unspecified, client will use address from the `appsettings.json` (by default `127.0.0.1`).
If the client cannot connect to the server or the server crashes, the client crashes.
