CREATE TABLE MyTable (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OriginalFileName NVARCHAR(MAX) NOT NULL,
    RandomFileName NVARCHAR(MAX) NOT NULL
);