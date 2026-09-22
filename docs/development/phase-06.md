# Phase 6 - Workflow Subscription & Inbox

## Goal

Allow tenant administrators to control who can start workflows and provide users with an inbox to manage their workflow tasks and workflow instances.

## Features

### Workflow Subscription

Users should not automatically have access to every workflow.

Administrators can subscribe users to published workflows.

Support:

* Subscribe users to a workflow
* Remove users from a workflow
* View workflow subscribers
* View workflows available to the current user

Only subscribed users can start a workflow.

### Workflow Catalog

Provide a list of published workflows available to the current user.

Support:

* List available workflows
* Search workflows
* Filter by category
* View workflow details

### Task Inbox

Users can view tasks assigned to them.

Support:

* My Tasks
* Pending Tasks
* Completed Tasks
* Search tasks
* Filter by status
* Filter by priority
* Sort by due date

### Workflow Instances

Users can view workflow instances they started.

Support:

* My Workflow Instances
* View workflow details
* View workflow progress
* View workflow history
* Cancel a workflow (if allowed)

### Task Actions

Users can:

* Open a task
* Approve
* Reject
* Submit a form
* Add comments

### Dashboard

Display basic information for the current user.

Show:

* Pending tasks
* Completed tasks
* Running workflows
* Completed workflows

## APIs

### Workflow Subscriptions

* GET /api/workflow-subscriptions
* POST /api/workflow-subscriptions
* DELETE /api/workflow-subscriptions/{id}
* GET /api/workflows/{id}/subscribers

### Workflow Catalog

* GET /api/workflows/available

### Inbox

* GET /api/inbox/tasks
* GET /api/inbox/workflows
* GET /api/inbox/dashboard

## Tenant Rules

* Workflow subscriptions belong to a tenant.
* Users can only subscribe users from the same tenant.
* Users can only start workflows they are subscribed to.
* Users can only access their own tasks and workflow instances unless they have administrative permissions.

## Swagger

Document all APIs.

## Out of Scope

Do not implement:

* Email notifications
* SMS notifications
* Push notifications
* Audit reports
* Analytics
* External integrations

These features will be implemented in later phases.

## Definition of Done

* Administrators can manage workflow subscriptions.
* Users can only start workflows they are subscribed to.
* Users have a task inbox.
* Users can view their workflow instances.
* Dashboard displays basic workflow information.
* Tenant isolation is enforced.
* Swagger is updated.
* `dotnet build` completes successfully without errors.
