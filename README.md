# SWE3 ORMapper "SeppMapper" by Josef Koch

## Setup
It is highly recommended to use the docker-compose.yaml file to create the required PostgreSQL database for the TestApp as well as for the unit tests. If you choose to use the provided docker-compose.yaml you don't have to setup anything further. If you choose to run your own database, you have to specify your own Connection String in the constructor of the Context.

## Requirements
1. Dotnet Core 3.1
2. PostgreSQL database with user and password

## Documentation
The Doxygen documentation is available as an html file in SWE3.SeppMapper/html/index.html.

## Not Implemented Features
* Changetracking
* Locking
* Caching
* Automatic querying, creating, updating of related Entities
* Transactions