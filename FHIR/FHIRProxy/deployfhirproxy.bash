#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

# -e: immediately exit if any command has a non-zero exit status
# -o: prevents errors in a pipeline from being masked
# IFS new value is less likely to cause confusing bugs when looping arrays or arguments (e.g. $@)
#
#HL7 Ingest Setup --- Author Steve Ordahl Principal Architect Health Data Platform
#

usage() { echo "Usage: $0 -i <subscriptionId> -g <resourceGroupName> -l <resourceGroupLocation> -p <prefix>" 1>&2; exit 1; }

declare defsubscriptionId=""
declare subscriptionId=""
declare resourceGroupName=""
declare resourceGroupLocation=""
declare storageAccountNameSuffix="store"$RANDOM
declare storageConnectionString=""
declare serviceplanSuffix="asp"
declare faname=sfp$RANDOM
declare fsurl=""
declare fsclientid=""
declare fssecret=""
declare fstenant=""
declare fsaud=""
declare fsdefaud="https://azurehealthcareapis.com"
declare deployzip="distribution/publish.zip"
declare deployprefix=""
declare defdeployprefix=""
declare storecontainername="deid"
declare stepresult=""
declare fahost=""
declare fakey=""
declare faresourceid=""
declare roleadmin="Administrator"
declare rolereader="Reader"
declare rolewriter="Writer"
declare rolepatient="Patient"
declare roleparticipant="Practitioner,RelatedPerson"
declare roleglobal="DataScientist"
declare roledeid="DataScientist"
declare spappid=""
declare spsecret=""
declare sptenant=""
declare spreplyurls=""
declare tokeniss=""
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
if [ -z "$subscriptionId" ] || [ -z "$resourceGroupName" ]; then
	echo "Either one of subscriptionId, resourceGroupName is empty"
	usage
fi
#Prompt for FHIR Server Parameters
echo "Enter the destination FHIR Server URL:"
read fsurl
if [ -z "$fsurl" ] ; then
	echo "You must provide a destination FHIR Server URL"
	exit 1;
fi
echo "Enter the FHIR Server Service Client Application ID:"
read fsclientid
if [ -z "$fsclientid" ] ; then
	echo "You must provide a Service Client Application ID"
	exit 1;
fi
echo "Enter the FHIR Server Service Client Secret:"
read fssecret
if [ -z "$fssecret" ] ; then
	echo "You must provide a Service Client Secret"
	exit 1;
fi
echo "Enter the FHIR Server/Service Client Audience/Resource ["$fsdefaud"]:"
	read fsaud
	if [ -z "$fsaud" ] ; then
		fsaud=$fsdefaud
	fi
	[[ "${fsaud:?}" ]]
echo "Enter the FHIR Server/Service Client Tenant ID:"
read fstenant
if [ -z "$fstenant" ] ; then
	echo "You must provide a FHIR Server/Service Client Tenant"
	exit 1;
