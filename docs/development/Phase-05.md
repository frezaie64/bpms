# Phase 5 - Workflow Runtime

## Goal

Implement the workflow engine that executes published workflow definitions. Users should be able to start workflows, execute each node, and complete the workflow.

## Features

### Workflow Execution

* Start a workflow
* Create a workflow instance
* Execute nodes in order
* Complete a workflow
* Cancel a workflow
* View workflow status

### Workflow Instance

Each workflow execution creates a new workflow instance.

Store:

* Workflow
* Workflow Version
* Current Node
* Status
* Started By
* Started Date
* Completed Date

### Workflow Variables

Support workflow variables.

Variables can store values such as:

* String
* Number
* Boolean
* Date

Variables are available throughout the workflow execution.

### User Tasks

When a User Task is reached:

* Create a task
* Assign it to a user or role
* Pause the workflow until the task is completed

Supported User Tasks:

* Approval
* Fill Form

### Completing Tasks

Users can:

* Approve
* Reject
* Submit a form

After completion, the workflow continues to the next node.

### System Tasks

Automatically execute:

* Send Email
* Send SMS

For now, use placeholder services that simulate sending emails and SMS. Real integrations can be added later.

### Decision Gateway

Evaluate simple conditions using workflow variables.

Example:

* Approved == true
* Amount > 1000

Execute the correct path based on the result.

### Parallel Gateway

Support:

* Parallel Split
* Parallel Join

The workflow continues only after all parallel branches are completed.

### Workflow History

Record:

* Started
* Node Executed
* Task Created
* Task Completed
* Workflow Completed
* Workflow Cancelled

## APIs

### Workflow Execution

* POST /api/workflow-instances
* GET /api/workflow-instances
* GET /api/workflow-instances/{id}
* POST /api/workflow-instances/{id}/cancel

### Tasks

* GET /api/tasks
* GET /api/tasks/{id}
* POST /api/tasks/{id}/approve
* POST /api/tasks/{id}/reject
* POST /api/tasks/{id}/submit-form

## Tenant Rules

* Workflow instances belong to a tenant.
* Users can only access workflow instances from their own tenant.
* Tasks are visible only to assigned users or users with the appropriate role.

## Swagger

Document all APIs.

## Out of Scope

Do not implement:

* Workflow subscriptions
* Dashboard
* Notifications
* Audit reports
* External email providers
* External SMS providers

These features will be implemented in later phases.

## Definition of Done

* Users can start workflows.
* Workflow instances are created successfully.
* User tasks pause workflow execution.
* Completing a task continues the workflow.
* System tasks execute successfully using placeholder services.
* Decision gateways work.
* Parallel gateways work.
* Workflow history is recorded.
* Tenant isolation is enforced.
* Swagger is updated.
* `dotnet build` completes successfully without errors.
