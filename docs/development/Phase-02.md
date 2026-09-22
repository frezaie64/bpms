# Phase 2 - User & Role Management

## Goal

Implement user management and role-based access control (RBAC) for the BPMS. All users and roles must belong to the current tenant.

## Features

### User Management

* View current user profile
* Update current user profile
* Change password
* List users in the current tenant
* Get user details
* Create new users
* Update users
* Activate or deactivate users
* Delete users (soft delete)

### Role Management

* Create roles
* Update roles
* Delete roles
* List roles
* Get role details
* Assign roles to users
* Remove roles from users

### Permissions

Implement permission-based authorization.

Default permissions:

#### Users

* Users.View
* Users.Create
* Users.Update
* Users.Delete

#### Roles

* Roles.View
* Roles.Create
* Roles.Update
* Roles.Delete

Users can have multiple roles.

Roles can have multiple permissions.

Authorization should support both roles and permissions.

## Tenant Rules

* Users can only access data from their own tenant.
* Roles belong to a tenant.
* Administrators cannot manage another tenant's users or roles.

## Database

Create the following tables if they do not already exist:

* Roles
* Permissions
* UserRoles
* RolePermissions

Update existing tables if needed.

## API Endpoints

### Users

* GET /api/users
* GET /api/users/{id}
* GET /api/users/me
* POST /api/users
* PUT /api/users/{id}
* DELETE /api/users/{id}
* PUT /api/users/{id}/status
* POST /api/users/change-password

### Roles

* GET /api/roles
* GET /api/roles/{id}
* POST /api/roles
* PUT /api/roles/{id}
* DELETE /api/roles/{id}
* POST /api/roles/{id}/users
* DELETE /api/roles/{id}/users/{userId}

## Validation

* Email addresses must be unique within a tenant.
* Role names must be unique within a tenant.
* Passwords must follow the password policy from Phase 1.
* Prevent deleting the last administrator of a tenant.

## Swagger

Document all APIs.

## Definition of Done

* User management works.
* Role management works.
* Role assignment works.
* Permission-based authorization works.
* Tenant isolation is enforced.
* All endpoints are secured.
* Swagger is updated.
* `dotnet build` completes successfully without errors.
