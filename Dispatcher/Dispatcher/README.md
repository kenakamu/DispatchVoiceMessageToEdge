# Dispatcher

This folder contains Azure Function sample application which demonstrates following capabilities.

- Get Items from central queue
- Convert text message to audio via Cognitize Text to Speech Service
- Upload converted message to blob and issue SAS token for each item
- Dispatch new item with the blob url to location queue

## local.settings.json

When debug locally, update local.settings.json. 

# Deploy

dotnet core Azure Function can run on both Windows and Linux OS base function applicaiton. As [Setup](./Setup) doesn't provision Azure Function, you can simply deploy from Visual Studio, Visual Studio Code or configure CI/CD by using Azure DevOps or any preferrerd DevOps toolings.

Once you deploy the service, add necessary settings to application settings.