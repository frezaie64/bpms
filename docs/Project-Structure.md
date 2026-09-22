# Project Structure

## Architecture

* Modular Monolith
* ASP.NET Core Web API (Minimal APIs)
* PostgreSQL database
* MinIO for object storage
* Shared Database, Shared Schema multi-tenancy
* EF Core global query filters for tenant isolation

## Solution Layout

```
BPMS.slnx
  src/
    BPMS.Api/BPMS.Api.csproj
    BPMS.Shared/BPMS.Shared.csproj
    BPMS.Modules.FileManagement/BPMS.Modules.FileManagement.csproj
    BPMS.Modules.Forms/BPMS.Modules.Forms.csproj
    BPMS.Modules.Identity/BPMS.Modules.Identity.csproj
    BPMS.Modules.Notifications/BPMS.Modules.Notifications.csproj
    BPMS.Modules.Tenancy/BPMS.Modules.Tenancy.csproj
    BPMS.Modules.WorkflowDefinitions/BPMS.Modules.WorkflowDefinitions.csproj
    BPMS.Modules.WorkflowRuntime/BPMS.Modules.WorkflowRuntime.csproj
```

---

## Module Structure

Each module follows this convention:

```
ModuleName/
    ModuleName.csproj
    I{ModuleName}Module.cs              -- Interface for DI
    {ModuleName}Module.cs               -- Implementation (business logic)
    {ModuleName}ModuleRegistration.cs    -- DI extension method

    Entities/
        Entity.cs                       -- Domain entities, enums

    Models/
        ModelNameModels.cs              -- Request/response records

    Endpoints/
        EntityEndpoints.cs              -- Minimal API route handlers

    Persistence/
        EntityConfiguration.cs          -- EF Core IEntityTypeConfiguration
```

---

## BPMS.Api

### Program.cs

Application entry point. Configures:
- Serilog logging
- JWT authentication
- Permission-based authorization
- DbContext with Npgsql (auto-migration + plan seed on startup)
- All 8 module registrations
- Health checks with DbContext check
- Swagger
- Tenant middleware
- Endpoint mapping for all 18 endpoint groups

### Configuration

| File | Purpose |
|------|---------|
| `appsettings.json` | Connection strings, JWT settings, MinIO, Serilog, File validation |
| `appsettings.Development.json` | Development overrides |
| `Dockerfile` | Multi-stage Docker build |
| `Properties/launchSettings.json` | Launch profiles (HTTP: 5171, HTTPS: 7204) |

---

## BPMS.Shared

Shared kernel referenced by all modules. No external module dependencies.

### Abstractions/

| File | Description |
|------|-------------|
| `IEntity.cs` | Marker interface: `Guid Id { get; }` |
| `BaseEntity.cs` | Abstract base class: `Id = Guid.NewGuid()` |
| `ITenantEntity.cs` | Interface for multi-tenancy: `string TenantId { get; }` |
| `IAuditableEntity.cs` | Interface for audit: `DateTime CreatedAt`, `string CreatedBy`, `DateTime? UpdatedAt`, `string? UpdatedBy` |

### Authorization/

| File | Description |
|------|-------------|
| `Permissions.cs` | Static permission constants (21 permissions across 6 groups). Exposes `All` array used to register policies. |
| `AuthorizationExtensions.cs` | `AddPermissionAuthorization()` — registers one `RequireClaim("permission", x)` policy per permission. |

#### Permission Groups

| Group | Permissions |
|-------|-------------|
| `Users` | View, Create, Update, Delete |
| `Roles` | View, Create, Update, Delete |
| `WorkflowSubscriptions` | Manage |
| `Files` | View, Upload, Download, Rename, Delete |
| `Tenants` | View, Create, Manage |
| `Administration` | ViewDashboard, ManageSettings, ManageWorkflowCategories, ManageLookups |

### Extensions/

| File | Description |
|------|-------------|
| `JwtExtensions.cs` | `AddJwtAuthentication()` — configures JWT Bearer from `Jwt:SecretKey`, `Jwt:Issuer`, `Jwt:Audience`. Tokens carry `sub`, `permission` claims. |
| `TenantMiddleware.cs` | Reads `X-Tenant-Id` header and JWT `sub` claim, sets `ITenantService.TenantId` and `ITenantService.UserId` for the scoped request. |

### Persistence/

| File | Description |
|------|-------------|
| `BpmsDbContext.cs` | Main EF Core DbContext. Scans all `BPMS.Modules.*` assemblies for `IEntityTypeConfiguration` implementations. Applies `ITenantEntity` global query filter to all entities except `User`. Auto-sets `TenantId` on added `ITenantEntity` entities. Auto-sets `CreatedAt`/`CreatedBy`/`UpdatedAt`/`UpdatedBy` on all `IAuditableEntity` entities. |

