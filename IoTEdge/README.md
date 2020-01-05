# PlayMessage IoT Edge sample

This folder contains IoT Edge module which demonstrate following capabilities.

- Receipt messsage(s) from Azure Queue
- Play audio by using mplayer inside IoT Edge
- How to describe DockerFile for ARM32

In this sample, the module monitor queue name "location1" which you can configure by chainging desired property value.

## Dockerfile.arm32v7 

IoT Edge is Docker Container based solution. It uses Dockerfile.<platform> file to define the container.
This sample uses "mplayer" to play the audio file, and setup information is included in Dockerfile.arm32v7 file.

### Install mplayer

Using apt-get to install mplayer.

```
RUN apt-get update && apt-get install -y mplayer
```

### Add user to audio group

To IoT Edge to play the sound, the execution user has to be audio group member.

```
RUN useradd -ms /bin/bash moduleuser
RUN adduser moduleuser audio
USER moduleuser
```

## deployment.template.json

This file contains following information.

- Container Registory
- Module information
- Desired Property for each modules
- Message routing information

You have to map the physical device capabilities to a container to use the feature such as audio and video.
modules\PlayMessage section contains "HostConfig" to map local device sound ("/dev/snd") to container.

```
"HostConfig": {
    "Devices":[{"PathOnHost":"/dev/snd","PathInContainer":"/dev/snd","CgroupPermissions":"mrw"}]
}
```

## .env

.env file contains environment variable which will be used by modules when you debug or build module locally. Update settings to match your Azure environment.

# Configure IoT Edge device

First of all, you need to register your device as IoT Edge device to Azure IoT Hub.

See [Install the Azure IoT Edge runtime on Debian-based Linux systems](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux#install-the-latest-runtime-version) and [Register an Azure IoT Edge device](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-register-device) for detail.

# How to deploy module

There are two steps to deploy the module to IoT Edge device.

## Upload IoT Edge module container to Azure Container Registory

Build the module and upload it to ACR, so that IoT Edge device can download the container.

## Update deployment manifest to IoT Hub

IoT Edge runtime communicates with IoT Hub to check if there is new update for configuration. To deploy the new module, you need to upload new configuration manifest for the IoT Edge device to IoT Hub.

There are two ways to achieve the above steps.

## Deploy from Local

You can use Visual Studio Code and IoT Edge Extension to deliver the module from local computer.
See [Azure IoT Edge Extension for Visual Studio](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-edge) for more detail.

## Deploy using DevOps

Another way is to use CI/CD pipeline of Azure DevOps.
See [DevOps](../DevOps) folder for more detail.