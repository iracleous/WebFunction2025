 using bash

#list of resourse groups as list
az group list --output table
az group delete --name <resource-group-name> --yes --no-wait




#define constants
resource_group_name="myResourceGroup"
location="northeurope"
storage_account_name="sia23vati2025"

# Create a resource group
az group create --name ${resource_group_name} --location ${location}
 
# Create a storage account with Standard_LRS SKU
 
az storage account create --name ${storage_account_name} --resource-group ${resource_group_name} \
	--location ${location} --sku Standard_LRS


#retreive storage account connection string

storage_account_connection_string=$(az storage account show-connection-string --name ${storage_account_name} --resource-group ${resource_group_name} --query connectionString --output tsv)

echo ${storage_account_connection_string}


 #1 create storage account with table container
 az storage table create --name myTable --account-name ${storage_account_name} --connection-string "${storage_account_connection_string}"


 #######

 # Create a storage container for blobs
 az storage container create --name myBlobContainer --account-name ${storage_account_name} --connection-string "${storage_account_connection_string}"


 ### --->

 # Create an App Service plan
 az appservice plan create --name myAppServicePlan --resource-group ${resource_group_name} \
 --location ${location} --sku S1 --is-linux


functionApp="myFunctwrt2025"

 #2 create function app with storage account
 az functionapp create --name ${functionApp} --storage-account ${storage_account_name} \
 --resource-group ${resource_group_name} --plan myAppServicePlan --runtime dotnet --runtime-version 8.0 --functions-version 4 --os-type Linux

 
 #######
 #  pull code from github
repoUrl="https://github.com/iracleous/WebFunction2025"
branch="master"

# Configure deployment from GitHub
az functionapp deployment source config \
  --name "$functionApp" \
  --resource-group "$resourceGroup" \
  --repo-url "$repoUrl" \
  --branch "$branch" \
  --manual-integration



 #1 create storage account with table container

 #3 create function with HTTP trigger and link to storage account
 #4 create function with timer trigger and link to storage account
