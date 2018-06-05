import React, { Component } from 'react';
import logo from './amido.png';
import './App.css';

import Button from 'react-md/lib/Buttons/Button';

import AWS from 'aws-sdk';

class App extends Component {

    queueUrlPrefix = "https://sqs.eu-west-1.amazonaws.com/525470265062/";
    queueUrlPostfix = ".fifo";
    region = "eu-west-1";
    endoint = "https://sqs.eu-west-1.amazonaws.com";

    constructor() {
        super();
        this.state = {
            queueContents: "",
            queueSize: 0,
            intervalHandler: null,
            running: false
        };
    }

      render() {
        return (
          <div className="App">
            <header className="App-header">
              <img src={logo} className="App-logo" alt="logo" />
              <h1 className="App-title">hey.crispy.wtf</h1>
            </header>
              <p>Left to process: {this.state.queueSize}</p>
              <p><Button raised onClick={() => {
                  this.setState({running: true});
                  this.doWork(this);
                }
              }>Go Map Reduce!</Button>
              </p>
              <Button raised onClick={() => {
                  this.setState({running: false})
              }
              }>Don't Map Reduce!</Button>
          </div>
        );
      }

      getQueueUrl(queueName) {
        return this.queueUrlPrefix + queueName + this.queueUrlPostfix;
      }