### Services/

| File | Description |
|------|-------------|
| `ITenantService.cs` | Interface: `string TenantId`, `string? UserId`, `SetTenant(string)`, `SetUser(string?)` |
| `TenantService.cs` | Scoped implementation holding current tenant/user per request |

### Migrations/

| File | Description |
|------|-------------|
| `20260713085332_Phase2_UserAndRoleManagement.cs` | Creates Users, Roles, Permissions, UserRoles, RolePermissions tables |
| `20260713171031_Phase4_WorkflowDefinitions.cs` | Creates Workflows, WorkflowVersions tables |
| `20260714085552_Phase6_WorkflowSubscriptionAndInbox.cs` | Creates WorkflowInstances, WorkflowTasks, WorkflowHistory, WorkflowSubscriptions tables |
| `20260714095213_Phase7_NotificationsAndAudit.cs` | Creates Notifications, AuditLogs tables |
| `20260715072526_Phase8_TenantFlow.cs` | Creates Tenants, TenantMembers, Plans, Subscriptions, Files tables |
| `BpmsDbContextModelSnapshot.cs` | EF Core model snapshot |

---

## BPMS.Modules.Identity

User management, roles, permissions, and authentication.

Dependencies: `BPMS.Shared`, `BCrypt.Net-Next`

### Entities

| Entity | Base Types | Table | Key Fields |
|--------|------------|-------|------------|
| `User` | BaseEntity, IAuditableEntity | `Users` | `TenantId` (nullable, string), `Email` (unique), `PasswordHash`, `FirstName`, `LastName`, `Status` (Registered/Active/Suspended), `RefreshToken`, `RefreshTokenExpiry`, `IsDeleted` |
| `Role` | BaseEntity, ITenantEntity, IAuditableEntity | `Roles` | `TenantId`, `Name`, `IsActive` |
| `Permission` | BaseEntity | `Permissions` | `Name` (unique), `Group`, `Description` |
| `UserRole` | IEntity | `UserRoles` | `UserId`, `RoleId` (composite PK) |
| `RolePermission` | IEntity | `RolePermissions` | `RoleId`, `PermissionId` (composite PK) |

### Enums

| Enum | Values |
|------|--------|
| `UserStatus` | Registered, Active, Suspended |

### Models

| Record | Fields |
|--------|--------|
| `RegisterRequest` | Email, Password, FirstName, LastName |
| `LoginRequest` | Email, Password |
| `RefreshTokenRequest` | RefreshToken |
| `AuthResponse` | AccessToken, RefreshToken, ExpiresAt |
| `CreateRoleRequest` | Name |
| `UpdateRoleRequest` | Name |
| `RoleResponse` | Id, Name, IsActive, CreatedAt, UpdatedAt |
| `RoleWithPermissionsResponse` | Id, Name, IsActive, CreatedAt, UpdatedAt, Permissions (List\<string\>) |
| `AssignUsersRequest` | UserIds (List\<Guid\>) |
| `UserProfileResponse` | Id, Email, FirstName, LastName, IsActive, CreatedAt |
| `UpdateProfileRequest` | FirstName, LastName |
| `ChangePasswordRequest` | CurrentPassword, NewPassword |
| `CreateUserRequest` | Email, Password, FirstName, LastName |
| `UpdateUserRequest` | Email, FirstName, LastName |
| `SetUserStatusRequest` | IsActive |
| `UserResponse` | Id, Email, FirstName, LastName, IsActive, CreatedAt, UpdatedAt |

### Endpoints

| Route | Methods | Tag | Auth | Description |
|-------|---------|-----|------|-------------|
| `/api/auth/register` | POST | — | Public | Register new user |
| `/api/auth/login` | POST | — | Public | Login, get tokens |
| `/api/auth/refresh` | POST | — | Public | Refresh access token |
| `/api/users/me` | GET, PUT | — | Authenticated | Get/update profile |
| `/api/users/change-password` | POST | — | Authenticated | Change password |
| `/api/users` | GET, POST | — | Users.View / Users.Create | List / create users |
| `/api/users/{id}` | GET, PUT, DELETE | — | Users.View / Update / Delete | Get / update / delete user |
| `/api/users/{id}/status` | PUT | — | Users.Update | Activate/deactivate user |
| `/api/roles` | GET, POST | — | Roles.View / Create | List / create roles |
| `/api/roles/{id}` | GET, PUT, DELETE | — | Roles.View / Update / Delete | Get / update / delete role |
| `/api/roles/{id}/users` | POST | — | Roles.Update | Assign users to role |
| `/api/roles/{id}/users/{userId}` | DELETE | — | Roles.Update | Remove user from role |

