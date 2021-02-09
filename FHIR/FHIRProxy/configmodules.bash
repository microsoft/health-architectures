#!/usr/bin/env bash
set -euo pipefail

# -e: immediately exit if any  command has a non-zero exit status
# -u: treat references to unset variables as errors
# -o: prevent errors in a pipeline from being masked

declare resourceGroupName=""
declare faname=""
declare preprocessors=""
declare postprocessors=""
declare defsubscriptionId=""
declare subscriptionId=""
declare stepresult=""
declare -a selectedModules

declare -A modules
modules[FHIRProxy.postprocessors.DateSortPostProcessor]="Date Sort Post Processor Module"
modules[FHIRProxy.postprocessors.ParticipantFilterPostProcess]="Participant Filter Post Processor Module"
modules[FHIRProxy.postprocessors.PublishFHIREventPostProcess]="Publish FHIR Events Post Processor Module"
modules[FHIRProxy.postprocessors.ConsentOptOutFilter]="Consent Opt-Out Filter Module"
modules[FHIRProxy.preprocessors.ProfileValidationPreProcess]="Profile Validation Pre-Processor Module"
modules[FHIRProxy.preprocessors.TransformBundlePreProcess]="Transform Bundle Pre-Processor Module"
modules[FHIRProxy.preprocessors.EverythingPatientPreProcess]="Everything Patient Pre-Processor Module"

usage() { echo "Usage: $0 -n <proxy name> -g <resourceGroupName> -i <subscription id>" 1>&2; exit 1; }

function askYesNo {
    PROMPT=$1
    DEFAULT=$2
    if [ "$DEFAULT" = true ]; then
        OPTIONS="[Y/n]"
        DEFAULT="y"
    else
        OPTIONS="[y/N]"
        DEFAULT="n"
    fi
    read -p "$PROMPT $OPTIONS " -n 1 -s -r INPUT
    INPUT=${INPUT:-${DEFAULT}}
    echo ${INPUT}
    if [[ "$INPUT" =~ ^[yY]$ ]]; then
        ANSWER=true
    else
        ANSWER=false
    fi
}

echo "configmodules.sh "
echo "This script with enable/disable FHIR Proxy modules"
echo ""
read -es -p "Press ENTER to continue." TRAP_ENTER_KEY < /dev/tty
echo ""
echo ""

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

echo ""
for key in "${!modules[@]}";
do
    echo "${modules[$key]}"
    askYesNo "Install this module?" true
    RESPONSE=$ANSWER
    if [ "$RESPONSE" = true ]; then
        selectedModules+=("$key")
    fi

    echo ""
    echo ""
done

for i in ${!selectedModules[@]}; do 
    if [[ "${selectedModules[i]}" ]]; then
		if [[ "${selectedModules[i]}" == *".preprocessors."* ]]; then
			preprocessors+="${selectedModules[i]}",
		fi
		if [[ "${selectedModules[i]}" == *".postprocessors."* ]]; then
			postprocessors+="${selectedModules[i]}",
		fi
	fi
done

preprocessors="${preprocessors%?}"
postprocessors="${postprocessors%?}"

echo "Configuring Secure FHIR Proxy ["$faname"] modules..."
stepresult=$(az functionapp config appsettings set --name $faname --resource-group $resourceGroupName --settings FP-PRE-PROCESSOR-TYPES=$preprocessors FP-POST-PROCESSOR-TYPES=$postprocessors)
if [ $? != 0 ];
then
	echo "Encountered problem updating FHIR Proxy configuration..."
	exit 1;
fi

echo "Pre-Processors enabled:"$preprocessors
echo "Post-Processors enabled:"$postprocessors
echo ""
echo ""

echo "Some modules may require additional configuration."
echo "Navigate to the FHIR Proxy configuration documentation located here: https://github.com/microsoft/health-architectures/tree/master/FHIR/FHIRProxy#configuration"
