# mbtx-assessment
Greenhouse Guard test assessment

## Branches

### main
* main is main.
* Demo project from here.

### alpha
* abandoned (do not use or consider)

### beta
* current active development

## Build & Run

### Install prerequisites
* dotnet SDK for .NET 10
* node and npm with a version supported by Angular 19; Node 20 LTS is the safe default

### Build
* clone repo
* checkout 'main' if not already there
* dotnet restore mbtx-assessment.sln (only required on initial checkout)
* cd ClientApp
* npm ci (only required on initial checkout)
* npm run build
* cd ..
* dotnet build
* dotnet test

### Run
* dotnet run --project MbtxAssessment/mbtx-assessment.csproj
* NOTE #1: When running from command line, you have to specify the --project since setting a default project in .sln file isn't available.
* NOTE #2: On start-up, you may see an error message "THUMP! Bad comms with server. Will retry in 10 seconds".  This is because the TestSensorReadingGenerator is started by the app, and at first it may try to post readings before the API is ready to receive them.
* navigate to --> localhost:5241

## Notes & Future Considerations
* Could not get vitest to behave properly in dev branch.  'npm test' runs ok, but 'npm run build' produces many error.  Leaving UI test code in its branch for now.
* Simplified the SignalR hub broadcast to "send all", removing the need for client registration and store. A more robust client handler might be needed in the future.
* hi/lo limits on the data could be specified using the UI, and stored server-side for validation before broadcast
* other business rules & validations could be performed server-side, such as data quality and staleness.
* TestSensorReadingGenerator is very un-graceful about handling connections to the API.  As a convinience for dev testing, this test process is currently integrated into the backend and started when the backend is started.  It should really be seperated from the main API application and run separately.  
* SensorService return values from ProcessNewReadings should be enhanced to allow calling processes to better understand and react if something goes wrong.
* The Anomalies and Sensor Reading tables on the UI are paginated completely in the browser.  This should be re-worked to include communication and coordination with the back-end to avoid memory bloat on the browser.
* The data stores and service classes do not use interfaces. For a more complicated system, introduing Interfaces will better prepare for use of different implementations for data stores and services. For example, using in-memory data store vs a cloud NoSQL solution. As well, when expanding the unit testing framework, Interfaces will allow for mocking of those classes.
