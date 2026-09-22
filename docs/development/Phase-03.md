# Phase 3 - Form Builder

## Goal

Implement a dynamic form builder that allows tenant users to create, manage, publish, and reuse forms. Forms will be used later by workflow definitions and workflow execution.

## Features

### Form Management

* Create a form
* Update a form
* Delete a form (soft delete)
* Publish a form
* Archive a form
* List forms
* View form details
* Duplicate a form

### Form Builder

Users can visually design forms by adding, removing, and rearranging fields.

Supported field types:

* Text
* Text Area
* Number
* Email
* Phone
* Password
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

### Field Properties

Each field should support:

* Label
* Placeholder
* Description
* Required
* Read Only
* Default Value
* Help Text
* Order
* Width
* Validation Rules

### Validation

Support the following validations:

* Required
* Minimum Length
* Maximum Length
* Minimum Value
* Maximum Value
* Regular Expression
* Custom Error Message

### Form Preview

Users can preview a form before publishing.

### Form Versioning

* Create new versions when a published form is modified.
* Existing submissions should continue using the version they were created with.

### Form Categories

Support optional categories for organizing forms.

### Form Status

A form can have one of the following statuses:

* Draft
* Published
* Archived

## Data Storage

Store form definitions as JSON.

Each form should include:

* Name
* Description
* Category
* Version
* Status
* JSON Definition

Form submissions are not implemented in this phase.

## APIs

### Forms

* GET /api/forms
* GET /api/forms/{id}
* POST /api/forms
* PUT /api/forms/{id}
* DELETE /api/forms/{id}
* POST /api/forms/{id}/publish
* POST /api/forms/{id}/archive
* POST /api/forms/{id}/duplicate
* GET /api/forms/{id}/preview

## Tenant Rules

* Forms belong to a tenant.
* Users can only access forms from their own tenant.

## Swagger

Document all APIs.

## Out of Scope

Do not implement:

* Workflow integration
* Form submissions
* Form approval
* Workflow variables
* Dynamic field visibility
* Conditional logic
* Calculated fields

These features will be implemented in later phases.

## Definition of Done

* Users can create forms.
* Users can edit forms.
* Users can publish forms.
* Users can archive forms.
* Users can duplicate forms.
* Form definitions are stored as JSON.
* Form versioning works.
* Tenant isolation is enforced.
* Swagger is updated.
* `dotnet build` completes successfully without errors.
