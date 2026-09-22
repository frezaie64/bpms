# Phase 4 - Workflow Designer

## Goal

Implement a visual workflow designer that allows tenant users to create, edit, version, and publish workflow definitions. Workflows are stored as JSON and will be executed in the next phase.

## Features

### Workflow Management

* Create a workflow
* Update a workflow
* Delete a workflow (soft delete)
* Publish a workflow
* Archive a workflow
* Duplicate a workflow
* List workflows
* View workflow details

### Workflow Designer

Users can visually build workflows by adding nodes and connecting them.

Each workflow must have:

* One Start node
* One End node
* One or more connected nodes

Store the workflow definition as JSON.

### Supported Nodes

#### Basic

* Start
* End

#### User Tasks

* Approval
* Fill Form

#### System Tasks

* Send Email
* Send SMS

#### Gateways

* Decision (Yes / No)
* Parallel Split
* Parallel Join

### Node Properties

Each node should support:

* Name
* Description
* Node Type
* Position (X, Y)
* Configuration
* Connections

### User Task Configuration

Support:

* Task Name
* Description
* Assigned User
* Assigned Role
* Form (optional)
* Due Days

### Email Task

Support:

* Recipient
* Subject
* Message

Only store the configuration in this phase. Emails are not sent until the Workflow Runtime phase.

### SMS Task

Support:

* Phone Number
* Message

Only store the configuration in this phase.

### Decision Gateway

Support simple conditions.

Examples:

* Approved == true
* Amount > 1000
* Status == "Completed"

Only save the condition definition. Evaluation will be implemented in the runtime phase.

### Workflow Versioning

* Editing a published workflow creates a new version.
* Existing versions remain unchanged.

### Workflow Status

Support:

* Draft
* Published
* Archived

## Data Storage

Store workflow definitions as JSON.

Each workflow should include:

* Name
* Description
* Version
* Status
* JSON Definition

## APIs

### Workflows

* GET /api/workflows
* GET /api/workflows/{id}
* POST /api/workflows
* PUT /api/workflows/{id}
* DELETE /api/workflows/{id}
* POST /api/workflows/{id}/publish
* POST /api/workflows/{id}/archive
* POST /api/workflows/{id}/duplicate
* GET /api/workflows/{id}/definition

## Tenant Rules

* Workflows belong to a tenant.
* Users can only access workflows from their own tenant.

## Swagger

Document all APIs.

## Out of Scope

Do not implement:

* Workflow execution
* Workflow instances
* User task assignment
* Sending emails
* Sending SMS
* API calls
* Workflow history
* Workflow subscriptions

These features will be implemented in later phases.

## Definition of Done

* Users can create workflows.
* Users can visually design workflows.
* Workflow definitions are stored as JSON.
* Workflow versioning works.
* Users can publish workflows.
* Users can archive workflows.
* Tenant isolation is enforced.
* Swagger is updated.
* `dotnet build` completes successfully without errors.
