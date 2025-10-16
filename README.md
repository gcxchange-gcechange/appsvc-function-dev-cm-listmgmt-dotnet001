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
| durationYearId           | The Id of the year(s) item in the Duration list. Used to calculate DurationInDays    |
| durationMonthId          | The Id of the month(s) item in the Duration list. Used to calculate DurationInDays   |
| durationWeekId           | The Id of the week(s) item in the Duration list. Used to calculate DurationInDays   |
| bilingualLanguageRequirementId | The Id of the Bilingual item in the LanguageRequirement list. Used for validation |
| deploymentJobTypeId	   | The Id of the Deployment term. Used for validation.							   |
| applicationDeadlineDate_Alias	   | The name of the ApplicationDeadlineDate column							   |
| approvedStaffing_Alias	   | The name of the ApprovedStaffing column							   |
| cityId_Alias	   | The name of the City column in the format `{ColumnName}LookupId`						   |
| classificationCodeId_Alias	   | The name of the ClassificationCode column in the format `{ColumnName}LookupId`	|
| classificationLevelId_Alias	   | The name of the ClassificationLevel column in the format `{ColumnName}LookupId`|
| contactEmail_Alias	   | The name of the ContactEmail column					   |
| contactName_Alias	   | The name of the ContactName column					   |
| contactObjectId_Alias	   | The name of the ContactObjectId column					   |
| departmentId_Alias	   | The name of the Department column in the format `{ColumnName}LookupId`	|
| durationId_Alias	   | The name of the Duration column in the format `{ColumnName}LookupId`	|
| durationInDays_Alias	   | The name of the DurationInDays column					   |
| durationQuantity_Alias	   | The name of the DurationQuantity column					   |
| jobDescriptionEn_Alias	   | The name of the JobDescriptionEn column					   |
| jobDescriptionFr_Alias	   | The name of the JobDescriptionFr column					   |
| jobTitleEn_Alias	   | The name of the JobTitleEn column					   |
| jobTitleFr_Alias	   | The name of the JobTitleFr column					   |
| jobType_Alias	   | The name of the JobType column					   |
| languageComprehension_Alias	   | The name of the LanguageComprehension column					   |
| languageRequirementId_Alias	   | The name of the LanguageRequirement column in the format `{ColumnName}LookupId` |
| numberOfOpportunities_Alias	   | The name of the NumberOfOpportunities column					   |
| programArea_Alias	   | The name of the ProgramArea column					   |
| securityClearanceId_Alias	   | The name of the SecurityClearance column in the format `{ColumnName}LookupId` |
| skillIds_Alias	   | The name of the Skills column in the format `{ColumnName}LookupId` |
| workArrangementId_Alias	   | The name of the WorkArrangement column in the format `{ColumnName}LookupId` |
| workScheduleId_Alias	   | The name of the WorkSchedule column in the format `{ColumnName}LookupId` |

## Version history

Version|Date|Comments
-------|----|--------
1.0|TBD|Initial release

## Disclaimer

**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**
