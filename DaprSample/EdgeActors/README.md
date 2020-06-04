# Dapr Hello World sample

[Related Blog Post](TODO)

This sample demonstrates a way to use [Dapr Actors](https://github.com/dapr/docs/blob/v0.6.0/concepts/actors/README.md) on IoT Edge.

## Run it

Use Azure Portal or az cli to deploy the solution, e.g.:

`az iot edge deployment create -d <DEPLOYMENT NAME> -n <IOT HUB NAME> --content config/deployment.amd64.json --target-condition "deviceId='<YOUR DEVICE ID>'" --priority 10`