### Persistence Configurations

| File | Table | Key Config |
|------|-------|------------|
| `UserConfiguration.cs` | `Users` | Unique index on `Email`, query filter `!IsDeleted` |
| `RoleConfiguration.cs` | `Roles` | Unique index on `(TenantId, Name)` |
| `PermissionConfiguration.cs` | `Permissions` | Unique index on `Name` |
| `UserRoleConfiguration.cs` | `UserRoles` | Composite PK `(UserId, RoleId)`, cascade delete FKs |
| `RolePermissionConfiguration.cs` | `RolePermissions` | Composite PK `(RoleId, PermissionId)`, cascade delete FKs |

### Services

| Service | Interface | Description |
|---------|-----------|-------------|
| `UserService` | `IUserService` | Create/update/delete users, assign roles, authenticate with BCrypt password hash |
| `RoleService` | `IRoleService` | Create/update/delete roles, assign permissions |
| `PasswordPolicy` | `IPasswordPolicy` | Validate password complexity requirements |

---

## BPMS.Modules.Tenancy

Multi-tenant management — plans, subscriptions, tenant settings, lookup values.

Dependencies: `BPMS.Shared`, `BPMS.Modules.Identity`

### Entities

| Entity | Base Types | Table | Key Fields |
|--------|------------|-------|------------|
| `Tenant` | BaseEntity, IAuditableEntity | `Tenants` | `Name`, `Slug` (unique), `ConnectionString` (nullable), `PlanId`, `SubscriptionId`, `Status` (Active/Suspended/Disabled) |
| `TenantMember` | — | `TenantMembers` | `TenantId`, `UserId` (composite PK), `Role` (Owner/Admin/Member), `JoinedAt` |
| `TenantSetting` | BaseEntity, ITenantEntity, IAuditableEntity | `TenantSettings` | `TenantId` (unique), `TenantName`, `Logo`, `TimeZone`, `DateFormat`, `Language`, `DefaultFileSizeLimit`, `DefaultPageSize`, `SmtpHost`, `SmtpPort`, `SmtpUsername`, `SmtpPassword`, `SenderName`, `SenderEmail`, `SmsProviderName`, `SmsApiUrl`, `SmsApiKey`, `SmsSenderId` |
| `LookupValue` | BaseEntity, ITenantEntity, IAuditableEntity | `LookupValues` | `TenantId`, `Group`, `Name`, `Value`, `IsActive`, `IsDeleted` |
| `Plan` | BaseEntity, IAuditableEntity | `Plans` | `Name`, `Slug` (unique), `Description`, `Price` (decimal), `MaxUsers`, `MaxStorageMb`, `MaxWorkflows`, `IsActive` |
| `Subscription` | BaseEntity, IAuditableEntity | `Subscriptions` | `UserId`, `PlanId`, `Status` (Active/Expired/Cancelled), `StartDate`, `EndDate` |

### Enums

| Enum | Values |
|------|--------|
| `TenantStatus` | Active, Suspended, Disabled |
| `TenantMemberRole` | Owner, Admin, Member |
| `SubscriptionStatus` | Active, Expired, Cancelled |

### Models

| Record | Fields |
|--------|--------|
| `TenantSettingResponse` | Id, TenantName, Logo, TimeZone, DateFormat, Language, DefaultFileSizeLimit, DefaultPageSize, SmtpHost, SmtpPort, SmtpUsername, SenderName, SenderEmail, SmsProviderName, SmsApiUrl, SmsSenderId, CreatedAt, UpdatedAt |
| `UpdateTenantSettingRequest` | All setting fields (includes SmtpPassword, SmsApiKey for writes) |
| `CreateLookupRequest` | Group, Name, Value |
| `UpdateLookupRequest` | Group, Name, Value, IsActive |
| `LookupResponse` | Id, Group, Name, Value, IsActive, CreatedAt, UpdatedAt |

### Endpoints

