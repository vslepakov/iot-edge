#!/bin/bash

node app.js &
sleep 3
daprd --app-id $1 --app-port $2 --dapr-http-port 3501 --dapr-grpc-port 50002 -components-path "components"
