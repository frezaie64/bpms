# BPMS Development Roadmap

This roadmap defines the implementation phases of the BPMS project. Each phase builds on the previous one. Complete one phase before starting the next.

---

# Phase 1 - Foundation

## Goal

Build the project foundation.

## Features

* Modular Monolith architecture
* ASP.NET Core Web API
* PostgreSQL
* MinIO configuration
* Docker Compose
* JWT Authentication
* Tenant Management
* Shared Database / Shared Schema multi-tenancy
* Global Query Filters
* Swagger
* Serilog
* Health Checks

---

# Phase 2 - User & Role Management

## Goal

Implement user management and authorization.

## Features

* User management
* Role management
* Permission-based authorization
* User profile
* Change password
* User activation/deactivation
* Soft delete
* Tenant isolation

---

# Phase 3 - Form Builder

## Goal

Allow users to create dynamic forms.

## Features

* Create forms
* Edit forms
* Delete forms
* Form versioning
* Form categories
* Form validation rules
* Form preview
* Form publishing
* JSON form definition
* Dynamic form rendering

### Supported Fields

* Text
* Text Area
* Number
* Email
* Phone
* Date
* Time
* DateTime
* Checkbox
* Radio Button
* Dropdown
* Multi Select
* File Upload
* Image Upload
* Signature
* Hidden Field

---

# Phase 4 - File Management

## Goal

Manage files using MinIO.

## Features

* Upload files
* Download files
* Delete files
* File metadata
* File versioning
* Image preview
* File access permissions

---

# Phase 5 - Workflow Designer

## Goal

Allow users to visually design workflows.

## Features

* Create workflows
* Edit workflows
* Delete workflows
* Publish workflows
* Workflow versioning
* Visual workflow designer
* JSON workflow definition

### Workflow Nodes

* Start
* End
* User Task
* Approval Task
* System Task
* Email
* SMS
* Webhook
* Delay
* Exclusive Gateway
* Parallel Gateway

---

# Phase 6 - Workflow Runtime

## Goal

Execute workflow definitions.

## Features

* Start workflow
* Resume workflow
* Complete workflow
* Cancel workflow
* Workflow variables
* Workflow history
* Workflow state management
* Workflow version support

---

# Phase 7 - User Tasks

## Goal

Manage human tasks created by workflows.

## Features

* Task inbox
* My tasks
* Assign task
* Reassign task
* Complete task
* Reject task
* Task comments
* Task attachments
* Due dates
* Task priorities

---

# Phase 8 - System Tasks

## Goal

Execute automated workflow actions.

## Features

* Send email
* Send SMS
* Call REST API
* Execute webhook
* Delay execution
* Update workflow variables
* Generate PDF

---

# Phase 9 - Workflow Execution

## Goal

Allow tenant users to execute published workflows.

## Features

* Publish workflow
* Subscribe users to workflows
* Start workflow
* Fill workflow forms
* Workflow inbox
* Workflow history
* Workflow search

---

# Phase 10 - Notifications

## Goal

Notify users about workflow events.

## Features

* In-app notifications
* Email notifications
* SMS notifications
* Notification templates
* Notification history

---

# Phase 11 - Administration

## Goal

Provide administration tools.

## Features

* Dashboard
* System settings
* Tenant settings
* Audit logs
* Workflow statistics
* User activity
* Storage management

---

# Phase 12 - Finalization

## Goal

Prepare the project for production.

## Features

* Performance optimization
* Security review
* Automated testing
* API documentation
* Deployment configuration
* Docker optimization
* Backup strategy
* Production readiness
* Final bug fixes

---

# Development Guidelines

* Complete one phase before starting the next.
* Do not implement features from future phases.
* Keep the architecture modular.
* Ensure the project builds successfully after each phase.
* Keep APIs documented in Swagger.
* Follow the project structure defined in `Project-Structure.md`.
* Prefer simple, maintainable solutions over unnecessary complexity.
