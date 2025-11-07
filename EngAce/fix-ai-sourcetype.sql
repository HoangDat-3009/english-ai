-- Fix SourceType for AI-generated exercises
-- Run this SQL script in SQL Server Management Studio or via dotnet ef

-- Update all exercises where SourceType is NULL or empty to 'manual' (default for old data)
UPDATE ReadingExercises
SET SourceType = 'manual'
WHERE SourceType IS NULL OR SourceType = '';

-- Update exercises created by AI (based on CreatedBy pattern)
-- Assuming AI-generated exercises have CreatedBy containing 'AI' or 'System'
UPDATE ReadingExercises
SET SourceType = 'ai'
WHERE (CreatedBy LIKE '%AI%' OR CreatedBy = 'System')
  AND SourceType != 'ai';

-- Verify the changes
SELECT 
    Id,
    Name,
    SourceType,
    CreatedBy,
    CreatedAt
FROM ReadingExercises
ORDER BY CreatedAt DESC;

-- Count by source type
SELECT 
    SourceType,
    COUNT(*) as Count
FROM ReadingExercises
GROUP BY SourceType;
