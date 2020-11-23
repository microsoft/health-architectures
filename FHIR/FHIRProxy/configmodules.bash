#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

# -e: immediately exit if any command has a non-zero exit status
# -o: prevents errors in a pipeline from being masked
# IFS new value is less likely to cause confusing bugs when looping arrays or arguments (e.g. $@)
#
# Enable/Disable Proxy Pre/Post Modules
#

usage() { echo "Usage: $0 -n <proxy name> -g <resourceGroupName> -i <subscription id>" 1>&2; exit 1; }


declare resourceGroupName=""
declare faname=""
declare preprocessors=""
declare postprocessors=""
declare defsubscriptionId=""
declare subscriptionId=""
declare stepresult=""
declare listofprocessors=""
declare enableprocessors=""
# Initialize parameters specified from command line
while getopts ":g:n:i:
" arg; do
	case "${arg}" in
		n)
			faname=${OPTARG}
			;;
		g)
			resourceGroupName=${OPTARG}
			;;
		i)
			subscriptionId=${OPTARG}
			;;
	esac
done
shift $((OPTIND-1))
if [[ -z "$resourceGroupName" ]]; then
	echo "You musty provide a resource group name."
	usage
fi
if [[ -z "$faname" ]]; then
	echo "You musty provide the name of the proxy function app."
	usage
fi
echo "Executing "$0"..."
echo "Checking Azure Authentication..."
#login to azure using your credentials
az account show 1> /dev/null

if [ $? != 0 ];
then
	az login
fi
defsubscriptionId=$(az account show --query "id" --out json | sed 's/"//g') 
if [[ -z "$subscriptionId" ]]; then
	echo "Enter your subscription ID ["$defsubscriptionId"]:"
	read subscriptionId
	if [ -z "$subscriptionId" ] ; then
		subscriptionId=$defsubscriptionId
	fi
	[[ "${subscriptionId:?}" ]]
fi
echo "Setting subscription default..."
#set the default subscription id
az account set --subscription $subscriptionId

set +e

#Check for existing RG
if [ $(az group exists --name $resourceGroupName) = false ]; then
	echo "Resource group with name" $resourceGroupName "could not be found."
	usage
fi
command -v whiptail >/dev/null 2>&1 || { echo >&2 "I require whiptail but it's not installed.  Aborting."; exit 1; }
whiptail --separate-output --noitem --checklist "Select processor modules to enable:" 15 65 7\
               FHIRProxy.postprocessors.DateSortPostProcessor off \
               FHIRProxy.postprocessors.ParticipantFilterPostProcess off  \
               FHIRProxy.postprocessors.PublishFHIREventPostProcess off \
			   FHIRProxy.postprocessors.ConsentOptOutFilter off \
               FHIRProxy.preprocessors.ProfileValidationPreProcess off\
			   FHIRProxy.preprocessors.TransformBundlePreProcess on \
			   FHIRProxy.preprocessors.EverythingPatientPreProcess on \
			   2>results
if [ $? != 0 ];
then
	echo "Cancelled..."
	exit 1;
fi
while read choice
do
  if [[ "$choice" == *".preprocessors."* ]]; then
	preprocessors+=$choice,
  fi
  if [[ "$choice" == *".postprocessors."* ]]; then
	postprocessors+=$choice,
  fi
done < results
preprocessors=$(echo $preprocessors | sed 's/.$//')
postprocessors=$(echo $postprocessors | sed 's/.$//')
echo "Configuring Secure FHIR Proxy App ["$faname"]..."
stepresult=$(az functionapp config appsettings set --name $faname  --resource-group $resourceGroupName --settings PRE_PROCESSOR_TYPES=$preprocessors POST_PROCESSOR_TYPES=$postprocessors)
if [ $? != 0 ];
then
	echo "Problem updating appsettings..."
	exit 1;
fi
echo "Pre-Processors enabled:"$preprocessors
echo "Post-Processors enabled:"$postprocessors
echo "Remember to check required configuration settings for each module!"
