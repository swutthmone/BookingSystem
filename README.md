# API and Scheduler for BookingSystem

### Introduction

There are two parts in this project

1. API

        Mobile application access this API project. Multiple instances of API project can be running behind a load balancer. 


2. Scheduler

        Scheduler are background processes and can run multiple instances depends on the work load.


### Language and tools
```
.Net 6
MySQL version 8 
Pomelo.EntityFrameworkCore.MySql
Log4Net
```

### Init
```
Visual Studio or Visual Studio Code with vscode-solution-extension.
This project requires .net 6 sdk

```

### How to start
```
git clone https://github.com/swutthmone/BookingSystem.git
In VSCode --> Open Folder and choose gths-api
Initialize new database with script BookingSystem/References/DatabaseStructure/bookingsystem.sql 
Update database connection string in appsettings.json
```
1. For API project
        
        Right click BookingSystem in Solution explorer Restore/Build/Run/Publish
        OR can use CLI
        cd BookingSystem
        dotnet restore
        dotnet run

2. For Scheduler

        Right click Scheduler in Solution explorer Restore/Build/Run/Publish
        OR can use CLI
        cd SchedulerTask
        dotnet restore
        dotnet run

3. For Swagger UI
        http://localhostport/swagger/index.html (need to insert "api" for require api)
        
