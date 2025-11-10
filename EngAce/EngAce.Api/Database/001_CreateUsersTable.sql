-- ====================================
-- Users Table Migration for SQL Server
-- Created: 2025-11-09
-- Purpose: Authentication with Username/Password and OAuth (Google/Facebook)
-- ====================================

-- Drop table if exists (USE WITH CAUTION IN PRODUCTION!)
-- DROP TABLE IF EXISTS Users;

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        UserID INT PRIMARY KEY IDENTITY(1,1),
        Email NVARCHAR(255) NOT NULL,
        Username NVARCHAR(100) NULL,
        FullName NVARCHAR(255) NULL,
        PasswordHash NVARCHAR(255) NULL, -- NULL if OAuth-only user
        Phone NVARCHAR(20) NULL,
        Avatar NVARCHAR(500) NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'user', -- user, admin, moderator
        Status NVARCHAR(50) NOT NULL DEFAULT 'active', -- active, inactive, suspended
        EmailVerified BIT NOT NULL DEFAULT 0,
        
        -- OAuth fields (NULL if not using OAuth)
        GoogleID NVARCHAR(255) NULL,
        FacebookID NVARCHAR(255) NULL,
        
        -- Reset Password
        ResetToken NVARCHAR(255) NULL,
        ResetTokenExpires DATETIME2 NULL,
        
        -- Timestamps
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2 NULL,
        
        -- Unique constraints
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT UQ_Users_Username UNIQUE (Username),
        CONSTRAINT UQ_Users_GoogleID UNIQUE (GoogleID),
        CONSTRAINT UQ_Users_FacebookID UNIQUE (FacebookID)
    );

    -- Create indexes for performance
    CREATE NONCLUSTERED INDEX IX_Users_Email ON [dbo].[Users](Email);
    CREATE NONCLUSTERED INDEX IX_Users_Username ON [dbo].[Users](Username);
    CREATE NONCLUSTERED INDEX IX_Users_GoogleID ON [dbo].[Users](GoogleID);
    CREATE NONCLUSTERED INDEX IX_Users_FacebookID ON [dbo].[Users](FacebookID);
    CREATE NONCLUSTERED INDEX IX_Users_ResetToken ON [dbo].[Users](ResetToken);
END
GO

-- ====================================
-- Sample Data (Optional - for testing)
-- ====================================

-- Admin user (password: Admin@123)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE Email = 'admin@engace.com')
BEGIN
    INSERT INTO [dbo].[Users] (Email, Username, FullName, PasswordHash, Role, Status, EmailVerified, CreatedAt, UpdatedAt)
    VALUES (
        'admin@engace.com',
        'admin',
        'System Administrator',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5lkRw8fKGYZr6', -- Admin@123
        'admin',
        'active',
        1,
        GETUTCDATE(),
        GETUTCDATE()
    );
END
GO

-- Test user (password: Test@123)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE Email = 'test@example.com')
BEGIN
    INSERT INTO [dbo].[Users] (Email, Username, FullName, PasswordHash, Role, Status, EmailVerified, CreatedAt, UpdatedAt)
    VALUES (
        'test@example.com',
        'testuser',
        'Test User',
        '$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', -- Test@123
        'user',
        'active',
        1,
        GETUTCDATE(),
        GETUTCDATE()
    );
END
GO

-- ====================================
-- Verification Queries
-- ====================================

-- Check table structure
-- EXEC sp_help 'Users';

-- Count users
-- SELECT COUNT(*) as TotalUsers FROM Users;

-- View all users (without sensitive data)
-- SELECT UserID, Email, Username, FullName, Role, Status, EmailVerified, CreatedAt 
-- FROM Users;
