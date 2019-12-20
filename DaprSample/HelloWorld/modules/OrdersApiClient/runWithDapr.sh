#!/bin/bash

python3 main.py &
sleep 3
daprd --dapr-id $1 --dapr-http-port 3500 --dapr-grpc-port 50001