| Route | Methods | Tag | Auth | Description |
|-------|---------|-----|------|-------------|
| `/api/tenants` | GET, POST | — | Authenticated | List / create tenants |
| `/api/tenants/my` | GET | — | Authenticated | Get current user's tenants |
| `/api/tenants/{id}` | GET | — | Authenticated | Get tenant by ID |
| `/api/tenants/{id}/invite` | POST | — | Authenticated | Invite member to tenant |
| `/api/tenants/{id}/members` | GET | — | Authenticated | List tenant members |
| `/api/plans` | GET, POST | — | Public / — | List (public) / create plans |
| `/api/subscriptions` | POST | — | Authenticated | Create subscription |
| `/api/subscriptions/my` | GET | — | Authenticated | Get user's subscriptions |
| `/api/settings` | GET, PUT | Settings | ManageSettings | Get / update tenant settings |
| `/api/lookups` | GET, POST | Lookups | ManageLookups | List (filter by group) / create lookups |
| `/api/lookups/{id}` | PUT, DELETE | Lookups | ManageLookups | Update / delete lookup |

### Persistence Configurations

| File | Table | Key Config |
|------|-------|------------|
| `TenantConfiguration.cs` | `Tenants` | Unique index on `Slug`, `Status` stored as int |
| `TenantMemberConfiguration.cs` | `TenantMembers` | Composite PK `(TenantId, UserId)`, FK→Tenant cascade |
| `TenantSettingConfiguration.cs` | `TenantSettings` | Unique index on `TenantId` |
| `LookupValueConfiguration.cs` | `LookupValues` | Unique index on `(TenantId, Group, Name)`, query filter `!IsDeleted` |
| `PlanConfiguration.cs` | `Plans` | Unique index on `Slug`, `Price` decimal(18,2) |
| `SubscriptionConfiguration.cs` | `Subscriptions` | `Status` stored as int |

---

## BPMS.Modules.Forms

Forms management with versioning and categories.

Dependencies: `BPMS.Shared`

### Entities

| Entity | Base Types | Table | Key Fields |
|--------|------------|-------|------------|
| `Form` | BaseEntity, ITenantEntity, IAuditableEntity | `Forms` | `TenantId`, `Name`, `Description`, `CategoryId`, `Status` (Draft/Published/Archived), `CurrentVersion`, `JsonDefinition`, `IsDeleted` |
| `FormCategory` | BaseEntity, ITenantEntity, IAuditableEntity | `FormCategories` | `TenantId`, `Name`, `Description`, `IsDeleted` |
| `FormVersion` | BaseEntity | `FormVersions` | `FormId`, `VersionNumber`, `JsonDefinition`, `Status`, `CreatedAt`, `Notes` |

### Enums

| Enum | Values |
|------|--------|
| `FormStatus` | Draft, Published, Archived |

### Models

| Record | Fields |
|--------|--------|
| `CreateFormRequest` | Name, Description?, CategoryId?, JsonDefinition? |
| `UpdateFormRequest` | Name, Description?, CategoryId?, JsonDefinition? |
| `FormResponse` | Id, Name, Description, CategoryId, CategoryName, Status, CurrentVersion, CreatedAt, UpdatedAt |
| `FormDetailResponse` | Id, Name, Description, CategoryId, CategoryName, Status, CurrentVersion, JsonDefinition, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Versions |
| `FormVersionResponse` | Id, VersionNumber, Status, JsonDefinition, CreatedAt, Notes? |
| `CreateFormCategoryRequest` | Name, Description? |
| `UpdateFormCategoryRequest` | Name, Description? |
| `FormCategoryResponse` | Id, Name, Description, CreatedAt, UpdatedAt |

### Endpoints

| Route | Methods | Tag | Auth | Description |
|-------|---------|-----|------|-------------|
| `/api/forms` | GET, POST | Forms | Authenticated | List / create forms |
| `/api/forms/{id}` | GET, PUT, DELETE | Forms | Authenticated | Get / update / delete form |
| `/api/forms/{id}/publish` | POST | Forms | Authenticated | Publish form (creates version) |
| `/api/forms/{id}/archive` | POST | Forms | Authenticated | Archive form |
| `/api/forms/{id}/duplicate` | POST | Forms | Authenticated | Duplicate form |
| `/api/forms/{id}/preview` | GET | Forms | Authenticated | Preview form |
| `/api/form-categories` | GET, POST | Form Categories | Authenticated | List / create categories |
| `/api/form-categories/{id}` | PUT, DELETE | Form Categories | Authenticated | Update / delete category |

### Persistence Configurations

| File | Table | Key Config |
|------|-------|------------|
| `FormConfiguration.cs` | `Forms` | Index on `(TenantId, Name)`, query filter `!IsDeleted`, FK to Category (SetNull) |
| `FormCategoryConfiguration.cs` | `FormCategories` | Unique index on `(TenantId, Name)`, query filter `!IsDeleted` |
| `FormVersionConfiguration.cs` | `FormVersions` | Unique index on `(FormId, VersionNumber)`, FK to Form (Cascade) |

