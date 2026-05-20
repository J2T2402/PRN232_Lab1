IF DB_ID(N'PRN232_LMS') IS NULL
BEGIN
    CREATE DATABASE [PRN232_LMS];
END;
GO

USE [PRN232_LMS];
GO

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory]
    (
        [MigrationId] NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF OBJECT_ID(N'dbo.Semesters', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Semesters]
    (
        [SemesterId] INT IDENTITY(1, 1) NOT NULL,
        [SemesterName] NVARCHAR(100) NOT NULL,
        [StartDate] DATETIME2 NOT NULL,
        [EndDate] DATETIME2 NOT NULL,
        CONSTRAINT [PK_Semesters] PRIMARY KEY ([SemesterId])
    );
END;
GO

IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Students]
    (
        [StudentId] INT IDENTITY(1, 1) NOT NULL,
        [FullName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(100) NOT NULL,
        [DateOfBirth] DATETIME2 NOT NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([StudentId])
    );
END;
GO

IF OBJECT_ID(N'dbo.Subjects', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Subjects]
    (
        [SubjectId] INT IDENTITY(1, 1) NOT NULL,
        [SubjectCode] NVARCHAR(20) NOT NULL,
        [SubjectName] NVARCHAR(100) NOT NULL,
        [Credit] INT NOT NULL,
        CONSTRAINT [PK_Subjects] PRIMARY KEY ([SubjectId])
    );
END;
GO

IF OBJECT_ID(N'dbo.Courses', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Courses]
    (
        [CourseId] INT IDENTITY(1, 1) NOT NULL,
        [CourseName] NVARCHAR(100) NOT NULL,
        [SemesterId] INT NOT NULL,
        [SubjectId] INT NOT NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([CourseId]),
        CONSTRAINT [FK_Courses_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [dbo].[Semesters]([SemesterId]),
        CONSTRAINT [FK_Courses_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([SubjectId])
    );
END;
GO

IF OBJECT_ID(N'dbo.Enrollments', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Enrollments]
    (
        [EnrollmentId] INT IDENTITY(1, 1) NOT NULL,
        [StudentId] INT NOT NULL,
        [CourseId] INT NOT NULL,
        [EnrollDate] DATETIME2 NOT NULL,
        [Status] NVARCHAR(20) NOT NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([EnrollmentId]),
        CONSTRAINT [FK_Enrollments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([StudentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Enrollments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses]([CourseId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Courses_SemesterId' AND object_id = OBJECT_ID(N'dbo.Courses'))
BEGIN
    CREATE INDEX [IX_Courses_SemesterId] ON [dbo].[Courses]([SemesterId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Courses_SubjectId' AND object_id = OBJECT_ID(N'dbo.Courses'))
BEGIN
    CREATE INDEX [IX_Courses_SubjectId] ON [dbo].[Courses]([SubjectId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Enrollments_CourseId' AND object_id = OBJECT_ID(N'dbo.Enrollments'))
BEGIN
    CREATE INDEX [IX_Enrollments_CourseId] ON [dbo].[Enrollments]([CourseId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Enrollments_StudentId_CourseId' AND object_id = OBJECT_ID(N'dbo.Enrollments'))
BEGIN
    CREATE UNIQUE INDEX [IX_Enrollments_StudentId_CourseId] ON [dbo].[Enrollments]([StudentId], [CourseId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Students_Email' AND object_id = OBJECT_ID(N'dbo.Students'))
BEGIN
    CREATE UNIQUE INDEX [IX_Students_Email] ON [dbo].[Students]([Email]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Subjects_SubjectCode' AND object_id = OBJECT_ID(N'dbo.Subjects'))
BEGIN
    CREATE UNIQUE INDEX [IX_Subjects_SubjectCode] ON [dbo].[Subjects]([SubjectCode]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260518030502_InitialCreate')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260518030502_InitialCreate', N'8.0.16');
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Semesters])
BEGIN
    INSERT INTO [dbo].[Semesters] ([SemesterName], [StartDate], [EndDate])
    VALUES
        (N'Spring 2024', CAST('2024-01-08T00:00:00' AS DATETIME2), CAST('2024-05-15T00:00:00' AS DATETIME2)),
        (N'Summer 2024', CAST('2024-05-20T00:00:00' AS DATETIME2), CAST('2024-08-25T00:00:00' AS DATETIME2)),
        (N'Fall 2024', CAST('2024-09-02T00:00:00' AS DATETIME2), CAST('2024-12-20T00:00:00' AS DATETIME2)),
        (N'Spring 2025', CAST('2025-01-06T00:00:00' AS DATETIME2), CAST('2025-05-12T00:00:00' AS DATETIME2)),
        (N'Summer 2025', CAST('2025-05-19T00:00:00' AS DATETIME2), CAST('2025-08-22T00:00:00' AS DATETIME2));
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Subjects])
BEGIN
    INSERT INTO [dbo].[Subjects] ([SubjectCode], [SubjectName], [Credit])
    VALUES
        (N'SUB001', N'Subject 1', 3),
        (N'SUB002', N'Subject 2', 4),
        (N'SUB003', N'Subject 3', 2),
        (N'SUB004', N'Subject 4', 3),
        (N'SUB005', N'Subject 5', 4),
        (N'SUB006', N'Subject 6', 2),
        (N'SUB007', N'Subject 7', 3),
        (N'SUB008', N'Subject 8', 4),
        (N'SUB009', N'Subject 9', 2),
        (N'SUB010', N'Subject 10', 3);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Courses])
BEGIN
    ;WITH SemesterIds AS
    (
        SELECT [SemesterId], ROW_NUMBER() OVER (ORDER BY [SemesterId]) - 1 AS [RowIndex]
        FROM [dbo].[Semesters]
    ),
    SubjectIds AS
    (
        SELECT [SubjectId], ROW_NUMBER() OVER (ORDER BY [SubjectId]) - 1 AS [RowIndex]
        FROM [dbo].[Subjects]
    ),
    Numbers AS
    (
        SELECT TOP (20) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS [Number]
        FROM sys.all_objects
    )
    INSERT INTO [dbo].[Courses] ([CourseName], [SemesterId], [SubjectId])
    SELECT
        CONCAT(N'Course ', [n].[Number]),
        [s].[SemesterId],
        [sub].[SubjectId]
    FROM Numbers AS [n]
    INNER JOIN SemesterIds AS [s] ON [s].[RowIndex] = ([n].[Number] - 1) % (SELECT COUNT(*) FROM SemesterIds)
    INNER JOIN SubjectIds AS [sub] ON [sub].[RowIndex] = ([n].[Number] - 1) % (SELECT COUNT(*) FROM SubjectIds)
    ORDER BY [n].[Number];
END;
GO

IF (SELECT COUNT(*) FROM [dbo].[Students]) < 50
BEGIN
    DECLARE @FirstNames TABLE ([Idx] INT PRIMARY KEY, [Name] NVARCHAR(20) NOT NULL);
    DECLARE @LastNames TABLE ([Idx] INT PRIMARY KEY, [Name] NVARCHAR(20) NOT NULL);

    INSERT INTO @FirstNames ([Idx], [Name])
    VALUES
        (0, N'Nguyen'), (1, N'Tran'), (2, N'Le'), (3, N'Pham'), (4, N'Hoang'),
        (5, N'Vo'), (6, N'Dang'), (7, N'Bui'), (8, N'Do'), (9, N'Huynh');

    INSERT INTO @LastNames ([Idx], [Name])
    VALUES
        (0, N'An'), (1, N'Binh'), (2, N'Chi'), (3, N'Dung'), (4, N'Giang'),
        (5, N'Hanh'), (6, N'Khanh'), (7, N'Linh'), (8, N'Minh'), (9, N'Phuong');

    ;WITH Numbers AS
    (
        SELECT TOP (50) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS [Number]
        FROM sys.all_objects
    )
    INSERT INTO [dbo].[Students] ([FullName], [Email], [DateOfBirth])
    SELECT
        CONCAT([f].[Name], N' ', [l].[Name], N' ', [n].[Number]),
        CONCAT(N'student', RIGHT(CONCAT(N'000', [n].[Number]), 3), N'@prn232.local'),
        DATEADD(DAY, [n].[Number] * 37, CAST('2000-01-01T00:00:00' AS DATETIME2))
    FROM Numbers AS [n]
    INNER JOIN @FirstNames AS [f] ON [f].[Idx] = ([n].[Number] - 1) % 10
    INNER JOIN @LastNames AS [l] ON [l].[Idx] = ([n].[Number] - 1) % 10
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[Students] AS [s]
        WHERE [s].[Email] = CONCAT(N'student', RIGHT(CONCAT(N'000', [n].[Number]), 3), N'@prn232.local')
    );
END;
GO

DECLARE @NeededEnrollments INT = 500 - (SELECT COUNT(*) FROM [dbo].[Enrollments]);

IF @NeededEnrollments > 0
BEGIN
    ;WITH StudentIds AS
    (
        SELECT [StudentId], ROW_NUMBER() OVER (ORDER BY [StudentId]) - 1 AS [StudentIndex]
        FROM [dbo].[Students]
    ),
    CourseIds AS
    (
        SELECT [CourseId], ROW_NUMBER() OVER (ORDER BY [CourseId]) - 1 AS [CourseIndex]
        FROM [dbo].[Courses]
    ),
    BasePairs AS
    (
        SELECT
            [s].[StudentId],
            [target].[CourseId],
            ROW_NUMBER() OVER (ORDER BY [s].[StudentIndex], [c].[CourseIndex]) AS [Seq]
        FROM StudentIds AS [s]
        CROSS JOIN CourseIds AS [c]
        INNER JOIN CourseIds AS [target]
            ON [target].[CourseIndex] = ([s].[StudentIndex] + [c].[CourseIndex]) % (SELECT COUNT(*) FROM CourseIds)
    ),
    CandidateEnrollments AS
    (
        SELECT
            [Seq],
            [StudentId],
            [CourseId],
            DATEADD(DAY, (([Seq] - 1) * 37) % 500, CAST('2024-01-01T00:00:00' AS DATETIME2)) AS [EnrollDate],
            CASE [Seq] % 3
                WHEN 1 THEN N'Active'
                WHEN 2 THEN N'Completed'
                ELSE N'Dropped'
            END AS [Status]
        FROM BasePairs
    )
    INSERT INTO [dbo].[Enrollments] ([StudentId], [CourseId], [EnrollDate], [Status])
    SELECT TOP (@NeededEnrollments)
        [ce].[StudentId],
        [ce].[CourseId],
        [ce].[EnrollDate],
        [ce].[Status]
    FROM CandidateEnrollments AS [ce]
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[Enrollments] AS [e]
        WHERE [e].[StudentId] = [ce].[StudentId]
          AND [e].[CourseId] = [ce].[CourseId]
    )
    ORDER BY [ce].[Seq];
END;
GO