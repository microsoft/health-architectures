#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

# -e: immediately exit if any command has a non-zero exit status
# -o: prevents errors in a pipeline from being masked
# IFS new value is less likely to cause confusing bugs when looping arrays or arguments (e.g. $@)
#
#HL7 Ingest Setup --- Author Steve Ordahl Principal Architect Health Data Platform
#

usage() { echo "Usage: $0 -i <subscriptionId> -g <resourceGroupName> -l <resourceGroupLocation> -p <prefix> -k <keyvault>" 1>&2; exit 1; }

declare defsubscriptionId=""
declare subscriptionId=""
declare resourceGroupName=""
declare resourceGroupLocation=""
declare storageAccountNameSuffix="store"$RANDOM
declare storageConnectionString=""
declare busnamespaceName="hlsb"$RANDOM
declare busqueue="hl7ingest"
declare defkvname="kv"$RANDOM
declare sbconnectionString=""
declare serviceplanSuffix="asp"
declare serviceplansku="B1"
declare faname=hl7ingest$RANDOM
declare deployzip="hl7ingest/distribution/publish.zip"
declare deployprefix=""
declare defdeployprefix=""
declare storecontainername="hl7"
declare stepresult=""
declare fahost=""
declare fakey=""
declare faresourceid=""
declare kvname=""
declare kvexists=""
function fail {
  echo $1 >&2
  exit 1
}

function retry {
  local n=1
  local max=5
  local delay=15
  while true; do
    "$@" && break || {
      if [[ $n -lt $max ]]; then
        ((n++))
        echo "Command failed. Retry Attempt $n/$max in $delay seconds:" >&2
        sleep $delay;
      else
        fail "The command has failed after $n attempts."
      fi
    }
  done
}
function kvuri {
	echo "@Microsoft.KeyVault(SecretUri=https://"$kvname".vault.azure.net/secrets/"$@"/)"
}

# Initialize parameters specified from command line
while getopts ":i:g:n:l:p" arg; do
	case "${arg}" in
		p)
			deployprefix=${OPTARG:0:14}
			deployprefix=${deployprefix,,}
			deployprefix=${deployprefix//[^[:alnum:]]/}
			;;
		i)
			subscriptionId=${OPTARG}
			;;
		g)
			resourceGroupName=${OPTARG}
			;;
		l)
			resourceGroupLocation=${OPTARG}
			;;
		k)
			kvname=${OPTARG}
			;;
		esac
done
shift $((OPTIND-1))
echo "Executing "$0"..."
echo "Checking Azure Authentication..."
#login to azure using your credentials
az account show 1> /dev/null

if [ $? != 0 ];
then
	az login
fi

defsubscriptionId=$(az account show --query "id" --out json | sed 's/"//g') 

#Prompt for parameters is some required parameters are missing
if [[ -z "$subscriptionId" ]]; then
	echo "Enter your subscription ID ["$defsubscriptionId"]:"
	read subscriptionId
	if [ -z "$subscriptionId" ] ; then
		subscriptionId=$defsubscriptionId
	fi
	[[ "${subscriptionId:?}" ]]
fi

if [[ -z "$resourceGroupName" ]]; then
	echo "This script will look for an existing resource group, otherwise a new one will be created "
	echo "You can create new resource groups with the CLI using: az group create "
	echo "Enter a resource group name"
	read resourceGroupName
	[[ "${resourceGroupName:?}" ]]
fi