---

## BPMS.Modules.WorkflowDefinitions

Workflow template definitions, versioning, categories, and subscriptions.

Dependencies: `BPMS.Shared`

### Entities

| Entity | Base Types | Table | Key Fields |
|--------|------------|-------|------------|
| `Workflow` | BaseEntity, ITenantEntity, IAuditableEntity | `Workflows` | `TenantId`, `Name`, `Description`, `Status` (Draft/Published/Archived), `CurrentVersion`, `JsonDefinition`, `IsDeleted` |
| `WorkflowVersion` | BaseEntity, ITenantEntity | `WorkflowVersions` | `TenantId`, `WorkflowId`, `VersionNumber`, `JsonDefinition`, `Status` |
| `WorkflowCategory` | BaseEntity, ITenantEntity, IAuditableEntity | `WorkflowCategories` | `TenantId`, `Name`, `Description`, `IsDeleted` |
| `WorkflowSubscription` | BaseEntity, ITenantEntity | `WorkflowSubscriptions` | `TenantId`, `WorkflowId`, `UserId` |

### Enums

| Enum | Values |
|------|--------|
| `WorkflowStatus` | Draft, Published, Archived |

### Models

| Record | Fields |
|--------|--------|
| `CreateWorkflowRequest` | Name, Description?, JsonDefinition? |
| `UpdateWorkflowRequest` | Name, Description?, JsonDefinition? |
| `WorkflowResponse` | Id, Name, Description, Status, CurrentVersion, CreatedAt, UpdatedAt |
| `WorkflowDetailResponse` | Id, Name, Description, Status, CurrentVersion, JsonDefinition, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Versions |
| `WorkflowVersionResponse` | Id, VersionNumber, Status, JsonDefinition, CreatedAt, Notes? |
| `CreateWorkflowCategoryRequest` | Name, Description? |
| `UpdateWorkflowCategoryRequest` | Name, Description? |
| `WorkflowCategoryResponse` | Id, Name, Description, CreatedAt, UpdatedAt |
| `SubscribeRequest` | WorkflowId, UserId |
| `WorkflowSubscriptionResponse` | Id, WorkflowId, UserId |
| `CatalogWorkflowResponse` | Id, Name, Description?, CurrentVersion, CreatedAt |

### Endpoints

| Route | Methods | Tag | Auth | Description |
|-------|---------|-----|------|-------------|
| `/api/workflows` | GET, POST | Workflows | Authenticated | List / create workflows |
| `/api/workflows/{id}` | GET, PUT, DELETE | Workflows | Authenticated | Get / update / delete workflow |
| `/api/workflows/{id}/publish` | POST | Workflows | Authenticated | Publish workflow (creates version) |
| `/api/workflows/{id}/archive` | POST | Workflows | Authenticated | Archive workflow |
| `/api/workflows/{id}/duplicate` | POST | Workflows | Authenticated | Duplicate workflow |
| `/api/workflows/{id}/definition` | GET | Workflows | Authenticated | Get raw JSON definition |
| `/api/workflow-subscriptions` | GET, POST | Workflow Subscriptions | Manage | List / create subscriptions |
| `/api/workflow-subscriptions/{id}` | DELETE | Workflow Subscriptions | Manage | Unsubscribe |
| `/api/workflow-categories` | GET, POST | Workflow Categories | ManageWorkflowCategories | List / create categories |
| `/api/workflow-categories/{id}` | PUT, DELETE | Workflow Categories | ManageWorkflowCategories | Update / delete category |

### Persistence Configurations

| File | Table | Key Config |
|------|-------|------------|
| `WorkflowConfiguration.cs` | `Workflows` | Index on `(TenantId, Name)`, query filter `!IsDeleted` |
| `WorkflowVersionConfiguration.cs` | `WorkflowVersions` | Unique index on `(WorkflowId, VersionNumber)` |
| `WorkflowCategoryConfiguration.cs` | `WorkflowCategories` | Unique index on `(TenantId, Name)`, query filter `!IsDeleted` |
| `WorkflowSubscriptionConfiguration.cs` | `WorkflowSubscriptions` | Index on `(WorkflowId, UserId)` |

---

## BPMS.Modules.WorkflowRuntime

Workflow execution engine — instances, tasks, history, inbox, dashboard.

Dependencies: `BPMS.Shared`, `BPMS.Modules.WorkflowDefinitions`, `BPMS.Modules.Notifications`, `BPMS.Modules.Identity`, `BPMS.Modules.Forms`

