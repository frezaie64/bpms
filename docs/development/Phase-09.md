# Phase 9 - Administration & System Configuration

## Goal

Implement administration features that allow tenant administrators to configure and manage their BPMS environment.

## Features

### Dashboard

Provide a dashboard with basic statistics.

Display:

* Total Users
* Active Users
* Total Forms
* Published Forms
* Total Workflows
* Running Workflow Instances
* Completed Workflow Instances
* Pending Tasks

### System Settings

Allow administrators to manage tenant settings.

Settings include:

* Tenant Name
* Logo
* Time Zone
* Date Format
* Language
* Default File Size Limit
* Default Page Size

### Email Configuration

Store tenant email settings.

Support:

* SMTP Host
* SMTP Port
* Username
* Password
* Sender Name
* Sender Email

These settings will be used by Email System Tasks.

### SMS Configuration

Store SMS provider settings.

Support:

* Provider Name
* API URL
* API Key
* Sender ID

These settings will be used by SMS System Tasks.

### Workflow Categories

Allow administrators to organize workflows into categories.

Support:

* Create category
* Update category
* Delete category
* List categories

### Form Categories

Allow administrators to organize forms into categories.

Support:

* Create category
* Update category
* Delete category
* List categories

### Lookup Values

Manage reusable lookup data.

Examples:

* Departments
* Branches
* Priorities
* Statuses

Support:

* Create
* Update
* Delete
* List

## APIs

### Dashboard

* GET /api/dashboard

### Settings

* GET /api/settings
* PUT /api/settings

### Workflow Categories

* GET /api/workflow-categories
* POST /api/workflow-categories
* PUT /api/workflow-categories/{id}
* DELETE /api/workflow-categories/{id}

### Form Categories

* GET /api/form-categories
* POST /api/form-categories
* PUT /api/form-categories/{id}
* DELETE /api/form-categories/{id}

### Lookup Values

* GET /api/lookups
* POST /api/lookups
* PUT /api/lookups/{id}
* DELETE /api/lookups/{id}

## Tenant Rules

* All settings belong to a tenant.
* Categories belong to a tenant.
* Lookup values belong to a tenant.
* Only administrators can access administration APIs.
* Tenant isolation must be enforced.

## Swagger

Document all APIs.

## Out of Scope

Do not implement:

* Global system administration
* Multi-language translations
* Billing
* Licensing
* Backup and restore
* Multi-region deployment

These features can be added in future phases.

## Definition of Done

* Dashboard displays tenant statistics.
* Tenant settings can be managed.
* Email configuration is stored.
* SMS configuration is stored.
* Workflow categories work.
* Form categories work.
* Lookup values can be managed.
* Tenant isolation is enforced.
* Swagger is updated.
* `dotnet build` completes successfully without errors.
