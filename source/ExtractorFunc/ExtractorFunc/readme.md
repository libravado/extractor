## WNS Document Extractor "2.0"
___
### Intro
This is a standalone function that provides claim documents for WNS learning.

**NB**: *There was a previous iteration ("Extractor 1.0") under Pawtal.DocumentExporter solution. The "1.0" version is obsoleted by the new function.*
___
### Developer Quickstart
- Ensure you have the latest VS installed (2022+)
- Install Azure Storage Explorer (dont need local admin rights if installing for your user only)
- Start debugging the function app (after clicking "Set as startup project" on ExtractorFunc project in VS)
    - This should start "Azurite" service (see Output > Service Dependencies in VS to confirm)
    - Open Azure Storage Explorer and under *Local & Attached > Storage Accounts > Emulator*, create a container called `wns-data-extract-trigger` (you should not have to do this step again, unless you remove it)
- With the app debugging, (and breakpoint inside function method, if desired), drag and drop a trigger payload file to this new container (see [Trigger Payload](#trigger-payload) for examples)
- Once a run has completed, look for an "export" container alongside your trigger container in Storage Explorer and there should be a report of the run (inside "runs" folder)

All going well, the run output should show how many document blob uris were returned from Pawtal and list out the failing uris that failed to copy.
(*Unless you've added source files in your Storage Explorer then every attempt should fail!*)

So in order to test some successful paths, you will need to upload blobs within your Storage Explorer / Emulator. To simulate a match, take a failing uri from a previous run, (eg. `https://animalfriendsdevdiag.blob.core.windows.net/pawtal-invoice/claim131447/placeholder.png`)
and disregard the account uri bit to get the format `CONTAINER/BLOB_PATH`, e.g. in this case, this is `pawtal-invoice/claim131447/placeholder.png`

So to simulate this locally, create a new container in your emulator called `pawtal-invoice`, create a folder therein called `claim131447` and upload a file called `placeholder.png` to this folder.
On your next run, you should get one less failed uri :relaxed:
___
### Technical Overview
There are several cloud resources involved:
- (new) Azure function
- (new) Storage blob container (for triggering the function) Must be named `wns-data-extract-trigger`
- (new) RBAC role assignments or similar (so that the function can access related resources)
- (existing) Storage account for the new container for trigger files
- (existing) Pawtal database (the single source of blob uris, by claim)
- (existing) Storage account + containers for Pawtal document blobs
- (existing) Storage account + container for extract data
- (existing) Function hosting plan / supporting infra
- (existing) App Insights component
#### Process
1. Create a trigger file (csv, json) with desired config
1. Upload trigger file to `wns-data-extract-trigger`
1. Function is triggered...
    1. Parses config from trigger file
    1. Queries Pawtal Db according to config for list of claim documents metadata
    1. Iterates results, copying blobs (if found and not already copied)
    1. Dumps out report json file under "runs/`timestamp`.results.json"
1. Documents and results json are vieweable in data extract storage account

#### Function Configuration
Note the following (present in Configuration section for the hosted function app):

|Key|Description|
|-|-|
|`TriggerBlobStorage__accountName`|The name of the storage account for triggering. The function must be able to read blobs in this account.|
|`SourceDocsStorageAccountName`|The name of the storage account where Pawtal docs live. The function must be able to read blobs in this account.|
|`ExportBlobStorageAccountName`|The name of the storage account for exporting data. The function must be able to write blobs to this account.|
|`ExportBlobContainerName`|The name of the container within the "Export Blob Storage Account" to which to export data.|
|`ConnectionStrings__SourceDb`|Connection to Pawtal db.|
|`APPLICATIONINSIGHTS_CONNECTION_STRING`|Connection to App Insights.|

**NB**: *The storage accounts might end be being the same (e.g. trigger and export could theoretically be under the same account). But this gives us the ability to separate them if desired.*

#### Trigger Payload
The following properties can be supplied via trigger payload:

|Name|Type|Description|
|-|-|-|
|**ClaimsCreatedFrom**|DateTime|Min value of claim created range.|
|**ClaimsCreatedTo**|DateTime|Max value of claim created range.|
|**PracticeIds**|int[]|A list of practice ids. If omitted, _all_ practices are considered.|

Both JSON and CSV files are supported. The triggering blob **MUST** have one of these file extensions **AND** be formatted appropriately in accordance with the extension.

##### CSV Example: (e.g. _data-extract-trigger.csv_)

```json
ClaimsCreatedFrom, ClaimsCreatedTo, PracticeIds
2021-01-01, 2022-01-01, "2300, 5883, 44, 10423"
```
##### JSON Example: (e.g. _data-extract-trigger.json_)
```json
{
  "ClaimsCreatedFrom": "2021-01-01",
  "ClaimsCreatedTo": "2022-01-01",
  "PracticeIds": [
    2300,
    5883,
    44,
    10423
  ]
}
```
___