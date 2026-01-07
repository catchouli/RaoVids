# RaoVids
Random youtube video aggregator and player for raocow. (wip, doesn't work yet. TODO: update readme :)

## How to run (dev mode)
A development compose.yaml is provided for starting a dev database with docker-compose.

```
# Start dev database.
docker-compose up -d

# Start application (alternatively use visual studio, etc).
dotnet run

```

## How to run (production)
Use the provided docker image (ghcr.io/catchouli/raovids) or helm chart (in the `helm` directory). If using docker directly, you'll need to specify the configuration either by binding a .env file at /app or using environment variables. See `.env` for a configuration example.

## Configuration values
The application will read configuration values from either the environment or a .env file, see the file `.env` for the default dev configuration.

### DatabaseConnString
A connection string for a postgres instance of the format "Host=hostname; Database=databasename; Username=username; Password=password".