defdeployprefix=${resourceGroupName:0:14}
defdeployprefix=${defdeployprefix//[^[:alnum:]]/}
defdeployprefix=${defdeployprefix,,}

if [[ -z "$resourceGroupLocation" ]]; then
	echo "If creating a *new* resource group, you need to set a location "
	echo "You can lookup locations with the CLI using: az account list-locations "
	
	echo "Enter resource group location:"
	read resourceGroupLocation
fi
#Prompt for parameters is some required parameters are missing
if [[ -z "$deployprefix" ]]; then
	echo "Enter your deployment prefix ["$defdeployprefix"]:"
	read deployprefix
	if [ -z "$deployprefix" ] ; then
		deployprefix=$defdeployprefix
	fi
	deployprefix=${deployprefix:0:14}
	deployprefix=${deployprefix//[^[:alnum:]]/}
    deployprefix=${deployprefix,,}
	[[ "${deployprefix:?}" ]]
fi
if [[ -z "$kvname" ]]; then
	echo "Enter an existing (e.g. your FHIR Proxy keyvault name) or new keyvault name to use ["$deployprefix$defkvname"]:"
	read kvname
	if [ -z "$kvname" ] ; then
		kvname=$deployprefix$defkvname
	fi
	[[ "${kvname:?}" ]]
fi
if [ -z "$subscriptionId" ] || [ -z "$resourceGroupName" ]; then
	echo "Either one of subscriptionId, resourceGroupName is empty"
	usage
fi

echo "Setting default subscription and resource group..." 
#set the default subscription id
az account set --subscription $subscriptionId

set +e

#Check for existing RG
if [ $(az group exists --name $resourceGroupName) = false ]; then
	echo "Resource group with name" $resourceGroupName "could not be found. Creating new resource group.."
	set -e
	(
		set -x
		az group create --name $resourceGroupName --location $resourceGroupLocation 1> /dev/null
	)
	else
	echo "Using existing resource group..."
fi
#Set up variables
faresourceid="/subscriptions/"$subscriptionId"/resourceGroups/"$resourceGroupName"/providers/Microsoft.Web/sites/"$faname
#Check KV exists
kvexists=$(az keyvault list --query "[?name == '$kvname'].name" --out tsv)
if [[ -z "$kvexists" ]]; then
	echo "Creating Key Vault ["$kvname"]..."
	stepresult=$(az keyvault create --name $kvname --resource-group $resourceGroupName --location  $resourceGroupLocation)
else
	echo "Using existing keyvault "$kvname"..."
fi
#Start deployment
echo "Starting HL7 Ingest Platform deployment..."
(
		#set -x
		#Create Storage Account
		echo "Creating Storage Account ["$deployprefix$storageAccountNameSuffix"]..."
		stepresult=$(az storage account create --name $deployprefix$storageAccountNameSuffix --resource-group $resourceGroupName --location  $resourceGroupLocation --sku Standard_LRS --encryption-services blob)
		echo "Retrieving Storage Account Connection String..."
		storageConnectionString=$(az storage account show-connection-string -g $resourceGroupName -n $deployprefix$storageAccountNameSuffix --query "connectionString" --out tsv)
		stepresult=$(az keyvault secret set --vault-name $kvname --name "HL7ING-STORAGEACCT" --value $storageConnectionString)
		echo "Creating Storage Account Container ["$storecontainername"]..."
		stepresult=$(az storage container create -n $storecontainername --connection-string $storageConnectionString)
		stepresult=$(az keyvault secret set --vault-name $kvname --name "HL7ING-STORAGEACCT-BLOB-CONTAINER" --value $storecontainername)
		#Create Service Bus Namespace and Queue
		echo "Creating Service Bus Namespace ["$busnamespaceName"]..."
		stepresult=$(az servicebus namespace create --resource-group $resourceGroupName --name $busnamespaceName --location $resourceGroupLocation)
		#Create hl7 ingest queue
		echo "Creating Queue ["$busqueue"]..."
		stepresult=$(az servicebus queue create --resource-group $resourceGroupName --namespace-name $busnamespaceName --name $busqueue)
		echo "Retrieving ServiceBus Connection String..."
		sbconnectionString=$(az servicebus namespace authorization-rule keys list --resource-group $resourceGroupName --namespace-name $busnamespaceName --name RootManageSharedAccessKey --query primaryConnectionString --output tsv)
		stepresult=$(az keyvault secret set --vault-name $kvname --name "HL7ING-SERVICEBUS-CONNECTION" --value $sbconnectionString)
		stepresult=$(az keyvault secret set --vault-name $kvname --name "HL7ING-QUEUENAME" --value $busqueue)
		#Create HL7OverHTTPS Ingest Functions App
		#Create Service Plan
		echo "Creating hl7ingest Function App Serviceplan["$deployprefix$serviceplanSuffix"]..."
		stepresult=$(az appservice plan create -g  $resourceGroupName -n $deployprefix$serviceplanSuffix --number-of-workers 2 --sku $serviceplansku)
		#Create the Transform Function App
		echo "Creating hl7ingest Function App ["$faname"]..."
		fahost=$(az functionapp create --name $faname --storage-account $deployprefix$storageAccountNameSuffix  --plan $deployprefix$serviceplanSuffix  --resource-group $resourceGroupName --runtime dotnet --os-type Windows --functions-version 3 --query defaultHostName --output tsv)
		echo "Creating MSI for Function App..."
		msi=$(az functionapp identity assign -g $resourceGroupName -n $faname --query "principalId" --out tsv)
		echo "Setting KeyVault Policy to allow secret access..."
		stepresult=$(az keyvault set-policy -n $kvname --secret-permissions list get set --object-id $msi)
		echo "Retrieving Function App Host Key..."
		fakey=$(retry az rest --method post --uri "https://management.azure.com"$faresourceid"/host/default/listKeys?api-version=2018-02-01" --query "functionKeys.default" --output tsv)
		#Add App Settings
		#StorageAccount
		echo "Configuring hl7ingest Function App ["$faname"]..."
		stepresult=$(az functionapp config appsettings set --name $faname  --resource-group $resourceGroupName --settings HL7ING-STORAGEACCT=$(kvuri HL7ING-STORAGEACCT) HL7ING-STORAGEACCT-BLOB-CONTAINER=$(kvuri HL7ING-STORAGEACCT-BLOB-CONTAINER) HL7ING-SERVICEBUS-CONNECTION=$(kvuri HL7ING-SERVICEBUS-CONNECTION) HL7ING-QUEUENAME=$(kvuri HL7ING-QUEUENAME))
		#deeployment from devops repo
		echo "Deploying hl7ingest Function App from source repo to ["$fahost"]..."
		stepresult=$(retry az functionapp deployment source config-zip --name $faname --resource-group $resourceGroupName --src $deployzip)
		echo " "
		echo "************************************************************************************************************"
		echo "HL7 Ingest Platform has successfully been deployed to group "$resourceGroupName" on "$(date)
		echo "Please note the following reference information for future use:"
		echo "Your ingest host is: https://"$fahost
		echo "Your ingest host key is: "$fakey
		echo "Your app configuration settings are stored securely in KeyVault: "$kvname
		echo "************************************************************************************************************"
		echo " "
)
	
if [ $? != 0 ];
 then
	echo "Health Data Ingest had errors. Consider deleting resource group "$resourceGroupName" and trying again..."
fi