### Entities

| Entity | Base Types | Table | Key Fields |
|--------|------------|-------|------------|
| `WorkflowInstance` | BaseEntity, ITenantEntity, IAuditableEntity | `WorkflowInstances` | `TenantId`, `WorkflowId`, `WorkflowVersionId`, `WorkflowName`, `WorkflowVersion`, `Status` (Running/Completed/Cancelled/Failed), `StartedBy`, `StartedDate`, `CompletedDate`, `ActiveNodeIds` (JSON), `CompletedNodeIds` (JSON), `Variables` (JSON), `JsonDefinition` |
| `WorkflowTask` | BaseEntity, ITenantEntity | `WorkflowTasks` | `TenantId`, `WorkflowInstanceId`, `NodeId`, `NodeName`, `TaskType` (Approval/FillForm), `Title`, `AssignedToUserId`, `AssignedToRole`, `Status` (Pending/Approved/Rejected/Submitted), `CompletedBy`, `CompletedDate`, `Comments`, `FormData` |
| `WorkflowHistory` | BaseEntity | `WorkflowHistory` | `WorkflowInstanceId`, `EventType`, `NodeId`, `NodeName`, `Data`, `Timestamp`, `PerformedBy` |

### Enums

| Enum | Values |
|------|--------|
| `WorkflowInstanceStatus` | Running, Completed, Cancelled, Failed |
| `WorkflowTaskStatus` | Pending, Approved, Rejected, Submitted |
| `WorkflowTaskType` | Approval, FillForm |
| `WorkflowHistoryEventType` | Started, NodeExecuted, TaskCreated, TaskCompleted, WorkflowCompleted, WorkflowCancelled |

### Models

| Record | Fields |
|--------|--------|
| `StartWorkflowRequest` | WorkflowId |
| `CancelWorkflowRequest` | Reason? |
| `WorkflowInstanceResponse` | Id, WorkflowId, WorkflowName, WorkflowVersion, Status, StartedBy, StartedDate, CompletedDate, CreatedAt |
| `WorkflowInstanceDetailResponse` | Id, WorkflowId, WorkflowName, WorkflowVersion, Status, StartedBy, StartedDate, CompletedDate, Variables, CreatedAt, UpdatedAt, Tasks, History |
| `WorkflowTaskResponse` | Id, WorkflowInstanceId, NodeId, NodeName, TaskType, Title, AssignedToUserId, AssignedToRole, Status, CompletedBy, CompletedDate, Comments, FormData |
| `WorkflowHistoryResponse` | Id, EventType, NodeId?, NodeName?, Data?, Timestamp, PerformedBy? |
| `ApproveTaskRequest` | Comments? |
| `RejectTaskRequest` | Comments? |
| `SubmitFormTaskRequest` | FormData, Comments? |
| `InboxTaskResponse` | Id, WorkflowInstanceId, WorkflowName, NodeId, NodeName, TaskType, Title, Status, CompletedBy, CompletedDate |
| `InboxWorkflowInstanceResponse` | Id, WorkflowId, WorkflowName, WorkflowVersion, Status, StartedDate, CompletedDate |
| `DashboardResponse` | PendingTasks, CompletedTasks, RunningWorkflows, CompletedWorkflows |
| `AdminDashboardResponse` | TotalUsers, ActiveUsers, TotalForms, PublishedForms, TotalWorkflows, RunningWorkflowInstances, CompletedWorkflowInstances, PendingTasks |
| `WorkflowDefinition` | Nodes, Connections (runtime deserialization model) |

### Endpoints

| Route | Methods | Tag | Auth | Description |
|-------|---------|-----|------|-------------|
| `/api/workflow-instances` | GET, POST | Workflow Instances | Authenticated | List instances / start workflow |
| `/api/workflow-instances/{id}` | GET | Workflow Instances | Authenticated | Get instance detail |
| `/api/workflow-instances/{id}/cancel` | POST | Workflow Instances | Authenticated | Cancel workflow |
| `/api/tasks` | GET | Tasks | Authenticated | List all tasks |
| `/api/tasks/{id}` | GET | Tasks | Authenticated | Get task detail |
| `/api/tasks/{id}/approve` | POST | Tasks | Authenticated | Approve task |
| `/api/tasks/{id}/reject` | POST | Tasks | Authenticated | Reject task |
| `/api/tasks/{id}/submit-form` | POST | Tasks | Authenticated | Submit form task |
| `/api/inbox/tasks` | GET | Inbox | Authenticated | User's tasks (filter by status) |
| `/api/inbox/workflows` | GET | Inbox | Authenticated | User's workflow instances (filter by status) |
| `/api/inbox/dashboard` | GET | Inbox | Authenticated | User's dashboard counts |
| `/api/admin/dashboard` | GET | Administration | ViewDashboard | Tenant-wide admin dashboard |

