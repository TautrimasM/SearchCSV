# Introduction 
It is an application that:
* Parses CSV files
* Searches for provided query
* Prints how many results and duplicates were found
* Optionally prints them and/or writes to JSON. 
* There is optional setting to implement severity threshold - if there are records with severity property which matches or ecxeeds provided threshold, it prints out them. 

# Requirements
.NET 8

Docker (for DataBase test functionality)

# How to use
## Ways of use
There are two ways of using this application - CLI or Wizard mode:
* CLI - you can provide data, query and other params as arguments
* Wizard mode - interactive mode which asks you for each parameter

## Spinning up test DataBase
To spin up a database instance you need to run dosker daemon and then run command
	```
	docker-compose up
	```

## Query syntax:
Query is made like this:

<name_of_param>='<value_of_param>
 
Example: 
```
signatureId='4608'
```	
Query supports wildcard operator like this:

Example:  
```
signatureId='*4608*'
```	
Also query supports boolean operators AND, OR, NOT:

Example: 
```
signatureId='*4608*' AND NOT deviceProduct='Windows Vista'
```

## CLI Arguments:
-f or --file: Input CSV file paths (comma-separated and individualy enclosed in quoutes, may be absolute or relative). Required.

-q or --query: Query to search logs (supports AND, OR, NOT). Required.

-o or --output: Output JSON file path.

-s or --severity: Enables printing out records that matches or exceeds povided severity threshold.

-p or --print: Print results to the console.

-w or --wizard: Enable wizard mode for interactive input.

-d or --delete-dublicates: Removes duplicate entries.

--save-db: tries to save search result to local NoSQL database.

--help: Show help message.

Example CLI:
```
dotnet run -- -f "logs.csv" -o "results.json" -q "signatureId='*4608*' AND NOT deviceProduct='Windows Vista'" -s 4 -d 
```
Example Wizard mode:
```
dotnet run -- -w
```

# Implemented TODOs:
## Required:
* Build a solution that is able to search any column by full or partial text string. 
* Results from the query should be combined and returned in JSON format.

## Extra:
* Add Boolean logical operator support in the queries.
* Add multiple file support.
* Add log count value in the resulting JSON output.
* Implement options to send alerts based on the severity of the log. No need to actually send the notification, you can use Console.WriteLine(). 
* Deal with duplicates.
* Write to DataBase(MongoDB due to possibly unknown structure of each CSV file)
* My own idea - implemented CLI for ease of automation tasks.

# Extra thoughts:
This is a very abstract project, and functions may need to be added or removed according to needs, i.e. if database is really needed, one should make more secure way to handle connection string. 
I was focusing on making it more like a standalone tool, so i have implemented database functionality just as an example
