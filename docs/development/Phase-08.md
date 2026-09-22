# Phase 8 - File Management

## Goal

Implement file management using MinIO to allow users to upload, download, and manage files. Files can later be attached to forms, workflow instances, and tasks.

## Features

### File Upload

Support uploading files to MinIO.

Store only file metadata in PostgreSQL.

Supported file types:

* Documents
* Images
* PDFs
* Spreadsheets
* Archives

Validate:

* File size
* File extension
* Content type

### File Download

Users can download files they have permission to access.

### File Preview

Support preview for supported file types:

* Images
* PDF files

### File Management

Users can:

* Upload files
* Download files
* Rename files
* Delete files (soft delete)
* View file details

### File Metadata

Store:

* File Name
* Original File Name
* File Size
* Content Type
* Object Name (MinIO)
* Bucket Name
* Uploaded By
* Uploaded Date
* Tenant ID

### Buckets

Use a configurable bucket.

Do not create one bucket per tenant.

Use object paths such as:

* tenant-id/forms/
* tenant-id/workflows/
* tenant-id/tasks/
* tenant-id/general/

### Security

* Users can only access files belonging to their tenant.
* Prevent access to deleted files.
* Validate file ownership before download.

## APIs

### Files

* GET /api/files
* GET /api/files/{id}
* POST /api/files/upload
* GET /api/files/{id}/download
* PUT /api/files/{id}
* DELETE /api/files/{id}

## Tenant Rules

* Files belong to a tenant.
* Users can only access files from their own tenant.
* Tenant isolation must be enforced.

## Swagger

Document all APIs.

## Out of Scope

Do not implement:

* File versioning
* Image resizing
* Virus scanning
* OCR
* CDN integration
* Public file sharing

These features can be added in future phases.

## Definition of Done

* Files are uploaded to MinIO.
* File metadata is stored in PostgreSQL.
* Users can download files.
* Users can rename and delete files.
* Tenant isolation is enforced.
* Swagger is updated.
* `dotnet build` completes successfully without errors.
