# extractor

## Run this script (per environment)
```powershell

$cloudenv="dev"; `

$workload="extractor"; `
$location="westeurope"; `
$locationShort="weu"; `

clear; `
$subId=az account list --all --query "[?ends_with(name, 'VS Pro')].{id:id}" -o tsv; `
az account set -s $subId; `
$scmSpName="github_$workload"; `
$scmSpId=az ad sp list --disp $scmSpName --only-show-errors --query '[].id' -o tsv; `
$scmCreds=""; `
if ( !$scmSpId ) { `
  $credsMap="{clientId:appId, clientSecret:password, subscriptionId: '$subId', tenantId:tenant}"; `
  $scmCreds=az ad sp create-for-rbac --name $scmSpName --only-show-errors --query $credsMap; `
  $scmSpId=az ad sp list --disp $scmSpName --only-show-errors --query '[].id' -o tsv; `
} `

$rgName="$cloudenv-rg-$workload-$locationShort"; `
$rgId=az group create -l $location -n $rgName --tags workload=$workload env=$cloudenv --query id -o tsv; `
$acrId=az acr show -n "$($cloudenv)acrsharedweu" -g "$($cloudenv)-rg-shared-weu" --query id -o tsv
$scmRgContrib=az role assignment create --assignee-object-id $scmSpId --assignee-principal-type ServicePrincipal --role contributor --scope $rgId --only-show-errors; `
$scmAcrPuller=az role assignment create --assignee-object-id $scmSpId --assignee-principal-type ServicePrincipal --role acrpull --scope $acrId --only-show-errors; `
$engAdId=az ad group create --display-name engineers --mail-nickname engineers --only-show-errors --query id -o tsv; `
$appiConnection=az resource show -g "$($cloudenv)-rg-shared-weu" -n "$($cloudenv)-appi-shared-weu" --resource-type "microsoft.insights/components" --query properties.ConnectionString -o tsv; `

echo "--------------------------------------------------------------------------------"; `
echo "AZURE_CI_PREFIX:  dev"; `
echo "AZURE_WORKLOAD:   $workload"; `
echo "AZURE_RG_LOC:     $locationShort"; `
echo "AZURE_ENG_ID:     $engAdId"; `
echo "AZURE_SUB_ID:     $subId"; `
echo "AZURE_SCM_ID:     $scmSpId"; `
echo "AZURE_INSIGHTS:   $appiConnection"; `
if ( $scmCreds ) { echo "AZURE_SCM_CREDS:" $scmCreds; } `
echo "--------------------------------------------------------------------------------";

```
