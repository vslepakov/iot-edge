# This sample shows how you can create a VSCode [devcontainer](https://code.visualstudio.com/docs/remote/containers) 

Blog post https://medium.com/@vslepakov/vs-code-devcontainer-for-azure-iot-edge-1c8aed0814f3

This devcontainer allows you to develop and test your IoT Edge solutions inside a container. There is no need to install IoT Edge on your machine or to use a VM for testing. Everything happens inside one container. You can also run multiple IoT Edge dev and test environements in different devcontainers. This solution does not use a simulator it is actual IoT Edge inside the devcontainer.

- Just add the whole .devcontainer folder to your Edge Solution.
- Inside .devcontainer directory create a file *.env* for example like this in Windows `type nul > .env`
- Inside the *.env* file add your IoT Edge Device ConnectionString like this:  
`DEVICE_CONNECTION_STRING='HostName=YOUR_IOTHUB.azure-devices.net;DeviceId=YOUR_DEVICEID;SharedAccessKey=YOUR_SAS_KEY'`
- Using the [Remote Containers Extension for VSCode](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) *Reopen Folder in Container*