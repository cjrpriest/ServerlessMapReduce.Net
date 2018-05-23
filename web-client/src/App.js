import React, { Component } from 'react';
import logo from './logo.svg';
import './App.css';

import Button from 'react-md/lib/Buttons/Button';

import AWS from 'aws-sdk';
// import { Credentials } from 'aws-sdk/lib/credentials';

class App extends Component {

    constructor() {
        super();
        this.state = {
            queueContents: ""
        };
    }

      render() {
        return (
          <div className="App">
            <header className="App-header">
              <img src={logo} className="App-logo" alt="logo" />
              <h1 className="App-title">Welcome to React</h1>
            </header>
            <p className="App-intro">
              To get started, edit <code>src/App.js</code> and save to reload.
            </p>
              <Button raised onClick={() => this.getQueueInfo()}>Go!</Button>
          </div>
        );
      }

    getQueueInfo() {
        let sqs = new AWS.SQS({apiVersion: '2012-11-05', endpoint: 'http://localhost:9324'});

        let creds = new AWS.Credentials('foo', 'bar');

        sqs.config.update(creds);
        sqs.config.update({region: 'eu-central-1'});

        let params = {};

        sqs.listQueues(params, function(err, data) {
            if (err) {
                console.log("Error", err);
            } else {
                console.log("Success", data.QueueUrls);
            }
        });
    }

}

export default App;
