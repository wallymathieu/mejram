Make sure to initialize variables from the .env file:

```sh
source ../../.env
```

See the README.md in the root of the repository for more information.

Run the following to serialize the database tables:

```sh
dotnet run Serialize tables --connectionString "$LOCAL_MSSQL_CONN" --database sqlserver
dotnet run Serialize tablecount --connectionString "$LOCAL_MSSQL_CONN" --database sqlserver
dotnet run Serialize keyweights --connectionString "$LOCAL_MSSQL_CONN" --database sqlserver
```

Once that is done you can run the following to generate the dot file (assuming that you have [graphviz](https://graphviz.org/) installed and in your path):

```sh
dotnet run DotGraph WriteDot --dot dot
```

On windows you might need to specify the path to the dot executable:

```ps1
dotnet run DotGraph WriteDot --dot='c:\program files\graphviz\bin\dot.exe' 
```

To generate using [mermaid](https://mermaid.js.org/) make sure you have the [cli installed](https://github.com/mermaid-js/mermaid-cli)

```sh
dotnet run Mermaid Write --mmdc mmdc
```
