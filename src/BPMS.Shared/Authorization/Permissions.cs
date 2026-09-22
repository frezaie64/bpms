namespace BPMS.Shared.Authorization;

public static class Permissions
{
    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string Create = "Roles.Create";
        public const string Update = "Roles.Update";
        public const string Delete = "Roles.Delete";
    }

    public static class WorkflowSubscriptions
    {
        public const string Manage = "WorkflowSubscriptions.Manage";
    }

    public static class Files
    {
        public const string View = "Files.View";
        public const string Upload = "Files.Upload";
        public const string Download = "Files.Download";
        public const string Rename = "Files.Rename";
        public const string Delete = "Files.Delete";
    }

    public static class Tenants
    {
        public const string View = "Tenants.View";
        public const string Create = "Tenants.Create";
        public const string Manage = "Tenants.Manage";
    }

    public static class Administration
    {
        public const string ViewDashboard = "Administration.ViewDashboard";
        public const string ManageSettings = "Administration.ManageSettings";
        public const string ManageWorkflowCategories = "Administration.ManageWorkflowCategories";
        public const string ManageLookups = "Administration.ManageLookups";
    }

    public static class FormGenerator
    {
        public const string Generate = "FormGenerator.Generate";
        public const string View = "FormGenerator.View";
        public const string Submit = "FormGenerator.Submit";
        public const string Admin = "FormGenerator.Admin";
    }

    public static readonly string[] All =
    [
        Users.View, Users.Create, Users.Update, Users.Delete,
        Roles.View, Roles.Create, Roles.Update, Roles.Delete,
        WorkflowSubscriptions.Manage,
        Files.View, Files.Upload, Files.Download, Files.Rename, Files.Delete,
        Tenants.View, Tenants.Create, Tenants.Manage,
        Administration.ViewDashboard, Administration.ManageSettings,
        Administration.ManageWorkflowCategories, Administration.ManageLookups,
        FormGenerator.Generate, FormGenerator.View, FormGenerator.Submit, FormGenerator.Admin
    ];
}