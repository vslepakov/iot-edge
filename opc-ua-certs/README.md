# Some Context
Please refer to [this blog post](https://vslepakov.medium.com/custom-x509-certificates-with-opc-publisher-73a0127bf884) for details.

## Step 1

Clone this repo. It is important that ```iiot_certs_openssl.cnf``` is in the same directory as the ```certGen.sh``` script.

## Step 2: Generate Root and Intermediate Certificates

```./certGen.sh create_root_and_intermediate```

## Step 3: Generate OPC UA Application Instance Certificate

```./certGen.sh create_opcua_certificate <GATEWAY_HOSTNAME> <COMMON_NAME>```

### Example

```./certGen.sh create_opcua_certificate vislepak-edge.westeurope.cloudapp.azure.com opcpublisher```

will generate these files in the ```certs``` folder:

- opcpublisher.cert.der
- opcpublisher.cert.pfx
- opcpublisher.cert.pem
- opcpublisher-full-chain.cert.pem

For OPC Publisher we are only interested in the first two.

The Subject will be:

```Subject: C = US, L = Redmond, O = Contoso Inc., OU = Contoso IT Security, CN = opcpublisher, emailAddress = iiotsupport@contoso.com, DC = vislepak-edge.westeurope.cloudapp.azure.com```  

subjectAltName:

```X509v3 Subject Alternative Name:```  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;```URI:urn:Redmond:factory1:productionline1:opcpublisher, DNS:vislepak-edge.westeurope.cloudapp.azure.com```


