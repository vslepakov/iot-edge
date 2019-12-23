const express = require('express');
const bodyParser = require('body-parser');
require('isomorphic-fetch');

const app = express();
app.use(bodyParser.json());

const port = 3000;

app.post('/my-rabbit', (req, res) => {
  console.log("Hello from RabbitMQ!");
  console.log(req.body);
  res.status(200).send();
});

app.listen(port, () => console.log(`Node App listening on port ${port}!`));