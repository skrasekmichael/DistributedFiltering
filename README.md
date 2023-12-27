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
