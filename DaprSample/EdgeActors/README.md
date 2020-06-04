# Dapr Hello World sample

[Related Blog Post](https://medium.com/@vslepakov/dapr-actors-on-azure-iot-edge-65782d0fcf23)

This sample demonstrates a way to use [Dapr Actors](https://github.com/dapr/docs/blob/v0.6.0/concepts/actors/README.md) on IoT Edge.  
It is based on [this sample](https://github.com/dapr/dotnet-sdk/blob/master/docs/get-started-dapr-actor.md).

## Run it

Use Azure Portal or az cli to deploy the solution using the pre-built deployment template, e.g.:

`az iot edge deployment create -d <DEPLOYMENT NAME> -n <IOT HUB NAME> --content config/deployment.amd64.json --target-condition "deviceId='<YOUR DEVICE ID>'" --priority 10`