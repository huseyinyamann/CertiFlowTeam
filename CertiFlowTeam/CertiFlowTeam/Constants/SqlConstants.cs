using CertiFlowTeam.Enums;

namespace CertiFlowTeam.Constants
{
    public static class SqlConstants
    {
        #region Table Names

        public static class TableNames
        {
            public const string Users = "Users";
            public const string Settings = "Settings";
            public const string Documents = "Documents";
            public const string DocumentLogs = "DocumentLogs";
            public const string Companies = "Companies";
        }

        #endregion

        #region Create Table Scripts

        public static class CreateTableScripts
        {
            public static string Users => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.Users}' AND xtype='U')
CREATE TABLE {TableNames.Users} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Role INT NOT NULL DEFAULT {(int)Role.User},
    CompanyId INT,
    Phone NVARCHAR(20),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    LastLoginDate DATETIME2,
    UpdatedDate DATETIME2,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (CompanyId) REFERENCES {TableNames.Companies}(Id)
);";

            public static string Companies => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.Companies}' AND xtype='U')
CREATE TABLE {TableNames.Companies} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName NVARCHAR(255) NOT NULL,
    TaxNumber NVARCHAR(50),
    Address NVARCHAR(500),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    AuthorizedPerson NVARCHAR(100),
    AuthorizedPhone NVARCHAR(20),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    UpdatedDate DATETIME2,
    IsDeleted BIT DEFAULT 0
);";

            public static string Documents => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.Documents}' AND xtype='U')
CREATE TABLE {TableNames.Documents} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DocumentName NVARCHAR(255) NOT NULL,
    DocumentType NVARCHAR(100),
    DocumentNumber NVARCHAR(50),
    FilePath NVARCHAR(500) NOT NULL,
    FileSize BIGINT,
    Description NVARCHAR(1000),
    ApprovalStatus INT NOT NULL DEFAULT {(int)DocumentApprovalStatus.Pending},
    UploadedByUserId INT NOT NULL,
    AssignedToUserId INT,
    ApprovedByUserId INT,
    ApprovalDate DATETIME2,
    RejectionReason NVARCHAR(500),
    CompanyId INT,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    UpdatedDate DATETIME2,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (UploadedByUserId) REFERENCES {TableNames.Users}(Id),
    FOREIGN KEY (AssignedToUserId) REFERENCES {TableNames.Users}(Id),
    FOREIGN KEY (ApprovedByUserId) REFERENCES {TableNames.Users}(Id),
    FOREIGN KEY (CompanyId) REFERENCES {TableNames.Companies}(Id)
);";

            public static string DocumentLogs => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.DocumentLogs}' AND xtype='U')
CREATE TABLE {TableNames.DocumentLogs} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DocumentId INT NOT NULL,
    UserId INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    OldStatus INT,
    NewStatus INT,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    IPAddress NVARCHAR(45),
    ActionDate DATETIME2 DEFAULT GETDATE(),
    IsSuccess BIT DEFAULT 1,
    ErrorMessage NVARCHAR(1000),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (DocumentId) REFERENCES {TableNames.Documents}(Id),
    FOREIGN KEY (UserId) REFERENCES {TableNames.Users}(Id)
);";

            public static string Settings => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.Settings}' AND xtype='U')
CREATE TABLE {TableNames.Settings} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(50) UNIQUE NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(255) NOT NULL,
    DefaultValue NVARCHAR(255) NOT NULL,
    DataType NVARCHAR(20) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    IsSystemSetting BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    UpdatedDate DATETIME2
);

IF NOT EXISTS (SELECT * FROM {TableNames.Settings} WHERE SettingKey = 'max_file_size_mb')
INSERT INTO {TableNames.Settings} (SettingKey, DisplayName, SettingValue, DefaultValue, DataType, Description, IsSystemSetting)
VALUES ('max_file_size_mb', 'Maximum File Size (MB)', '50', '50', 'int', 'Maximum file size that can be uploaded (in MB)', 1);

IF NOT EXISTS (SELECT * FROM {TableNames.Settings} WHERE SettingKey = 'allowed_file_types')
INSERT INTO {TableNames.Settings} (SettingKey, DisplayName, SettingValue, DefaultValue, DataType, Description, IsSystemSetting)
VALUES ('allowed_file_types', 'Allowed File Types', '.pdf,.doc,.docx,.xls,.xlsx,.jpg,.png', '.pdf,.doc,.docx,.xls,.xlsx,.jpg,.png', 'string', 'Allowed file extensions (comma separated)', 1);

IF NOT EXISTS (SELECT * FROM {TableNames.Settings} WHERE SettingKey = 'auto_approval_enabled')
INSERT INTO {TableNames.Settings} (SettingKey, DisplayName, SettingValue, DefaultValue, DataType, Description, IsSystemSetting)
VALUES ('auto_approval_enabled', 'Auto Approval Enabled', '0', '0', 'bit', 'Enable automatic approval under certain conditions', 1);";
        }

        #endregion

        #region Table Creation Order

        public static readonly string[] TableCreationOrder = {
            TableNames.Companies,
            TableNames.Users,
            TableNames.Documents,
            TableNames.DocumentLogs,
            TableNames.Settings
        };

        #endregion

        #region Table Existence Check

        public static string GetTableExistsScript(string tableName) =>
            $"SELECT COUNT(*) FROM sysobjects WHERE name='{tableName}' AND xtype='U'";

        #endregion

        #region Column Check Scripts

        public static string GetColumnExistsScript(string tableName, string columnName) =>
            $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";

        #endregion

        #region Migration Scripts

        public static class MigrationScripts
        {
            public static string AddDocumentTypeToDocuments => $@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{TableNames.Documents}' AND COLUMN_NAME = 'DocumentType')
BEGIN
    ALTER TABLE {TableNames.Documents} ADD DocumentType NVARCHAR(100) NULL;
    PRINT 'DocumentType column added';
END
ELSE
BEGIN
    PRINT 'DocumentType column already exists';
END";
        }

        #endregion

        #region Log Actions

        public static class LogActions
        {
            public const string DocumentUploaded = "DocumentUploaded";
            public const string DocumentUpdated = "DocumentUpdated";
            public const string DocumentDeleted = "DocumentDeleted";
            public const string DocumentApproved = "DocumentApproved";
            public const string DocumentRejected = "DocumentRejected";
            public const string DocumentCancelled = "DocumentCancelled";
            public const string UserLogin = "UserLogin";
            public const string UserLogout = "UserLogout";
            public const string UserCreated = "UserCreated";
            public const string UserUpdated = "UserUpdated";
            public const string UserDeleted = "UserDeleted";
            public const string CompanyCreated = "CompanyCreated";
            public const string CompanyUpdated = "CompanyUpdated";
            public const string CompanyDeleted = "CompanyDeleted";
            public const string DatabaseChecked = "DatabaseChecked";
            public const string DatabaseCreated = "DatabaseCreated";
        }

        #endregion
    }
}
