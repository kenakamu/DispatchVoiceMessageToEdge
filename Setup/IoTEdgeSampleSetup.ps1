$Resource_Group_Name="IoTEdgeSampleRG"
$Location="eastus"
$Azure_Registry_Name="IoTEdgeSampleACR"
$Azure_IoT_Hub_Name="IoTEdgeSampleHub"
$Azure_Storage_Account_Name="iotedgesamplestorage" #lowercase
$Azure_CognitiveService_Account_Name="IoTEdgeSampleCog"

# Login To Azure
az login
# Check the subscription
az account list
# Create Resource Group
az group create --name $Resource_Group_Name --location $Location
# Create Azure Container Registry
az acr create --name $Azure_Registry_Name --resource-group $Resource_Group_Name --location $Location --admin-enabled $true --sku Basic --output tsv
# Create Azure IoT Hub
az iot hub create --name $Azure_IoT_Hub_Name --resource-group $Resource_Group_Name --sku s1 
# Create Azure Storage for Queue and Blob
az storage account create --name $Azure_Storage_Account_Name --resource-group $Resource_Group_Name --location $Location --kind storage
# Store Connection String
$Azure_Storage_Connection_String = az storage account show-connection-string --name $Azure_Storage_Account_Name --resource-group $Resource_Group_Name --query connectionString
# Create Queues
az storage queue create --name "central" --connection-string $Azure_Storage_Connection_String
az storage queue create --name "location1" --connection-string $Azure_Storage_Connection_String
# Create Blob Container
az storage container create --name audio --connection-string $Azure_Storage_Connection_String
$Storage_SAS_Token = az storage container generate-sas --name audio --connection-string $Azure_Storage_Connection_String --permissions r
# Create Speech Cognitive Service
az cognitiveservices account create --name $Azure_CognitiveService_Account_Name --resource-group $Resource_Group_Name --kind SpeechServices --sku F0 --location $Location

# Results
az acr show --name $Azure_Registry_Name --resource-group $Resource_Group_Name --query loginServer
az acr credential show --name $Azure_Registry_Name --resource-group $Resource_Group_Name --query username
az acr credential show --name $Azure_Registry_Name --resource-group $Resource_Group_Name --query passwords[0].value
az cognitiveservices account keys list --name $Azure_CognitiveService_Account_Name --resource-group $Resource_Group_Name --query key1
az cognitiveservices account show --name $Azure_CognitiveService_Account_Name --resource-group $Resource_Group_Name --query endpoint
$Azure_CognitiveService_Account_Name
$Azure_Storage_Connection_String
$Storage_SAS_Token
az storage account show --name $Azure_Storage_Account_Name --query primaryEndpoints.blob 