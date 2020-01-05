# Azure Resouce Setup
 
[IoTEdgeSampleSetup.ps1](./IoTEdgeSampleSetup.ps1) contains sample script to setup following Azure Resources.

- Resource Group
- Azure Container Registry
- Azure IoT Hub
- Azure Storage Account
- Queues ('centra' and 'location1' queue)
- Container ('audio')
- Speech Cognitive Serivce

- Virtual Machine as DevOps Build Agent (optional)

To use the script, you need to install [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) and PowerShell.

# DevOps build agent

If you are using AMR32 devices such as Raspberry Pi, then you need to build the IoT Edge module on ARM32 host. As Microsoft doesn't host ARM32 build agent for Azure DevOps, you need to either use ARM32 physical device of emulated server.

[SetupDevOpsAgent.bash](./SetupDevOpsAgent.bash) contains necessary commands to make provisioned VM to build agent.

Once you crate VM and run the script, then follow DevOps document to make the VM as build agent.
[Self-hosted Linux agents](https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/v2-linux?view=azure-devops)
