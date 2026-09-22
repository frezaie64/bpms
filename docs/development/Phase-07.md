# Phase 7 - Notifications & Audit

## Goal

Implement notifications and audit logging to keep users informed about workflow events and maintain a complete history of important system activities.

## Features

### In-App Notifications

Create notifications for important events.

Examples:

* New task assigned
* Task approved
* Task rejected
* Workflow started
* Workflow completed
* Workflow cancelled

Each notification should include:

* Title
* Message
* Type
* Read status
* Created date

Users can:

* View notifications
* Mark a notification as read
* Mark all notifications as read
* Delete a notification

### Notification History

Store all notifications for future reference.

Users should be able to:

* View notification history
* Filter by read/unread
* Filter by notification type

### Audit Log

Record important system events.

Examples:

* User login
* User logout
* User created
* User updated
* Role updated
* Form created
* Form updated
* Workflow published
* Workflow started
* Workflow completed
* Task approved
* Task rejected

Each audit record should include:

* Event
* User
* Tenant
* Entity
* Entity ID
* Timestamp
* Additional details (optional)

### Search

Support searching audit logs by:

* User
* Entity
* Event type
* Date range

## APIs

### Notifications

* GET /api/notifications
* GET /api/notifications/{id}
* PUT /api/notifications/{id}/read
* PUT /api/notifications/read-all
* DELETE /api/notifications/{id}

### Audit Logs

* GET /api/audit-logs
* GET /api/audit-logs/{id}

## Tenant Rules

* Notifications belong to a tenant.
* Audit logs belong to a tenant.
* Users can only view their own notifications.
* Only administrators can view audit logs.
* Tenant isolation must be enforced.

## Swagger

Document all APIs.

## Out of Scope

Do not implement:

* Email notifications
* SMS notifications
* Push notifications
* Real-time notifications (SignalR)
* External logging systems

These features can be added in future phases.

## Definition of Done

* Notifications are created for workflow events.
* Users can manage their notifications.
* Audit logs record important system events.
* Audit logs can be searched and filtered.
* Tenant isolation is enforced.
* Swagger is updated.
* `dotnet build` completes successfully without errors.
