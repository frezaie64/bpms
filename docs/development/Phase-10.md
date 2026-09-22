# Phase 10 - Reporting

## Goal

Implement reporting features that allow tenant administrators to monitor workflows, tasks, forms, and user activities.

## Features

### Dashboard Reports

Provide summary statistics for the current tenant.

Display:

* Total Users
* Active Users
* Total Forms
* Published Forms
* Total Workflows
* Published Workflows
* Running Workflow Instances
* Completed Workflow Instances
* Pending Tasks
* Completed Tasks

### Workflow Reports

Support reports for:

* Workflow execution
* Workflow completion
* Workflow duration
* Workflow status

Allow filtering by:

* Workflow
* Status
* Date Range
* Started By

### Task Reports

Support reports for:

* Pending Tasks
* Completed Tasks
* Overdue Tasks
* Tasks by User
* Tasks by Role

Allow filtering by:

* User
* Role
* Priority
* Status
* Date Range

### Form Reports

Support reports for:

* Form Usage
* Form Submission Count
* Most Used Forms

### User Activity Reports

Support reports for:

* User Login History
* Workflow Participation
* Task Completion Statistics
* User Activity Summary

### Global Search

Implement a global search feature.

Users can search:

* Workflows
* Workflow Instances
* Tasks
* Forms
* Users

### Export

Allow reports to be exported as:

* Excel
* PDF

## APIs

### Reports

* GET /api/reports/dashboard
* GET /api/reports/workflows
* GET /api/reports/tasks
* GET /api/reports/forms
* GET /api/reports/users

### Search

* GET /api/search

## Tenant Rules

* Reports only include data from the current tenant.
* Search results only include data from the current tenant.
* Tenant isolation must always be enforced.

## Swagger

Document all APIs.

## Out of Scope

Do not implement:

* Charts
* BI dashboards
* Scheduled reports
* Email report delivery
* Data warehouse integration

These features can be added in future versions.

## Definition of Done

* Dashboard report works.
* Workflow reports work.
* Task reports work.
* Form reports work.
* User activity reports work.
* Global search works.
* Excel export works.
* PDF export works.
* Tenant isolation is enforced.
* Swagger is updated.
* `dotnet build` completes successfully without errors.
