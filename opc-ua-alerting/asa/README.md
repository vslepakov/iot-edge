# Sample IoT Edge ASA alert query for OPC-UA data from OPC-Publisher

Refer to [this docs](https://docs.microsoft.com/en-us/azure/stream-analytics/cicd-tools?tabs=visual-studio-code) for details.  

## Run unit tests

```azure-streamanalytics-cicd test -project asaproj.json```  
Comment out like this ```--System.Timestamp() as Timestamp``` in ```Transformation.asaql``` before you run the tests.  
Currently the test framework cannot deal with values changing on each run.