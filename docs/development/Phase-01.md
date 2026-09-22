# Phase 1 - Foundation

## Goal

Create the foundation of the BPMS.

## Tech Stack

- ASP.NET Core 10 Web API
- PostgreSQL
- Entity Framework Core
- MinIO
- JWT Authentication
- Modular Monolith

## Modules

- Identity
- Tenancy

## Features

- Modular Monolith structure
- JWT Authentication
- Register
- Login
- Refresh Token
- Tenant Management
- Shared database multi-tenancy
- Global query filters
- Swagger
- Serilog
- Health Checks
- Docker Compose

## APIs

### Authentication

- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/refresh

### Tenants

- POST /api/tenants
- GET /api/tenants

## Definition of Done

- Project builds successfully
- Authentication works
- Tenant isolation works
- PostgreSQL works
- Docker runs successfully