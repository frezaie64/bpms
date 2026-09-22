# System Overview

## Purpose

This project is a Business Process Management System (BPMS) that allows organizations (tenants) to create forms, design workflows, and execute business processes.

Each tenant has its own users, forms, workflows, and workflow data.

---

# User Lifecycle

1. **Sign Up** — User registers an account. Status: `Registered`. No tenant yet.
2. **Choose Plan** — User selects a plan (Basic, Pro, Enterprise) from `GET /api/plans`.
3. **Purchase** — User creates a subscription via `POST /api/subscriptions`. A pending payment is created automatically.
4. **Payment** — User completes payment via `POST /api/subscriptions/{id}/pay`. Payment status changes to `Completed`.
5. **Create Workspace** — User creates a tenant via `POST /api/tenants/`. The user becomes the Owner. Status changes to `Active`.
6. **Invite Members** — Owner invites other users by email via `POST /api/tenants/{id}/invite-by-email`.
7. **Accept Invitation** — Invited user accepts via `POST /api/invitations/accept` and becomes a Member of the tenant.

---

# Main Workflow (Inside a Tenant)

After a workspace is created, the system works in the following order:

1. Assign roles and permissions.
2. Create forms.
3. Create workflow definitions.
4. Publish workflows.
5. Users subscribe to workflows.
6. Users start workflow instances.
7. The workflow engine executes each step.
8. User tasks are completed by users.
9. System tasks are executed automatically.
10. The workflow finishes and stores its history.

---

# Main Modules

## Identity

Responsible for:

* Authentication
* Authorization
* User accounts
* Roles
* Permissions

---

## Tenancy

Responsible for:

* Tenant management
* Tenant isolation
* Shared database filtering
* Plans & Pricing (Basic, Pro, Enterprise)
* Subscriptions & Payments
* Email invitations
* Tenant settings & lookups

### Plans

Seeded on startup:

| Plan | Price | Max Users | Max Storage | Max Workflows |
|------|-------|-----------|-------------|---------------|
| Basic | $0 | 5 | 100 MB | 5 |
| Pro | $29 | 20 | 1 GB | 50 |
| Enterprise | $99 | 100 | 10 GB | 500 |

### Subscriptions & Payments

* Creating a subscription also creates a `Payment` record with status `Pending`.
* `POST /api/subscriptions/{id}/pay` simulates payment and sets status to `Completed`.
* A user must have an active subscription with completed payment to create a workspace.

### Invitations

* Owner/Admin can invite users by email via `POST /api/tenants/{id}/invite-by-email`.
* A unique token is generated and stored. The invited user accepts via `POST /api/invitations/accept`.
* Plan limits are enforced: cannot invite more users than the plan allows.

---

## Forms

Responsible for:

* Form builder
* Form versions
* Form rendering
* Form validation

Forms are independent objects and can be reused by multiple workflows.

---

## Workflow Definitions

Responsible for designing workflows.

A workflow contains:

* Nodes
* Connections
* Variables
* Forms
* Version information

Publishing a workflow makes it available for execution.

---

## Workflow Runtime

Responsible for executing workflows.

Each execution creates a Workflow Instance.

The runtime moves from node to node until the workflow reaches the End node.

---

## User Tasks

A User Task waits for a user action.

Examples:

* Fill a form
* Approve a request
* Reject a request
* Upload a file

After completion, the workflow continues.

---

## System Tasks

Executed automatically.

Examples:

* Send Email
* Send SMS
* Call REST API
* Wait for a delay
* Generate PDF

No user interaction is required.

---

## Files

Files are stored in MinIO.

Only metadata is stored in PostgreSQL.

---

# Typical Business Scenario

Example:

Employee submits a Leave Request.

↓

The Leave Request form is displayed.

↓

The employee submits the form.

↓

A workflow instance is created.

↓

Manager Approval task is assigned.

↓

Manager approves.

↓

HR Approval task is assigned.

↓

HR approves.

↓

A confirmation email is sent.

↓

Workflow ends.

---

# Data Ownership

Every tenant owns:

* Users
* Roles
* Forms
* Workflows
* Workflow Instances
* Tasks
* Files
* Invitations
* Settings
* Lookups

**Global (not tenant-scoped):**

* Plans
* Subscriptions
* Payments

Tenants cannot access data belonging to another tenant.

---

# API Endpoints (Subscription Flow)

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/auth/register` | Register a new user |
| `GET` | `/api/plans` | List all active plans |
| `POST` | `/api/subscriptions` | Create subscription + pending payment |
| `POST` | `/api/subscriptions/{id}/pay` | Simulate payment completion |
| `GET` | `/api/subscriptions/my/status` | Get current subscription status |
| `POST` | `/api/tenants/` | Create workspace (requires active subscription) |
| `POST` | `/api/tenants/{id}/invite-by-email` | Send email invitation |
| `GET` | `/api/tenants/{id}/invitations` | List tenant invitations |
| `POST` | `/api/invitations/accept` | Accept invitation by token |

---

# Development Rules

* Build one phase at a time.
* Do not implement features from future phases.
* Keep modules independent.
* Reuse existing components whenever possible.
* Keep the implementation simple and maintainable.