### Persistence Configurations

| File | Table | Key Config |
|------|-------|------------|
| `WorkflowInstanceConfiguration.cs` | `WorkflowInstances` | Index on `TenantId` |
| `WorkflowTaskConfiguration.cs` | `WorkflowTasks` | Index on `TenantId` |
| `WorkflowHistoryConfiguration.cs` | `WorkflowHistory` | Index on `WorkflowInstanceId` |

### Services

| Service | Interface | Description |
|---------|-----------|-------------|
| `WorkflowEngine` | — | Core engine: `StartWorkflowAsync`, `AdvanceWorkflowAsync`, `CompleteTaskAsync`, `CancelWorkflowAsync`. Deserializes JSON definition, walks node graph, evaluates decision gateway conditions (`{{variable}} == value`), creates tasks, records history, creates notifications and audit logs via `INotificationsModule` |
| `SystemTaskService` | `ISystemTaskService` | Executes system tasks: `SendEmailAsync`, `SendSmsAsync`, `WebhookAsync`, `DelayAsync` (stub implementations) |

---

## BPMS.Modules.Notifications

In-app notifications and audit logging.

Dependencies: `BPMS.Shared`

### Entities

| Entity | Base Types | Table | Key Fields |
|--------|------------|-------|------------|
| `Notification` | BaseEntity, ITenantEntity | `Notifications` | `TenantId`, `UserId`, `Title`, `Message`, `Type`, `IsRead`, `CreatedAt` |
| `AuditLog` | BaseEntity, ITenantEntity | `AuditLogs` | `TenantId`, `Event`, `UserId`, `UserName`, `Entity`, `EntityId`, `Timestamp`, `Details` |

### Models

| Record | Fields |
|--------|--------|
| `NotificationResponse` | Id, UserId, Title, Message, Type, IsRead, CreatedAt |
| `CreateNotificationRequest` | UserId, Title, Message, Type |
| `NotificationFilterRequest` | Type?, IsRead? |
| `AuditLogResponse` | Id, Event, UserId, UserName?, Entity, EntityId?, Timestamp, Details? |
| `AuditLogSearchRequest` | UserId?, Entity?, Event?, From?, To? |

### Endpoints

| Route | Methods | Tag | Auth | Description |
|-------|---------|-----|------|-------------|
| `/api/notifications` | GET | Notifications | Authenticated | List user's notifications (filter by type, read) |
| `/api/notifications/{id}` | GET | Notifications | Authenticated | Get single notification |
| `/api/notifications/{id}/read` | PUT | Notifications | Authenticated | Mark as read |
| `/api/notifications/read-all` | PUT | Notifications | Authenticated | Mark all as read |
| `/api/notifications/{id}` | DELETE | Notifications | Authenticated | Delete notification |
| `/api/audit-logs` | GET | Audit Logs | Authenticated | Search audit logs (filter by user, entity, event, date) |
| `/api/audit-logs/{id}` | GET | Audit Logs | Authenticated | Get single audit log |

### Persistence Configurations

| File | Table | Key Config |
|------|-------|------------|
| `NotificationConfiguration.cs` | `Notifications` | Indexes on `(TenantId, UserId, IsRead)`, `(TenantId, CreatedAt)` |
| `AuditLogConfiguration.cs` | `AuditLogs` | Indexes on `(TenantId, Event)`, `(TenantId, UserId)`, `(TenantId, Entity)`, `(TenantId, Timestamp)` |

### Integration Points

The `WorkflowEngine` creates notifications and audit logs for:
- Workflow started → notification to starter + audit log
- Task assigned → notification to assignee
- Task approved/rejected/submitted → notification to starter + audit log
- Workflow completed → notification to starter + audit log
- Workflow cancelled → notification to starter + audit log

---

## BPMS.Modules.FileManagement

File storage via MinIO (S3-compatible object store).

Dependencies: `BPMS.Shared`, `Minio`

### Entities

| Entity | Base Types | Table | Key Fields |
|--------|------------|-------|------------|
| `FileItem` | BaseEntity, ITenantEntity, IAuditableEntity | `Files` | `TenantId`, `FileName`, `OriginalFileName`, `FileSize`, `ContentType`, `StoragePath`, `Category`, `IsDeleted` |

### Models

