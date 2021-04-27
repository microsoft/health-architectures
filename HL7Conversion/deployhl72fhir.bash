#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

# -e: immediately exit if any command has a non-zero exit status
# -o: prevents errors in a pipeline from being masked
# IFS new value is less likely to cause confusing bugs when looping arrays or arguments (e.g. $@)
#
#HL72FHIR Workf Setup --- Author Steve Ordahl Principal Architect Health Data Platform
#

usage() { echo "Usage: $0 -i <subscriptionId> -g <resourceGroupName> -l <resourceGroupLocation> -p <prefix> -k <keyvault>" 1>&2; exit 1; }

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
declare defsubscriptionId=""
declare subscriptionId=""
declare resourceGroupName=""
declare resourceGroupLocation=""
declare deployprefix=""
declare defdeployprefix=""
declare hl7storename=""
declare hl7sbqueuename=""
declare hl7storekey=""
declare hl7sbconnection=""
declare hl7convertername="hl7conv"
declare hl7converterrg=""
declare hl7convertkey=""
declare hl7converterinstance=""
declare stepresult=""
declare fahost=""
declare kvname=""
declare kvexists=""
declare fpclientid=""
declare fptenantid=""
declare fpsecret=""
declare fphost=""
declare repurls=""
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
	echo "Ente keyvault name that contains ingest and proxy configuration: "
	read kvname
fi
if [ -z "$subscriptionId" ] || [ -z "$resourceGroupName" ] || [ -z "$kvname" ]; then
	echo "Either one of subscriptionId, resourceGroupName or keyvault is empty"
	usage
fi
echo "Setting subscription id..."
#set the default subscription id
az account set --subscription $subscriptionId
#Check KV exists
echo "Checking for keyvault "$kvname"..."
kvexists=$(az keyvault list --query "[?name == '$kvname'].name" --out tsv)
if [[ -z "$kvexists" ]]; then
	echo "Cannot Locate Key Vault "$kvname" this deployment requires access to the proxy keyvault...Is the Proxy Installed?"
	exit 1
fi
#Prompt for Converter Resource group to avoid function app conflicts
echo "Enter a resource group name to deploy the converter to ["$deployprefix$hl7convertername"]:"
read hl7converterrg
if [ -z "$hl7converterrg" ] ; then
 	 hl7converterrg=$deployprefix$hl7convertername
fi
if [ "$hl7converterrg" = "$resourceGroupName" ]; then
    echo "The converter resource group cannot be the same as the target resource group"
	exit 1;
fi
if [ -z "$hl7converterrg" ] ; then
 	 hl7converterrg=$deployprefix$hl7convertername
fi

echo "Checking resource groups..."

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

if [ $(az group exists --name $hl7converterrg) = false ]; then
	echo "Resource group with name" $hl7converterrg "could not be found. Creating new resource group.."
	set -e
	(
		set -x
		az group create --name $hl7converterrg --location $resourceGroupLocation 1> /dev/null
	)
else
	echo "Using existing resource group for converter deployment..."
fi
#Start deployment
echo "Starting HL72FHIR Workflow Platform deployment..."
(
		#Deploy HL7 FHIR Converter
		hl7converterinstance=$deployprefix$hl7convertername$RANDOM
		echo "Loading configuration settings from key vault "$kvname"..."
		faname=$(az keyvault secret show --vault-name $kvname --name FP-HOST --query "value" --out tsv)
		#Get and Parse HL7 Storage Account String
		stepresult=$(az keyvault secret show --vault-name $kvname --name HL7ING-STORAGEACCT --query "value" --out tsv)
		IFS=';' read -ra ADDR <<< $stepresult
		for i in "${ADDR[@]}"; do
			if [[ $i = AccountName* ]]
			then
			   hl7storename=${i/AccountName=/}
			fi
			if [[ $i = AccountKey* ]]
			then
			   hl7storekey=${i/AccountKey=/}
			fi
		done
		IFS=$'\n\t'
		hl7sbconnection=$(az keyvault secret show --vault-name $kvname --name HL7ING-SERVICEBUS-CONNECTION --query "value" --out tsv)
		hl7sbqueuename=$(az keyvault secret show --vault-name $kvname --name HL7ING-QUEUENAME --query "value" --out tsv)
		fpclientid=$(az keyvault secret show --vault-name $kvname --name FP-RBAC-CLIENT-ID --query "value" --out tsv)
		fptenantid=$(az keyvault secret show --vault-name $kvname --name FP-RBAC-TENANT-NAME --query "value" --out tsv)
		fpsecret=$(az keyvault secret show --vault-name $kvname --name FP-RBAC-CLIENT-SECRET --query "value" --out tsv)
		fphost=$(az keyvault secret show --vault-name $kvname --name FP-HOST --query "value" --out tsv)
		repurls="https://"$fphost"/.auth/login/aad/callback https://logic-apis-"$resourceGroupLocation".consent.azure-apim.net/redirect"
		echo "Deploying FHIR Converter ["$hl7converterinstance"] to resource group ["$hl7converterrg"]..."
		stepresult=$(az deployment group create -g $hl7converterrg --template-uri https://raw.githubusercontent.com/microsoft/FHIR-Converter/master/deploy/default-azuredeploy.json --parameters serviceName=$hl7converterinstance)
		hl7convertkey=$(az functionapp config appsettings list --resource-group $hl7converterrg --name $hl7converterinstance --query "[?name == 'CONVERSION_API_KEYS'].value" --out tsv)
		echo "Deploying Custom Logic App Connector for FHIR Server Proxy..."
		stepresult=$(az deployment group create -g $resourceGroupName --template-file hl7tofhir/LogicAppCustomConnectors/fhir_server_connect_template.json  --parameters fhirserverproxyhost=$faname fhirserverproxyclientid=$fpclientid fhirserverproxytenantid=$fptenantid fhirserverproxyclientsecret=$fpsecret)
		echo "Deploying Custom Logic App Connector for FHIR Converter..."
		stepresult=$(az deployment group create -g $resourceGroupName --template-file hl7tofhir/LogicAppCustomConnectors/fhir_converter_connect_template.json --parameters fhirconverterhost=$hl7converterinstance".azurewebsites.net")
		echo "Deploying HL72FHIR Logic App..."
		stepresult=$(az deployment group create -g $resourceGroupName --template-file hl7tofhir/hl72fhir.json  --parameters HL7FHIRConverter_1_api_key=$hl7convertkey azureblob_1_accountName=$hl7storename azureblob_1_accessKey=$hl7storekey servicebus_1_connectionString=$hl7sbconnection servicebus_1_queue=$hl7sbqueuename)
		set +e
		echo "Updating callback URLs for FHIR Proxy SP from Logic App..."
		stepresult=$(az ad app update --id $fpclientid --reply-urls $repurls)
		echo " "
		echo "************************************************************************************************************"
		echo "HL72FHIR Workflow Platform has successfully been deployed to group "$resourceGroupName" on "$(date)
		echo "Please note the following reference information for future use:"
		echo "Your HL7 FHIR Converter Host is: "$hl7converterinstance
		echo "Your HL7 FHIR Converter Key is: "$hl7convertkey
		echo "Your HL7 FHIR Converter Resource Group is: "$hl7converterrg
		echo "************************************************************************************************************"
		echo " "
)
	
if [ $?  == 0 ];
 then
	echo "HL72FHIR Workflow Platform has successfully been deployed"
fi