fi

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
#Start deployment
echo "Starting Secure FHIR Proxy deployment..."
(
		#set -x
		#Create Storage Account
		echo "Creating Storage Account ["$deployprefix$storageAccountNameSuffix"]..."
		stepresult=$(az storage account create --name $deployprefix$storageAccountNameSuffix --resource-group $resourceGroupName --location  $resourceGroupLocation --sku Standard_LRS --encryption-services blob)
		echo "Retrieving Storage Account Connection String..."
		storageConnectionString=$(az storage account show-connection-string -g $resourceGroupName -n $deployprefix$storageAccountNameSuffix --query "connectionString" --out tsv)
		echo "Creating Storage Account Container ["$storecontainername"]..."
		stepresult=$(az storage container create -n $storecontainername --connection-string $storageConnectionString)
		#Create FHIR Proxy Function App
		#Create Service Plan
		echo "Creating Secure FHIR Proxy Function App Serviceplan["$deployprefix$serviceplanSuffix"]..."
		stepresult=$(az appservice plan create -g  $resourceGroupName -n $deployprefix$serviceplanSuffix --number-of-workers 2 --sku P1v2)
		#Create the function app
		echo "Creating Secure FHIR Proxy Function App ["$faname"]..."
		fahost=$(az functionapp create --name $faname --storage-account $deployprefix$storageAccountNameSuffix  --plan $deployprefix$serviceplanSuffix  --resource-group $resourceGroupName --runtime dotnet --os-type Windows --runtime-version 2 --query defaultHostName --output tsv)
		echo "Creating Service Principal for AAD Auth"
		stepresult=$(az ad sp create-for-rbac -n "https://"$fahost)
		spappid=$(echo $stepresult | jq -r '.appId')
		sptenant=$(echo $stepresult | jq -r '.tenant')
		spsecret=$(echo $stepresult | jq -r '.password')
		spreplyurls="https://"$fahost"/.auth/login/aad/callback"
		tokeniss="https://sts.windows.net/"$sptenant
		echo "Adding Sign-in User Read Permission on Graph API..."
		stepresult=$(az ad app permission add --id $spappid --api 00000002-0000-0000-c000-000000000000 --api-permissions 311a71cc-e848-46a1-bdf8-97ff7156d8e6=Scope)
		echo "Granting Admin Consent to Permission..."
		stepresult=$(az ad app permission grant --id $spappid --api 00000002-0000-0000-c000-000000000000)
		echo "Configuring reply urls for app..."
		stepresult=$(az ad app update --id $spappid --reply-urls $spreplyurls)
		echo "Adding FHIR Custom Roles to Manifest..."
		stepresult=$(az ad app update --id $spappid --app-roles @fhirroles.json)
		#Add App Settings
		#StorageAccount
		echo "Configuring Secure FHIR Proxy App ["$faname"]..."
		stepresult=$(az functionapp config appsettings set --name $faname  --resource-group $resourceGroupName --settings ADMIN_ROLE=$roleadmin READER_ROLE=$rolereader WRITER_ROLE=$rolewriter GLOBAL_ACCESS_ROLES=$roleglobal PATIENT_ACCESS_ROLES=$rolepatient DEID_ROLES=$roledeid PARTICIPANT_ACCESS_ROLES=$roleparticipant STORAGEACCT=$storageConnectionString DEIDCONFIG=$storecontainername/configuration.json FS_URL=$fsurl FS_TENANT_NAME=$fstenant FS_CLIENT_ID=$fsclientid FS_SECRET=$fssecret FS_RESOURCE=$fsaud)
		#deeployment from publish directory
		echo "Deploying Secure FHIR Proxy Function App from source repo to ["$fahost"]..."
		stepresult=$(az functionapp deployment source config-zip --name $faname --resource-group $resourceGroupName --src $deployzip)
		echo "Enabling AAD Authorization and Securing the FHIR Proxy"
		stepresult=$(az webapp auth update -g $resourceGroupName -n $faname --enabled true --action LoginWithAzureActiveDirectory --aad-allowed-token-audiences $fahost --aad-client-id $spappid --aad-client-secret $spsecret --aad-token-issuer-url $tokeniss)
		echo " "
		echo "************************************************************************************************************"
		echo "Secure FHIR Proxy Platform has successfully been deployed to group "$resourceGroupName" on "$(date)
		echo "Please note the following reference information for future use:"
		echo "Your secure fhir proxy host is: https://"$fahost
		echo "Your secure fhir proxy application id is: "$spappid
		echo "Your secure fhir proxy FHIR Server URL is: "$fsurl
		echo "Your deid config storage account name is: "$deployprefix$storageAccountNameSuffix
		echo "Your deid config storage account container is: "$storecontainername
		echo "Note: Place your deid configuration file in the deid config storage container before use."
		echo "************************************************************************************************************"
		echo " "
)
	
if [ $?  != 0 ];
 then
	echo "FHIR Proxy deployment had errors. Consider deleting resource group "$resourceGroupName" and trying again..."
fi
