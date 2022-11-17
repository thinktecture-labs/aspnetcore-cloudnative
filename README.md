# ASP.NET Core Cloud-Native example

## Docker Compose

### URLs and demo credentials

* Gateway
  * Root: [http://localhost:5000](http://localhost:5000)
  * Swagger (Products Service): [http://localhost:5000/products/swagger/](http://localhost:5000/products/swagger/)
* Grafana
  * [http://localhost:3000](http://localhost:3000)
* Zipkin
  * [http://localhost:9411](http://localhost:9411)

### Necessary installation for Docker Compose-based setup

```bash
# Install Docker Plugin for Loki
docker plugin install grafana/loki-docker-driver:latest --alias loki --grant-all-permissions
```

### Local Environment

Find the `Makefile` in the root of the repository. Use it to perform common tasks as shown below:

```bash
# Start the sample locally (in docker)
make start 

# Quickstart (no image build) the sample locally (in docker)
make quickstart

# get logs
make logs

# stop the sample
make stop

# clean-up the local docker environment
## stops everything
## removes images
## removes volumes
## removes orphan containers
## removes custom docker network
make cleanup
```

### Common Docker-Compose commands

```bash
# Build Container images
docker-compose build

# Cleanup previously started instances
docker-compose rm -f

# Start cloud-native sample application (detached)
docker-compose up -d
# Start cloud-native sample application (blocking)
docker-compose up

# To stream logs to the terminal use
docker-compose logs
```

### Cleanup environment

```bash
# remove running containers
docker-compose rm -f

# remove custom Docker network
docker network rm cloud-native -f

# uninstall Loki Plugin
docker plugin rm loki -f
```
