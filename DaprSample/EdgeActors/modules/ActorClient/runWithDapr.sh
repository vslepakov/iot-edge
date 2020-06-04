#!/bin/bash

dotnet ActorClient.dll &
sleep 3
daprd --app-id actorclient --placement-address Placement:50005 --log-level debug


