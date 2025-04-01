# Career Marketplace List Management

## Summary

Provides functionality for managing the sharepoint list used for job opportunities
- Create new job opportunity
- Update existing job opportunity
- Delete existing job opportunity

## Prerequisites

The following user accounts (as reflected in the app settings) are required:

| Account           | Membership requirements                               |
| ----------------- | ----------------------------------------------------- |
| delegatedUserName | n/a                                                   |

Note that user account design can be modified to suit your environment

## Version 

![dotnet 8](https://img.shields.io/badge/net8.0-blue.svg)

## API permission

MSGraph

| API / Permissions name    | Type        | Admin consent | Justification                       |
| ------------------------- | ----------- | ------------- | ----------------------------------- |
| Sites.ReadWrite.All       | Delegated   | Yes           | Send mail as a user                 | 

Sharepoint

n/a

## App setting

| Name                     | Description                                                                       |
| ------------------------ | --------------------------------------------------------------------------------- |
| AzureWebJobsStorage      | Connection string for the storage acoount                                         |
| clientId                 | The application (client) ID of the app registration                               |
| delegatedUserName        | The account used for delegated access                                             |
| delegatedUserSecret      | The name of the key vault secret for the delegated user password                  |
| keyVaultUrl              | Address for the key vault                                                         |
| listId                   | ID of the job opportunity SharePoint list                                         |
| secretName               | Secret name used to authorize the function app                                    |
| siteId                   | ID of the site that contains the job opportunity list                             |
| tenantId                 | Id of the Azure tenant that hosts the function app                                |
| jobTypeHiddenColName     | The name of the hidden column that corresponds to the metadata column JobType     |
| programAreaHiddenColName | The name of the hidden column that corresponds to the metadata column ProgramArea |
| durationMonthId          | The Id of the month item in the Duration list. Used to calculate DurationInDays   |
| bilingualLanguageRequirementId | The Id of the Bilingual item in the LanguageRequirement list. Used for validation |
| deploymentJobTypeId	   | The Id of the Deployment term. Used for validation.							   |

## Version history

Version|Date|Comments
-------|----|--------
1.0|TBD|Initial release

## Disclaimer

**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**