    uuidv4() {
        return ([1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        )
    }

    doWork(self) {
        let sqs = new AWS.SQS({apiVersion: '2012-11-05'});//, endpoint: this.endoint});

        sqs.config.update({
            region: this.region,
            accessKeyId: 'AKIAIPV5N5QMERUWT2OQ',
            secretAccessKey: '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'});

        let remoteQueueUrl = self.getQueueUrl('serverless-mapreduce-remoteCommandQueue');

        sqs.getQueueAttributes({
            QueueUrl: remoteQueueUrl,
            AttributeNames: ['ApproximateNumberOfMessages']
        }, function (err, data) {
            if (err) {
                console.log("Get queue size error", err);
            } else if (data.Attributes) {
                self.setState({queueSize: data.Attributes['ApproximateNumberOfMessages']});
            }
        });

        let params = {
            AttributeNames: [
                "SentTimestamp"
            ],
            MaxNumberOfMessages: 1,
            MessageAttributeNames: [
                "All"
            ],
            QueueUrl: self.getQueueUrl('serverless-mapreduce-remoteCommandQueue'),
            VisibilityTimeout: 5,
            WaitTimeSeconds: 0
        };

        sqs.receiveMessage(params, function(err, data) {
            if (err) {
                console.log("Receive Error", err);
                self.triggerAnotherDoWork(self);
            } else if (data.Messages) {
                if (!data.Messages[0]) {
                    self.triggerAnotherDoWork(self);
                    return;
                }

                let obj = JSON.parse(data.Messages[0].Body);
                console.log(obj);

                let writeDataCommand = '';
                if (obj.Command.$type === "ServerlessMapReduceDotNet.MapReduce.Commands.Map.BatchMapDataCommand, ServerlessMapReduceDotNet") {
                    let mostAccidentProneKvps = [];
                    for (let i = 0, len = obj.Command.Lines.$values.length; i < len; i++) {
                        let line = obj.Command.Lines.$values[i];
                        let lineValues = line.split(',');
                        if (lineValues.length > 20) {
                            let manufacturer = lineValues[22].toUpperCase();
                            let mostAccidentProneKvp = {
                                M: manufacturer,
                                S: {
                                    A: 1,
                                    C: 0,
                                    R: 0.0
                                }
                            };
                            mostAccidentProneKvps.push(mostAccidentProneKvp);
                        } else {
                            // registrations stat
                            let manufacturer = lineValues[0].toUpperCase();
                            let dirtyInt = lineValues[2];
                            let cleanInt = dirtyInt.replace(',','').replace('"','');
                            if (cleanInt === '') cleanInt = '0';
                            let noOfRegistrations = parseInt(cleanInt);
                            let mostAccidentProneKvp = {
                                M: manufacturer,
                                S: {
                                    A : 0,
                                    C: noOfRegistrations,
                                    R: 0.0
                                }
                            };
                            mostAccidentProneKvps.push(mostAccidentProneKvp);
                        }
                    }
                    writeDataCommand = {
                        "$type":"AzureFromTheTrenches.Commanding.Abstractions.Model.NoResultCommandWrapper, AzureFromTheTrenches.Commanding.Abstractions",
                        Command: {
                            "$type":"ServerlessMapReduceDotNet.MapReduce.Commands.Map.WriteMappedDataCommand, ServerlessMapReduceDotNet",
                            ResultOfMap2: mostAccidentProneKvps,
                            ContextQueueMessage: obj.Command.ContextQueueMessage
                        }
                    };
                } else if (obj.Command.$type === "ServerlessMapReduceDotNet.MapReduce.Commands.Reduce.BatchReduceDataCommand, ServerlessMapReduceDotNet") {
                    let mostAccidentProneKvps = {};

                    for (let i = 0, len = obj.Command.InputKeyValuePairs2.length; i < len; i++) {
                        if (!mostAccidentProneKvps[obj.Command.InputKeyValuePairs2[i].M]){
                            mostAccidentProneKvps[obj.Command.InputKeyValuePairs2[i].M] = {
                                A: 0,
                                C: 0,
                                R: 0.0
                            }
                        }

                        let data1 = mostAccidentProneKvps[obj.Command.InputKeyValuePairs2[i].M];
                        let data2 = obj.Command.InputKeyValuePairs2[i].S;

                        let newNoOfAccidents = data1.A + data2.A;
                        let newNoOfCarsRegistered = data1.C + data2.C;

                        let registrationsPerAccident = newNoOfCarsRegistered / newNoOfAccidents;
                        if (isFinite(registrationsPerAccident) === false) {
                            registrationsPerAccident = 999999
                        } else {
                            registrationsPerAccident = registrationsPerAccident || 0;
                        }

                        mostAccidentProneKvps[obj.Command.InputKeyValuePairs2[i].M] = {
                            A: newNoOfAccidents,
                            C: newNoOfCarsRegistered,
                            R: registrationsPerAccident
                        };
                    }

                    let reducedData = [];

                    for (let manufacturer in mostAccidentProneKvps) {
                        reducedData.push({
                                "M": manufacturer,
                                "S": mostAccidentProneKvps[manufacturer]
                            });
                    }

                    reducedData.sort(function(a,b) {
                       return b.S.R - a.S.R;
                    });

                    if (reducedData.length !== 0) {
                        writeDataCommand = {
                            "$type":"AzureFromTheTrenches.Commanding.Abstractions.Model.NoResultCommandWrapper, AzureFromTheTrenches.Commanding.Abstractions",
                            Command: {
                                "$type":"ServerlessMapReduceDotNet.MapReduce.Commands.Reduce.WriteReducedDataCommand, ServerlessMapReduceDotNet",
                                ReducedData: reducedData,
                                ProcessedMessageIdsHash: obj.Command.ProcessedMessageIdsHash
                            }
                        };
                    }
                }

                if (writeDataCommand !== '') {

                    console.log(writeDataCommand);

                    sqs.sendMessage({
                        QueueUrl: self.getQueueUrl('serverless-mapreduce-commandQueue'),
                        MessageBody: JSON.stringify(writeDataCommand),
                        MessageGroupId: self.uuidv4(),
                        MessageDeduplicationId: self.uuidv4()
                    }, function(err, data) {
                        if (err) {
                            console.log("Receive Error", err);
                        } else {
                            console.log(data);
                        }
                    });
                } else {
                    console.log("no output");
                }

                let remoteQueue = self.getQueueUrl('serverless-mapreduce-remoteCommandQueue');

                sqs.deleteMessage({
                    QueueUrl: remoteQueue,
                    ReceiptHandle: data.Messages[0].ReceiptHandle
                }, function(err) {
                    if (err) {
                        console.log("Delete Error", err);
                    }
                });

            }
            self.triggerAnotherDoWork(self);
        });
    }

    triggerAnotherDoWork(self) {
        if (this.state.running) {
            setTimeout(function () {
                self.doWork(self);
            }, 2000)
        }
    }

}

export default App;
