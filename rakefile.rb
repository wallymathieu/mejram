import './script/rakefile.rb'

# mssql -d sakila -u sa -p Pass@word -s localhost -o 5433

desc "mssql test (make sure that docker-compose is up)"
task :mssql_test do
  cd 'csharp/Mejram.Console' do
    connectionstring = '--connectionstring "Server=localhost,5433;Database=sakila;User Id=sa;Password=Pass@word"'
    sh "dotnet run serialize tables #{connectionstring} --database=sqlserver --tablespath=./bin/outfile.Tables.json"
  end
end

desc "pg test (make sure that docker-compose is up)"
task :pg_test do
  cd 'csharp/Mejram.Console' do
    connectionstring = '--connectionstring "Host=localhost;Port=5432;Database=sakila;User Id=postgres"'
    sh "dotnet run serialize tables #{connectionstring} --database=sqlserver --tablespath=./bin/outfile.Tables.json"
  end
end
