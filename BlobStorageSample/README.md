## BlobStorage Sample

This sample demonstrates how to use Blob Storage module on Edge Devices.
Data published by the SimulatedTemperatureSensorModule is routed to the BlobArchiverModule.
BlobArchiverModule collects 50 messages and writes a file to the local Blob Storage (module).
The Blob Storage Module is configured to upload its content to a Cloud Storage Account.

Before you can run this sample please modify deployment.template.json and deployment.debug.template.json as follows:

* in Line 12 provide a value for LOCAL_STORAGE_ACCOUNT_NAME. This is the name you want to use for your local storage account in Blob Storage Module

* in Line 13 provide value for LOCAL_STORAGE_ACCOUNT_KEY. It can be a random 64-byte base64 key. You can generate one [here](https://generate.plus/en/base64?gp_base64_base%5Blength%5D=64)

* in Line 42 provide BLOB_STORAGE_CONNECTION_STR. This is the connection string to your local Blob Storage. It has this form: *"DefaultEndpointsProtocol=http;BlobEndpoint=http://azureblobstorageoniotedge:11002/[ACCOUNT NAME];AccountName=[ACCOUNT NAME];AccountKey=[ACCOUNT KEY];"*

* in Line 133 provide your Cloud Storage Connection String. It used to automatically (and optionally) upload files from the local storage to the Cloud storage

* in Line 135 use the name of your local Blob container

* in Line 136 use the name of your Cloud Blob container

* in VSCode generate IoT Edge Deployment Manifest

* Deploy: az iot edge deployment create --deployment-id [deployment id] --hub-name [hub name] --content [file path] --labels "[labels]" --target-condition "[target query]" --priority [int].  
**Example:** *az iot edge deployment create --deployment-id blobtestdeployment --hub-name vislepakhub --content config\deployment.amd64.json --target-condition "deviceId='vislepak-edge'" --priority 5*