# Tinamous Measurements Processor

This service is reponsible for taking new measurements from the Raw Measurements Kinesis stream, processing the measurement and then publishing it on the Processed Measurements Kinesis stream.

The Raw Measurements stream has measurements posted to it via the web API, through bots such as the Particle Bot, and other sources.

Each measurement is checked for the fields supplied, any calibration or other adjustments needed are applied. Hence the service needs to have details of the device field definitions.

The Processed Measurements stream delivers these new measurements to the persistance worker, and to other services (web / notifier etc).

## EasyNetQ integration

Processed measurments Stream is monitored with new measurements published on that from the Processed Measurement stream. 

This will be removed in the future to move away from loading the RabbitMQ servers with measurement data, it's present for now to allow for existing services that aren't picking up from Kinesis to handle the messages.


## Notes 

This is extracted from the main Measurements Service and is still work in process. For now the measurements service is still handing the processing.

This should only be run as a single instance to prevent clashing over stream reading.

Logging / Error reporting still to be properly configured.



