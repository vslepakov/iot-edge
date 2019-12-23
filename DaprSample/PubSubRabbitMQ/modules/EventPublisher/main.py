import time
import json
import requests
import os

dapr_port = os.getenv("DAPR_HTTP_PORT", 3500)
dapr_url = "http://localhost:{}/v1.0/publish/my-rabbit".format(dapr_port)
headers = {'Content-type': 'application/json', 'Accept': 'application/json'}

n = 0
while True:
    n += 1
    payload = { "data": {"orderId": n}}
    print(payload, flush=True)
    try:
        response = requests.post(dapr_url, data=json.dumps(payload), headers=headers)
        print(response.text, flush=True)

    except Exception as e:
        print(e)

    time.sleep(1)