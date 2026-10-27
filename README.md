# RaoVids
Random youtube video aggregator and player for raocow.

<!-- Screenshot gallery -->
<a href="https://raw.githubusercontent.com/catchouli/RaoVids/main/screenshots/home.png">
  <img src="https://raw.githubusercontent.com/catchouli/RaoVids/main/screenshots/home_thumb.png">
</a>
<a href="https://raw.githubusercontent.com/catchouli/RaoVids/main/screenshots/viewer.png">
  <img src="https://raw.githubusercontent.com/catchouli/RaoVids/main/screenshots/viewer_thumb.png">
</a>

## How to run (dev mode)
A development compose.yaml is provided for starting a dev database with docker-compose.

```
# Start dev database.
docker-compose up -d

# Rename .env.example to .env.
# (Open it in your editor too and make sure the values look sensible. The default
#  DatabaseConnString points to the dev db in compose.yaml.)
cp .env.example .env

# Start application (alternatively use visual studio, etc).
dotnet run

```

A youtube data API key is necessary for video scanning to work.

## How to run (production)
Use the provided docker image (ghcr.io/catchouli/raovids) or helm chart (in the `helm` directory). If using docker directly, you'll need to specify the configuration either by binding a .env file at /app or using environment variables. See `.env.example` for a configuration example.

The helm chart reads both configuration values from an existing Secret, which you create yourself:

```
kubectl create secret generic raovids \
  --from-literal=DatabaseConnString='Host=...; Database=...; Username=...; Password=...' \
  --from-literal=YoutubeDataApiKey='...'

helm upgrade --install raovids ./helm --set existingSecret=raovids \
  --set ingress.enabled=true --set ingress.host=raovids.example.com
```

See `helm/values.yaml` for the other settings (ingress class, annotations, TLS secret, resources). The app applies its database migrations at startup, so its database user needs to own its tables.

## Configuration values
The application will read configuration values from either the environment or a .env file, see the file `.env.example` for the default dev configuration.

### DatabaseConnString
A connection string for a postgres instance of the format "Host=hostname; Database=databasename; Username=username; Password=password".
