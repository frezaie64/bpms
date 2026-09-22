# BPMS API Documentation

A modular **Business Process Management System** built with ASP.NET Core Minimal APIs.

**Base URL:** `https://localhost:<port>/api`
**Swagger UI:** `https://localhost:<port>/swagger` (Development only)

---

## Table of Contents

1. [Project Structure](#project-structure)
2. [Tech Stack](#tech-stack)
3. [Authentication](#authentication)
4. [Tenant Context](#tenant-context)
5. [Error Handling](#error-handling)
6. [User Lifecycle](#user-lifecycle)
7. [API Reference](#api-reference)
   - [Auth](#auth)
   - [Users](#users)
   - [Roles](#roles)
   - [Plans](#plans)
   - [Subscriptions & Payments](#subscriptions--payments)
   - [Tenants](#tenants)
   - [Invitations](#invitations)
   - [Settings & Lookups](#settings--lookups)
   - [Forms](#forms)
   - [Form Categories](#form-categories)
   - [Workflows](#workflows)
   - [Workflow Categories](#workflow-categories)
   - [Workflow Subscriptions](#workflow-subscriptions)
   - [Workflow Instances](#workflow-instances)
   - [Tasks](#tasks)
   - [Inbox](#inbox)
   - [Admin Dashboard](#admin-dashboard)
   - [Notifications](#notifications)
   - [Audit Logs](#audit-logs)
   - [Files](#files)
   - [Reports](#reports)
   - [Search](#search)
8. [Schemas](#schemas)
9. [Permissions Reference](#permissions-reference)
10. [Development](#development)

---

## Project Structure

```
BPMS/
├── BPMS.slnx
├── docker-compose.yml
├── docs/
│   ├── adr/
│   ├── Roadmap.md
│   ├── System-Overview.md
│   └── Phase-*.md
└── src/
    ├── BPMS.Api/                          # ASP.NET Core host
    │   ├── Program.cs
    │   ├── appsettings.json
    │   └── Properties/
    │       └── launchSettings.json
    ├── BPMS.Shared/                       # Shared kernel
    │   ├── Abstractions/                  # BaseEntity, IEntity, IAuditableEntity, ITenantEntity
    │   ├── Authorization/                 # Permissions, AuthorizationExtensions
    │   ├── Extensions/                    # JwtExtensions, TenantMiddleware
    │   ├── Migrations/                    # EF Core migrations
    │   ├── Persistence/                   # BpmsDbContext
    │   └── Services/                      # ITenantService, TenantService
    ├── BPMS.Modules.Identity/             # Auth, users, roles, permissions
    │   ├── Endpoints/                     # AuthEndpoints, UserEndpoints, RoleEndpoints
    │   ├── Entities/                      # User, Role, Permission, UserRole, RolePermission
    │   ├── Models/                        # DTOs / request-response models
    │   ├── Persistence/                   # EF Core entity configurations
    │   └── Services/                      # UserService, RoleService, PasswordPolicy
    ├── BPMS.Modules.Tenancy/              # Multi-tenancy, plans, subscriptions, payments, invitations
    │   ├── Endpoints/                     # TenantEndpoints, SubscriptionEndpoints, PlanEndpoints, InvitationEndpoints
    │   ├── Entities/                      # Tenant, TenantMember, Plan, Subscription, Payment, Invitation
    │   ├── Models/                        # DTOs / request-response models
    │   ├── Persistence/                   # EF Core entity configurations
    │   └── Services/                      # (via TenancyModule)
    ├── BPMS.Modules.Forms/                # Form builder
    ├── BPMS.Modules.WorkflowDefinitions/  # Workflow designer
    ├── BPMS.Modules.WorkflowRuntime/      # Workflow execution engine
    ├── BPMS.Modules.Notifications/        # Notifications and audit trails
    ├── BPMS.Modules.FileManagement/       # File upload/download via MinIO
    └── BPMS.Modules.Reports/              # Reports and global search
```

---

## Tech Stack

| Component | Technology |
|---|---|
| Runtime | .NET 10 |
| API Style | Minimal APIs |
| Database | PostgreSQL (Npgsql / EF Core) |
| File Storage | MinIO (S3-compatible) |
| Auth | JWT Bearer + BCrypt |
| Logging | Serilog |
| Docs | Swashbuckle (Swagger) |

### External Services (docker-compose.yml)

| Service | Port |
|---|---|
| PostgreSQL | `localhost:5432` |
| MinIO API | `localhost:9000` |
| MinIO Console | `localhost:9001` |

---

## Authentication

The API uses **JWT Bearer** tokens. Obtain a token by logging in, then include it in all subsequent requests.

### Obtain a Token

**POST** `/api/auth/login`

```json
{
  "email": "user@example.com",
  "password": "your-password"
}
```

**Response:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJl...",
  "expiresAt": "2026-07-17T12:00:00Z"
}
```

### Use the Token

Include the `Authorization` header on every authenticated request:

```
Authorization: Bearer <accessToken>
```

### Refresh an Expired Token

**POST** `/api/auth/refresh`

```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJl..."
}
```

Returns a new `accessToken`, `refreshToken`, and `expiresAt`.

---

## Tenant Context

The API supports **multi-tenancy**. Pass the tenant identifier in the header:

```
X-Tenant-Id: <tenant-guid>
```

The tenant ID is also resolved from the `tenantId` claim in the JWT.

---

## Error Handling

| Status Code | Meaning |
|-------------|---------|
| `200 OK` | Success |
| `201 Created` | Resource created |
| `204 No Content` | Success (no body, typically for DELETE) |
| `400 Bad Request` | Invalid input / validation error |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Authenticated but lacks required permission |
| `404 Not Found` | Resource does not exist |
| `500 Internal Server Error` | Unexpected server error |

---

## User Lifecycle

1. **Sign Up** — User registers an account. Status: `Registered`. No tenant yet.
2. **Choose Plan** — User selects a plan (Basic, Pro, Enterprise) from `GET /api/plans`.
3. **Purchase** — User creates a subscription via `POST /api/subscriptions`. A pending payment is created automatically.
4. **Payment** — User completes payment via `POST /api/subscriptions/{id}/pay`. Payment status changes to `Completed`.
5. **Create Workspace** — User creates a tenant via `POST /api/tenants/`. The user becomes the Owner. Status changes to `Active`.
6. **Invite Members** — Owner invites other users by email via `POST /api/tenants/{id}/invite-by-email`.
7. **Accept Invitation** — Invited user accepts via `POST /api/invitations/accept` and becomes a Member of the tenant.

---

## API Reference

### Auth

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/auth/register` | Register a new user | No |
| `POST` | `/api/auth/login` | Log in and receive a JWT | No |
| `POST` | `/api/auth/refresh` | Refresh an access token | No |

#### Register

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "securePassword123",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:** `AuthResponse` (201 Created)

#### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "securePassword123"
}
```

**Response:** `AuthResponse`

#### Refresh Token

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJl..."
}
```

**Response:** `AuthResponse`

---

### Users

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| `GET` | `/api/users/me` | Get current user profile | Authenticated |
| `PUT` | `/api/users/me` | Update current user profile | Authenticated |
| `POST` | `/api/users/change-password` | Change current user password | Authenticated |
| `GET` | `/api/users/` | List all users | `Users.View` |
| `GET` | `/api/users/{id}` | Get user by ID | `Users.View` |
| `POST` | `/api/users/` | Create a new user | `Users.Create` |
| `PUT` | `/api/users/{id}` | Update a user | `Users.Update` |
| `PUT` | `/api/users/{id}/status` | Activate/deactivate a user | `Users.Update` |
| `DELETE` | `/api/users/{id}` | Delete a user | `Users.Delete` |

#### Get Current Profile

```http
GET /api/users/me
Authorization: Bearer <token>
```

**Response:** `UserProfileResponse`

#### Create User

```http
POST /api/users/
Authorization: Bearer <token>
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "securePassword123",
  "firstName": "Jane",
  "lastName": "Smith"
}
```

**Response:** 201 Created — `UserResponse`

#### Change Password

```http
POST /api/users/change-password
Authorization: Bearer <token>
Content-Type: application/json

{
  "currentPassword": "oldPassword123",
  "newPassword": "newPassword456"
}
```

#### Set User Status

```http
PUT /api/users/{id}/status
Authorization: Bearer <token>
Content-Type: application/json

{
  "isActive": false
}
```

---

### Roles

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| `GET` | `/api/roles/` | List all roles | `Roles.View` |
| `GET` | `/api/roles/{id}` | Get role by ID | `Roles.View` |
| `POST` | `/api/roles/` | Create a role | `Roles.Create` |
| `PUT` | `/api/roles/{id}` | Update a role | `Roles.Update` |
| `PUT` | `/api/roles/{id}/permissions` | Replace a role's permissions | `Roles.Update` |
| `DELETE` | `/api/roles/{id}` | Delete a role | `Roles.Delete` |
| `POST` | `/api/roles/{id}/users` | Assign users to role | `Roles.Update` |
| `DELETE` | `/api/roles/{id}/users/{userId}` | Remove user from role | `Roles.Update` |

> Creating a workspace automatically provisions an `Owner` role with all permissions and assigns it to the tenant owner. Other users must be assigned roles by the owner before they gain permissions. Permission claims are minted into the JWT at login — re-login (or `/api/auth/refresh`) after role changes for them to take effect.

#### Create Role

```http
POST /api/roles/
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Manager"
}
```

#### Set Role Permissions

Replaces the role's permission set with the given list. Unknown permission names are rejected.

```http
PUT /api/roles/{roleId}/permissions
Authorization: Bearer <token>
Content-Type: application/json

{
  "permissions": ["Users.View", "Roles.View"]
}
```

#### Assign Users to Role

```http
POST /api/roles/{roleId}/users
Authorization: Bearer <token>
Content-Type: application/json

{
  "userIds": ["11111111-1111-1111-1111-111111111111"]
}
```

---

### Plans

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/plans/` | List all active plans | No |
| `POST` | `/api/plans/` | Create a plan | No |

#### Seeded Plans

| Plan | Price | Max Users | Max Storage | Max Workflows |
|------|-------|-----------|-------------|---------------|
| Basic | $0 | 5 | 100 MB | 5 |
| Pro | $29 | 20 | 1 GB | 50 |
| Enterprise | $99 | 100 | 10 GB | 500 |

#### List Plans

```http
GET /api/plans/
```

**Response:** `Plan[]`

```json
[
  {
    "id": "guid",
    "name": "Basic",
    "slug": "basic",
    "description": "For small teams getting started",
    "price": 0,
    "maxUsers": 5,
    "maxStorageMb": 100,
    "maxWorkflows": 5,
    "isActive": true
  }
]
```

---

### Subscriptions & Payments

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/subscriptions/` | Create a subscription (+ pending payment) | Authenticated |
| `GET` | `/api/subscriptions/my` | Get current user's subscriptions | Authenticated |
| `GET` | `/api/subscriptions/my/status` | Get current user's active subscription status | Authenticated |
| `POST` | `/api/subscriptions/{id}/pay` | Simulate payment completion | Authenticated |

#### Create Subscription

```http
POST /api/subscriptions/
Authorization: Bearer <token>
Content-Type: application/json

{
  "planId": "33333333-3333-3333-3333-333333333333"
}
```

**Response:** `SubscriptionResponse`

```json
{
  "id": "guid",
  "userId": "guid",
  "planId": "guid",
  "status": "Active",
  "startDate": "2026-07-17T00:00:00Z",
  "endDate": "2027-07-17T00:00:00Z"
}
```

#### Get Active Subscription Status

```http
GET /api/subscriptions/my/status
Authorization: Bearer <token>
```

**Response:** `SubscriptionStatusResponse`

```json
{
  "hasActiveSubscription": true,
  "subscriptionId": "guid",
  "planId": "guid",
  "planName": "Pro",
  "endDate": "2027-07-17T00:00:00Z"
}
```

#### Simulate Payment

```http
POST /api/subscriptions/{subscriptionId}/pay
Authorization: Bearer <token>
```

**Response:** `PayPaymentResponse`

```json
{
  "id": "guid",
  "status": "Completed",
  "transactionId": "SIM-abc123...",
  "amount": 29.00
}
```

---

### Tenants

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/tenants/` | Create a workspace (requires active subscription) | Authenticated |
| `GET` | `/api/tenants/` | List all tenants | Authenticated |
| `GET` | `/api/tenants/my` | Get current user's tenants | Authenticated |
| `GET` | `/api/tenants/{id}` | Get tenant by ID | Authenticated |
| `POST` | `/api/tenants/{id}/invite` | Add a member by UserId | Authenticated |
| `POST` | `/api/tenants/{id}/invite-by-email` | Send email invitation | Authenticated |
| `GET` | `/api/tenants/{id}/members` | List tenant members | Authenticated |
| `GET` | `/api/tenants/{id}/invitations` | List tenant invitations | Authenticated |

#### Create Workspace

```http
POST /api/tenants/
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Acme Corp",
  "slug": "acme-corp"
}
```

**Response:** 201 Created

```json
{
  "id": "guid",
  "name": "Acme Corp",
  "slug": "acme-corp",
  "status": "Active"
}
```

> **Note:** The user must have an active subscription with completed payment. The subscription and plan are resolved automatically from the user's account.

#### Invite Member by UserId

```http
POST /api/tenants/{tenantId}/invite
Authorization: Bearer <token>
Content-Type: application/json

{
  "userId": "11111111-1111-1111-1111-111111111111",
  "role": "Member"
}
```

**Response:** `TenantMemberResponse`

#### Invite by Email

```http
POST /api/tenants/{tenantId}/invite-by-email
Authorization: Bearer <token>
Content-Type: application/json

{
  "email": "colleague@example.com",
  "role": "Member"
}
```

**Response:** `InvitationResponse`

```json
{
  "id": "guid",
  "email": "colleague@example.com",
  "role": "Member",
  "status": "Pending",
  "expiresAt": "2026-07-24T00:00:00Z",
  "acceptedAt": null,
  "createdAt": "2026-07-17T00:00:00Z"
}
```

> **Note:** Only Owner or Admin can send invitations. Plan user limits are enforced.

---

### Invitations

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/invitations/accept` | Accept an invitation by token | Authenticated |

#### Accept Invitation

```http
POST /api/invitations/accept
Authorization: Bearer <token>
Content-Type: application/json

{
  "token": "base64token..."
}
```

**Response:** `InvitationAcceptResponse`

```json
{
  "tenantId": "guid",
  "userId": "guid",
  "role": "Member"
}
```

---

### Settings & Lookups

#### Settings

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| `GET` | `/api/settings/` | Get tenant settings | `Administration.ManageSettings` |
| `PUT` | `/api/settings/` | Update tenant settings | `Administration.ManageSettings` |

```http
PUT /api/settings/
Authorization: Bearer <token>
X-Tenant-Id: <tenant-guid>
Content-Type: application/json

{
  "tenantName": "Acme Corp",
  "timeZone": "UTC",
  "dateFormat": "yyyy-MM-dd",
  "language": "en",
  "defaultFileSizeLimit": 10,
  "defaultPageSize": 25,
  "smtpHost": "smtp.example.com",
  "smtpPort": 587,
  "smtpUsername": "noreply@example.com",
  "senderName": "BPMS",
  "senderEmail": "noreply@example.com"
}
```

#### Lookups

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| `GET` | `/api/lookups/` | Get lookups (optional `?group=`) | `Administration.ManageLookups` |
| `POST` | `/api/lookups/` | Create a lookup value | `Administration.ManageLookups` |
| `PUT` | `/api/lookups/{id}` | Update a lookup value | `Administration.ManageLookups` |
| `DELETE` | `/api/lookups/{id}` | Delete a lookup value | `Administration.ManageLookups` |

```http
POST /api/lookups/
Authorization: Bearer <token>
Content-Type: application/json

{
  "group": "Priority",
  "name": "High",
  "value": "high"
}
```

---

### Forms

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/forms/` | List all forms | Authenticated |
| `GET` | `/api/forms/{id}` | Get form detail + version history | Authenticated |
| `POST` | `/api/forms/` | Create a form | Authenticated |
| `PUT` | `/api/forms/{id}` | Update a form | Authenticated |
| `DELETE` | `/api/forms/{id}` | Delete a form | Authenticated |
| `POST` | `/api/forms/{id}/publish` | Publish a form | Authenticated |
| `POST` | `/api/forms/{id}/archive` | Archive a form | Authenticated |
| `POST` | `/api/forms/{id}/duplicate` | Duplicate a form | Authenticated |
| `GET` | `/api/forms/{id}/preview` | Preview a form | Authenticated |

#### Create Form

```http
POST /api/forms/
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Employee Onboarding",
  "description": "New hire intake form",
  "categoryId": "44444444-4444-4444-4444-444444444444",
  "jsonDefinition": "{\"type\":\"form\",\"fields\":[]}"
}
```

---

### Form Categories

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/form-categories/` | List form categories | Authenticated |
| `POST` | `/api/form-categories/` | Create a category | Authenticated |
| `PUT` | `/api/form-categories/{id}` | Update a category | Authenticated |
| `DELETE` | `/api/form-categories/{id}` | Delete a category | Authenticated |

---

### Workflows

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/workflows/` | List all workflows | Authenticated |
| `GET` | `/api/workflows/{id}` | Get workflow detail + version history | Authenticated |
| `POST` | `/api/workflows/` | Create a workflow | Authenticated |
| `PUT` | `/api/workflows/{id}` | Update a workflow | Authenticated |
| `DELETE` | `/api/workflows/{id}` | Delete a workflow | Authenticated |
| `POST` | `/api/workflows/{id}/publish` | Publish a workflow | Authenticated |
| `POST` | `/api/workflows/{id}/archive` | Archive a workflow | Authenticated |
| `POST` | `/api/workflows/{id}/duplicate` | Duplicate a workflow | Authenticated |
| `GET` | `/api/workflows/{id}/definition` | Get raw JSON definition | Authenticated |

#### Create Workflow

```http
POST /api/workflows/
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Leave Request",
  "description": "Employee leave request approval process",
  "jsonDefinition": "{\"nodes\":[],\"connections\":[]}"
}
```

---

### Workflow Categories

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| `GET` | `/api/workflow-categories/` | List categories | `Administration.ManageWorkflowCategories` |
| `POST` | `/api/workflow-categories/` | Create a category | `Administration.ManageWorkflowCategories` |
| `PUT` | `/api/workflow-categories/{id}` | Update a category | `Administration.ManageWorkflowCategories` |
| `DELETE` | `/api/workflow-categories/{id}` | Delete a category | `Administration.ManageWorkflowCategories` |

---

### Workflow Subscriptions

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| `GET` | `/api/workflow-subscriptions/` | List all subscriptions | `WorkflowSubscriptions.Manage` |
| `POST` | `/api/workflow-subscriptions/` | Subscribe a user to a workflow | `WorkflowSubscriptions.Manage` |
| `DELETE` | `/api/workflow-subscriptions/{id}` | Remove a subscription | `WorkflowSubscriptions.Manage` |

#### Subscribe User to Workflow

```http
POST /api/workflow-subscriptions/
Authorization: Bearer <token>
Content-Type: application/json

{
  "workflowId": "55555555-5555-5555-5555-555555555555",
  "userId": "11111111-1111-1111-1111-111111111111"
}
```

---

### Workflow Instances

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/workflow-instances/` | Start a new instance | Authenticated |
| `GET` | `/api/workflow-instances/` | List all instances | Authenticated |
| `GET` | `/api/workflow-instances/{id}` | Get instance detail + tasks + history | Authenticated |
| `POST` | `/api/workflow-instances/{id}/cancel` | Cancel a running instance | Authenticated |

#### Start Workflow Instance

```http
POST /api/workflow-instances/
Authorization: Bearer <token>
Content-Type: application/json

{
  "workflowId": "55555555-5555-5555-5555-555555555555"
}
```

#### Cancel Workflow Instance

```http
POST /api/workflow-instances/{instanceId}/cancel
Authorization: Bearer <token>
Content-Type: application/json

{
  "reason": "No longer needed"
}
```

---

### Tasks

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/tasks/` | List all tasks | Authenticated |
| `GET` | `/api/tasks/{id}` | Get task by ID | Authenticated |
| `POST` | `/api/tasks/{id}/approve` | Approve a task | Authenticated |
| `POST` | `/api/tasks/{id}/reject` | Reject a task | Authenticated |
| `POST` | `/api/tasks/{id}/submit-form` | Submit form data for a form task | Authenticated |

#### Approve Task

```http
POST /api/tasks/{taskId}/approve
Authorization: Bearer <token>
Content-Type: application/json

{
  "comments": "Looks good, approved."
}
```

#### Reject Task

```http
POST /api/tasks/{taskId}/reject
Authorization: Bearer <token>
Content-Type: application/json

{
  "comments": "Missing required documentation."
}
```

#### Submit Form Task

```http
POST /api/tasks/{taskId}/submit-form
Authorization: Bearer <token>
Content-Type: application/json

{
  "formData": "{\"field1\":\"value1\",\"field2\":\"value2\"}",
  "comments": "Form completed."
}
```

---

### Inbox

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/inbox/tasks` | Get tasks assigned to me (optional `?status=`) | Authenticated |
| `GET` | `/api/inbox/workflows` | Get workflows I started (optional `?status=`) | Authenticated |
| `GET` | `/api/inbox/dashboard` | Get personal dashboard summary | Authenticated |

#### Get My Tasks

```http
GET /api/inbox/tasks?status=Pending
Authorization: Bearer <token>
```

#### Get Personal Dashboard

```http
GET /api/inbox/dashboard
Authorization: Bearer <token>
```

**Response:**

```json
{
  "pendingTasks": 5,
  "completedTasks": 12,
  "runningWorkflows": 3,
  "completedWorkflows": 8
}
```

---

### Admin Dashboard

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| `GET` | `/api/admin/dashboard` | System-wide metrics | `Administration.ViewDashboard` |

```http
GET /api/admin/dashboard
Authorization: Bearer <token>
```

**Response:**

```json
{
  "totalUsers": 150,
  "activeUsers": 120,
  "totalForms": 34,
  "publishedForms": 28,
  "totalWorkflows": 15,
  "runningWorkflowInstances": 42,
  "completedWorkflowInstances": 310,
  "pendingTasks": 18
}
```

---

### Notifications

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/notifications/` | Get my notifications (optional `?type=&isRead=`) | Authenticated |
| `GET` | `/api/notifications/{id}` | Get notification by ID | Authenticated |
| `PUT` | `/api/notifications/{id}/read` | Mark as read | Authenticated |
| `PUT` | `/api/notifications/read-all` | Mark all as read | Authenticated |
| `DELETE` | `/api/notifications/{id}` | Delete a notification | Authenticated |

---

### Audit Logs

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/audit-logs/` | Search audit logs (optional filters) | Authenticated |
| `GET` | `/api/audit-logs/{id}` | Get audit log by ID | Authenticated |

#### Search Audit Logs

```http
GET /api/audit-logs/?userId=11111111-1111-1111-1111-111111111111&entity=WorkflowInstance&from=2026-07-01&to=2026-07-16
Authorization: Bearer <token>
```

**Query Parameters (all optional):**

| Parameter | Description |
|-----------|-------------|
| `userId` | Filter by user ID |
| `entity` | Filter by entity type |
| `eventType` | Filter by event type |
| `from` | Start date (ISO 8601) |
| `to` | End date (ISO 8601) |

---

### Files

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/files/` | List files (optional `?category=`) | Authenticated |
| `GET` | `/api/files/{id}` | Get file metadata | Authenticated |
| `POST` | `/api/files/upload` | Upload a file | Authenticated |
| `GET` | `/api/files/{id}/download` | Download a file | Authenticated |
| `GET` | `/api/files/{id}/preview` | Preview a file (inline) | Authenticated |
| `PUT` | `/api/files/{id}` | Rename a file | Authenticated |
| `DELETE` | `/api/files/{id}` | Delete a file | Authenticated |

#### Upload File

```http
POST /api/files/upload?category=documents
Authorization: Bearer <token>
Content-Type: multipart/form-data

file: <binary file data>
```

#### Rename File

```http
PUT /api/files/{fileId}
Authorization: Bearer <token>
Content-Type: application/json

{
  "fileName": "renamed-document.pdf"
}
```

---

### Reports

All report endpoints support an optional `format` query parameter (`excel` or `pdf`) for export. Without it, JSON is returned.

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/reports/dashboard` | Dashboard summary report | Authenticated |
| `GET` | `/api/reports/workflows` | Workflow execution report | Authenticated |
| `GET` | `/api/reports/tasks` | Task activity report | Authenticated |
| `GET` | `/api/reports/forms` | Form usage report | Authenticated |
| `GET` | `/api/reports/users` | User activity report | Authenticated |

#### Workflow Report with Filters

```http
GET /api/reports/workflows?status=Completed&from=2026-07-01&to=2026-07-16&format=excel
Authorization: Bearer <token>
```

---

### Search

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/search` | Global search across workflows, forms, and tasks | Authenticated |

```http
GET /api/search?q=leave+request
Authorization: Bearer <token>
```

**Response:**

```json
{
  "results": [
    {
      "type": "Workflow",
      "id": "55555555-5555-5555-5555-555555555555",
      "title": "Leave Request",
      "description": "Employee leave request approval process",
      "status": "Published"
    }
  ],
  "totalCount": 1
}
```

---

## Schemas

### AuthResponse

| Property | Type |
|---|---|
| `accessToken` | `string` |
| `refreshToken` | `string` |
| `expiresAt` | `datetime` |

### UserProfileResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `email` | `string` |
| `firstName` | `string` |
| `lastName` | `string` |
| `isActive` | `boolean` |
| `createdAt` | `datetime` |

### UserResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `email` | `string` |
| `firstName` | `string` |
| `lastName` | `string` |
| `isActive` | `boolean` |
| `createdAt` | `datetime` |
| `updatedAt` | `datetime?` |

### RoleResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `name` | `string` |
| `isActive` | `boolean` |
| `createdAt` | `datetime` |
| `updatedAt` | `datetime?` |

### RoleWithPermissionsResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `name` | `string` |
| `isActive` | `boolean` |
| `createdAt` | `datetime` |
| `updatedAt` | `datetime?` |
| `permissions` | `string[]` |

### PlanResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `name` | `string` |
| `slug` | `string` |
| `description` | `string?` |
| `price` | `decimal` |
| `maxUsers` | `integer` |
| `maxStorageMb` | `integer` |
| `maxWorkflows` | `integer` |
| `isActive` | `boolean` |

### SubscriptionResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `userId` | `guid` |
| `planId` | `guid` |
| `status` | `Active \| Expired \| Cancelled` |
| `startDate` | `datetime` |
| `endDate` | `datetime?` |

### SubscriptionStatusResponse

| Property | Type |
|---|---|
| `hasActiveSubscription` | `boolean` |
| `subscriptionId` | `guid?` |
| `planId` | `guid?` |
| `planName` | `string?` |
| `endDate` | `datetime?` |

### PayPaymentResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `status` | `Pending \| Completed \| Failed \| Refunded` |
| `transactionId` | `string?` |
| `amount` | `decimal` |

### TenantResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `name` | `string` |
| `slug` | `string` |
| `status` | `Active \| Suspended \| Disabled` |

### TenantMemberResponse

| Property | Type |
|---|---|
| `tenantId` | `guid` |
| `userId` | `guid` |
| `role` | `Owner \| Admin \| Member` |
| `joinedAt` | `datetime` |

### InvitationResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `email` | `string` |
| `role` | `Owner \| Admin \| Member` |
| `status` | `Pending \| Accepted \| Expired \| Cancelled` |
| `expiresAt` | `datetime` |
| `acceptedAt` | `datetime?` |
| `createdAt` | `datetime` |

### InvitationAcceptResponse

| Property | Type |
|---|---|
| `tenantId` | `guid` |
| `userId` | `guid` |
| `role` | `Owner \| Admin \| Member` |

### FormResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `name` | `string` |
| `description` | `string?` |
| `categoryId` | `guid?` |
| `categoryName` | `string?` |
| `status` | `Draft \| Published \| Archived` |
| `currentVersion` | `integer` |
| `createdAt` | `datetime` |
| `updatedAt` | `datetime?` |

### FormDetailResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `name` | `string` |
| `description` | `string?` |
| `categoryId` | `guid?` |
| `categoryName` | `string?` |
| `status` | `Draft \| Published \| Archived` |
| `currentVersion` | `integer` |
| `jsonDefinition` | `string` |
| `createdAt` | `datetime` |
| `createdBy` | `string` |
| `updatedAt` | `datetime?` |
| `updatedBy` | `string?` |
| `versions` | `FormVersionResponse[]` |

### FormVersionResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `versionNumber` | `integer` |
| `status` | `Draft \| Published \| Archived` |
| `jsonDefinition` | `string` |
| `createdAt` | `datetime` |
| `notes` | `string?` |

### FormCategoryResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `name` | `string` |
| `description` | `string?` |
| `createdAt` | `datetime` |
| `updatedAt` | `datetime?` |

### WorkflowResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `name` | `string` |
| `description` | `string?` |
| `status` | `Draft \| Published \| Archived` |
| `currentVersion` | `integer` |
| `createdAt` | `datetime` |
| `updatedAt` | `datetime?` |

### WorkflowDetailResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `name` | `string` |
| `description` | `string?` |
| `status` | `Draft \| Published \| Archived` |
| `currentVersion` | `integer` |
| `jsonDefinition` | `string` |
| `createdAt` | `datetime` |
| `createdBy` | `string` |
| `updatedAt` | `datetime?` |
| `updatedBy` | `string?` |
| `versions` | `WorkflowVersionResponse[]` |

### WorkflowVersionResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `versionNumber` | `integer` |
| `status` | `Draft \| Published \| Archived` |
| `jsonDefinition` | `string` |
| `createdAt` | `datetime` |
| `notes` | `string?` |

### WorkflowInstanceResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `workflowId` | `guid` |
| `workflowName` | `string` |
| `workflowVersion` | `integer` |
| `status` | `Running \| Completed \| Cancelled` |
| `startedBy` | `string` |
| `startedDate` | `datetime` |
| `completedDate` | `datetime?` |
| `createdAt` | `datetime` |

### WorkflowInstanceDetailResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `workflowId` | `guid` |
| `workflowName` | `string` |
| `workflowVersion` | `integer` |
| `status` | `Running \| Completed \| Cancelled` |
| `startedBy` | `string` |
| `startedDate` | `datetime` |
| `completedDate` | `datetime?` |
| `variables` | `string` |
| `createdAt` | `datetime` |
| `updatedAt` | `datetime?` |
| `tasks` | `WorkflowTaskResponse[]` |
| `history` | `WorkflowHistoryResponse[]` |

### WorkflowHistoryResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `eventType` | `string` |
| `nodeId` | `string?` |
| `nodeName` | `string?` |
| `data` | `string?` |
| `timestamp` | `datetime` |
| `performedBy` | `string?` |

### WorkflowTaskResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `workflowInstanceId` | `guid` |
| `nodeId` | `string` |
| `nodeName` | `string` |
| `taskType` | `Approval \| FormSubmission` |
| `title` | `string` |
| `assignedToUserId` | `string?` |
| `assignedToRole` | `string?` |
| `status` | `Pending \| InProgress \| Completed \| Rejected` |
| `completedBy` | `string?` |
| `completedDate` | `datetime?` |
| `comments` | `string?` |
| `formData` | `string?` |

### NotificationResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `userId` | `string` |
| `title` | `string` |
| `message` | `string` |
| `type` | `string` |
| `isRead` | `boolean` |
| `createdAt` | `datetime` |

### AuditLogResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `event` | `string` |
| `userId` | `string` |
| `userName` | `string?` |
| `entity` | `string` |
| `entityId` | `string?` |
| `timestamp` | `datetime` |
| `details` | `string?` |

### FileResponse

| Property | Type |
|---|---|
| `id` | `guid` |
| `fileName` | `string` |
| `originalFileName` | `string` |
| `fileSize` | `long` |
| `contentType` | `string` |
| `category` | `string` |
| `createdBy` | `string` |
| `createdAt` | `datetime` |
| `updatedAt` | `datetime?` |

### DashboardResponse

| Property | Type |
|---|---|
| `pendingTasks` | `integer` |
| `completedTasks` | `integer` |
| `runningWorkflows` | `integer` |
| `completedWorkflows` | `integer` |

---

## Permissions Reference

| Permission | Description |
|------------|-------------|
| `Users.View` | View user list and details |
| `Users.Create` | Create new users |
| `Users.Update` | Update user profiles and status |
| `Users.Delete` | Delete users |
| `Roles.View` | View roles and their permissions |
| `Roles.Create` | Create new roles |
| `Roles.Update` | Update roles and assign users |
| `Roles.Delete` | Delete roles |
| `Tenants.View` | View tenant information |
| `Tenants.Create` | Create new tenants |
| `Tenants.Manage` | Manage tenant settings and members |
| `WorkflowSubscriptions.Manage` | Manage workflow-to-user subscriptions |
| `Files.View` | View file listings and metadata |
| `Files.Upload` | Upload new files |
| `Files.Download` | Download files |
| `Files.Rename` | Rename files |
| `Files.Delete` | Delete files |
| `Administration.ViewDashboard` | View admin dashboard metrics |
| `Administration.ManageSettings` | Manage tenant settings |
| `Administration.ManageWorkflowCategories` | Manage workflow categories |
| `Administration.ManageLookups` | Manage lookup values |

---

## Development

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- Docker (for MinIO and PostgreSQL)

### Running

```bash
cd src/BPMS.Api
dotnet run
```

The API starts with Swagger UI enabled at `https://localhost:<port>/swagger`.

### Health Check

```
GET /health
```

Returns `200 OK` when the database connection is healthy.
