#!/bin/bash

python3 main.py &
sleep 3
daprd --app-id $1 --dapr-http-port 3500 --dapr-grpc-port 50001