#!/bin/bash

dotnet SensorActor.dll &
sleep 3
daprd --app-id actorservice --app-port 3000 --placement-address Placement:50005 --log-level debug