| Record | Fields |
|--------|--------|
| `RenameFileRequest` | FileName |
| `FileResponse` | Id, FileName, OriginalFileName, FileSize, ContentType, Category, CreatedBy, CreatedAt, UpdatedAt |
| `MinioOptions` | Endpoint, AccessKey, SecretKey, BucketName, UseSsl, PublicUrl |
| `FileValidationOptions` | MaxFileSize, AllowedExtensions (IOptions config classes) |

### Endpoints

| Route | Methods | Tag | Auth | Description |
|-------|---------|-----|------|-------------|
| `/api/files` | GET | Files | Authenticated | List files (filter by category) |
| `/api/files/{id}` | GET | Files | Authenticated | Get file metadata |
| `/api/files/upload` | POST | Files | Authenticated | Upload file (multipart) |
| `/api/files/{id}/download` | GET | Files | Authenticated | Download file |
| `/api/files/{id}/preview` | GET | Files | Authenticated | Preview file (inline) |
| `/api/files/{id}` | PUT | Files | Authenticated | Rename file |
| `/api/files/{id}` | DELETE | Files | Authenticated | Delete file |

### Persistence Configurations

| File | Table | Key Config |
|------|-------|------------|
| `FileConfiguration.cs` | `Files` | Index on `(TenantId, Category)`, query filter `!IsDeleted` |

### Services

| Service | Interface | Description |
|---------|-----------|-------------|
| `FileStorageService` | — | Wraps MinIO SDK: `UploadAsync`, `DownloadAsync`, `DeleteAsync`, `GetPresignedUrlAsync` |

---

## Module Dependency Graph

```
BPMS.Shared (no module dependencies)
  ├── BPMS.Modules.Tenancy → Shared, Identity
  ├── BPMS.Modules.Identity → Shared
  ├── BPMS.Modules.Forms → Shared
  ├── BPMS.Modules.Notifications → Shared
  ├── BPMS.Modules.FileManagement → Shared
  ├── BPMS.Modules.WorkflowDefinitions → Shared
  └── BPMS.Modules.WorkflowRuntime → Shared, WorkflowDefinitions, Notifications, Identity, Forms
```

## API Counts (91 total endpoints)

| Module | Public | Authenticated | Permission-gated | Total |
|--------|--------|---------------|------------------|-------|
| Identity (Auth) | 3 | 0 | 0 | 3 |
| Identity (Users) | 0 | 3 | 6 | 9 |
| Identity (Roles) | 0 | 0 | 7 | 7 |
| Tenancy (Tenants) | 0 | 6 | 0 | 6 |
| Tenancy (Plans) | 1 | 1 | 0 | 2 |
| Tenancy (Subscriptions) | 0 | 2 | 0 | 2 |
| Tenancy (Settings/Lookups) | 0 | 0 | 6 | 6 |
| Forms | 0 | 9 | 0 | 9 |
| Form Categories | 0 | 4 | 0 | 4 |
| Workflow Categories | 0 | 0 | 4 | 4 |
| Workflows | 0 | 9 | 0 | 9 |
| Workflow Subscriptions | 0 | 0 | 3 | 3 |
| Admin Dashboard | 0 | 0 | 1 | 1 |
| Workflow Instances | 0 | 4 | 0 | 4 |
| Tasks | 0 | 5 | 0 | 5 |
| Inbox | 0 | 3 | 0 | 3 |
| Notifications | 0 | 5 | 0 | 5 |
| Audit Logs | 0 | 2 | 0 | 2 |
| Files | 0 | 7 | 0 | 7 |
| **TOTAL** | **4** | **60** | **27** | **91** |

## Coding Guidelines

| Rule | Value |
|------|-------|
| Language | C# 13 |
| Framework | .NET 10 |
| API Style | Minimal APIs |
| ORM | Entity Framework Core with Npgsql |
| File Storage | MinIO (S3-compatible) |
| Password Hashing | BCrypt.Net-Next |
| Logging | Serilog |
| Documentation | Swashbuckle (Swagger) |
| Health Checks | Built-in with DbContext check |
| Pattern | Module Interface + Implementation (no MediatR/CQRS) |
| Async | async/await throughout |
| Nullable | Enabled project-wide |
| Multi-tenancy | `ITenantEntity` + EF Core global query filters |
| Entity IDs | `Guid` via `BaseEntity` |
| Soft Deletes | `IsDeleted` bool + query filter |
| Auditing | `IAuditableEntity` auto-populated by `BpmsDbContext.SaveChangesAsync` |
| Error Handling | `KeyNotFoundException` → 404, other exceptions → 500 |