#!/bin/bash

python3 main.py &
sleep 3
daprd --dapr-id $1