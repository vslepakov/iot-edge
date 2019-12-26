# Pub/Sub with RabbitMQ

[Related Blog Post](https://medium.com/@vslepakov/dapr-on-azure-iot-edge-31c7020c8cda)

This sample demonstrates how to use Dapr Pub/Sub with RabbitMQ on IoT Edge.
It based on the [original Pub/Sub sample with Redis](https://github.com/dapr/samples/tree/master/4.pub-sub)

## Run it

To run this sample, please exchange *YOUR VALUE* in *deployment.template.json* and all *module.json* files with the address of your container registry. Also create a .env file containing credentials for your container registry.

Then deploy to your edge device.

## Gotcha

Sequence order of module start up currently does play a role. Redis and RabbitMQ should be started first.
Currently IoT Edge does not offer control over this aspect and Dapr is currently not resilient enough to handle situations like this.
If modules are started in different order, just go ahead and restart *EventPublisher* and *EventConsumer* as soon as Redis and RabbitMQ are up and running.