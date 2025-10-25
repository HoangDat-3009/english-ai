-- ======================================================================
-- BỔ SUNG CẤU TRÚC DATABASE CHO PROGRESS & LEADERBOARD FEATURES
-- ======================================================================

-- ==========================================================
-- 1. Cập nhật Achievement table để phù hợp với frontend
-- ==========================================================
ALTER TABLE `Achievement` 
ADD COLUMN `Level` INT DEFAULT 1,
ADD COLUMN `TotalXP` INT DEFAULT 0,
ADD COLUMN `WeeklyXP` INT DEFAULT 0,
ADD COLUMN `MonthlyXP` INT DEFAULT 0,
ADD COLUMN `StreakDays` INT DEFAULT 0,
ADD COLUMN `LastActivity` DATETIME DEFAULT CURRENT_TIMESTAMP,
ADD COLUMN `ExercisesCompleted` INT DEFAULT 0,
ADD COLUMN `LessonsCompleted` INT DEFAULT 0,
ADD COLUMN `WordsLearned` INT DEFAULT 0,
ADD COLUMN `ReadingScore` DECIMAL(5,2) DEFAULT 0,
ADD COLUMN `ListeningScore` DECIMAL(5,2) DEFAULT 0,
ADD COLUMN `GrammarScore` DECIMAL(5,2) DEFAULT 0,
ADD COLUMN `VocabularyScore` DECIMAL(5,2) DEFAULT 0,
ADD COLUMN `WeeklyRank` INT DEFAULT 0,
ADD COLUMN `MonthlyRank` INT DEFAULT 0,
MODIFY COLUMN `Progress` TEXT; -- Chứa JSON array achievements

-- ==========================================================
-- 2. Bảng LeaderboardEntry (cho data sync)
-- ==========================================================
CREATE TABLE `LeaderboardEntry` (
  `EntryID` INT AUTO_INCREMENT PRIMARY KEY,
  `UserID` INT NOT NULL,
  `Username` VARCHAR(50) NOT NULL,
  `Avatar` VARCHAR(255),
  `Level` INT DEFAULT 1,
  `TotalXP` INT DEFAULT 0,
  `WeeklyXP` INT DEFAULT 0,
  `MonthlyXP` INT DEFAULT 0,
  `CurrentRank` INT DEFAULT 0,
  `WeeklyRank` INT DEFAULT 0,
  `MonthlyRank` INT DEFAULT 0,
  `StreakDays` INT DEFAULT 0,
  `Badge` VARCHAR(10),
  `LastUpdated` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (`UserID`) REFERENCES `User`(`UserID`) ON DELETE CASCADE,
  INDEX idx_total_xp (`TotalXP` DESC),
  INDEX idx_weekly_xp (`WeeklyXP` DESC),
  INDEX idx_monthly_xp (`MonthlyXP` DESC)
) ENGINE=InnoDB;

-- ==========================================================
-- 3. Bảng UserProgress (cho Progress page data)
-- ==========================================================
CREATE TABLE `UserProgress` (
  `ProgressID` INT AUTO_INCREMENT PRIMARY KEY,
  `UserID` INT NOT NULL,
  `CurrentLevel` INT DEFAULT 1,
  `CurrentXP` INT DEFAULT 0,
  `XPToNextLevel` INT DEFAULT 1000,
  `ProgressPercentage` INT DEFAULT 0,
  `WeeklyGoal` INT DEFAULT 1000,
  `WeeklyProgress` INT DEFAULT 0,
  `MonthlyGoal` INT DEFAULT 4000,
  `MonthlyProgress` INT DEFAULT 0,
  `StreakDays` INT DEFAULT 0,
  `ExercisesThisWeek` INT DEFAULT 0,
  `LessonsThisMonth` INT DEFAULT 0,
  `WordsLearnedTotal` INT DEFAULT 0,
  `RecentAchievements` TEXT, -- JSON array
  `LastUpdated` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (`UserID`) REFERENCES `User`(`UserID`) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ==========================================================
-- 4. Cập nhật Exercise table cho Reading Exercises
-- ==========================================================
ALTER TABLE `Exercise`
ADD COLUMN `SourceType` ENUM('uploaded', 'ai') DEFAULT 'uploaded',
ADD COLUMN `Topic` VARCHAR(100),
ADD COLUMN `PartType` ENUM('Part 5', 'Part 6', 'Part 7'),
ADD COLUMN `Questions` TEXT, -- JSON array of questions
ADD COLUMN `CreatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP,
ADD COLUMN `UpdatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

-- ==========================================================
-- 5. Bảng ReadingExercise (specific cho TOEIC Reading)
-- ==========================================================
CREATE TABLE `ReadingExercise` (
  `ExerciseID` INT AUTO_INCREMENT PRIMARY KEY,
  `Name` VARCHAR(150) NOT NULL,
  `Content` TEXT NOT NULL, -- Reading passage
  `Level` ENUM('Beginner', 'Intermediate', 'Advanced') NOT NULL,
  `Type` ENUM('Part 5', 'Part 6', 'Part 7') NOT NULL,
  `SourceType` ENUM('uploaded', 'ai') DEFAULT 'uploaded',
  `Topic` VARCHAR(100),
  `Questions` JSON NOT NULL, -- Array of question objects
  `CreatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_level (`Level`),
  INDEX idx_type (`Type`),
  INDEX idx_source (`SourceType`)
) ENGINE=InnoDB;

-- ==========================================================
-- 6. Bảng UserResult (cho Reading Exercise results)
-- ==========================================================
CREATE TABLE `UserResult` (
  `ResultID` INT AUTO_INCREMENT PRIMARY KEY,
  `UserID` INT NOT NULL,
  `ExerciseID` INT NOT NULL,
  `Answers` JSON NOT NULL, -- Array of user answers
  `Score` INT NOT NULL,
  `TotalQuestions` INT NOT NULL,
  `TimeSpent` INT, -- seconds
  `CompletedAt` DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (`UserID`) REFERENCES `User`(`UserID`) ON DELETE CASCADE,
  FOREIGN KEY (`ExerciseID`) REFERENCES `ReadingExercise`(`ExerciseID`) ON DELETE CASCADE,
  INDEX idx_user_score (`UserID`, `Score` DESC),
  INDEX idx_completed (`CompletedAt` DESC)
) ENGINE=InnoDB;

-- ==========================================================
-- 7. INSERT SAMPLE DATA để phù hợp với frontend mock data
-- ==========================================================

-- Cập nhật Achievement data cho 7 users trong frontend
UPDATE `Achievement` SET 
  `Level` = 12, `TotalXP` = 15750, `WeeklyXP` = 890, `MonthlyXP` = 3240,
  `StreakDays` = 23, `ReadingScore` = 480.00, `ListeningScore` = 195.00,
  `GrammarScore` = 190.00, `VocabularyScore` = 85.00,
  `Ranking` = 1, `WeeklyRank` = 2, `MonthlyRank` = 1,
  `Progress` = '["First Steps", "Week Warrior", "Grammar Master", "Reading Pro", "TOEIC Expert"]'
WHERE `UserID` = 2;

-- Insert thêm users để match với frontend (NguyenVanA, TranThiB, etc.)
INSERT INTO `User` (`Username`, `PasswordHash`, `Email`, `Role`) VALUES
('NguyenVanA', 'hash_nguyenvana', 'nguyenvana@example.com', 'student'),
('TranThiB', 'hash_tranthib', 'tranthib@example.com', 'student'), 
('LeVanC', 'hash_levanc', 'levanc@example.com', 'student'),
('englishlearner01', 'hash_english01', 'english01@example.com', 'student'),
('PhamThiD', 'hash_phamthid', 'phamthid@example.com', 'student'),
('VuThiF', 'hash_vuthif', 'vuthif@example.com', 'student'),
('DangVanG', 'hash_dangvang', 'dangvang@example.com', 'student');

-- Insert LeaderboardEntry data
INSERT INTO `LeaderboardEntry` (`UserID`, `Username`, `Avatar`, `Level`, `TotalXP`, `WeeklyXP`, `MonthlyXP`, `CurrentRank`, `WeeklyRank`, `MonthlyRank`, `StreakDays`, `Badge`) VALUES
(5, 'NguyenVanA', 'https://images.unsplash.com/photo-1494790108755-2616b612b56c?w=100&h=100&fit=crop&crop=face', 12, 15750, 890, 3240, 1, 2, 1, 23, '🏆'),
(6, 'TranThiB', 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100&h=100&fit=crop&crop=face', 11, 14200, 920, 2980, 2, 1, 2, 18, '🥈'),
(7, 'LeVanC', 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=100&h=100&fit=crop&crop=face', 10, 12800, 750, 2650, 3, 3, 3, 15, '🥉'),
(8, 'englishlearner01', NULL, 7, 8200, 620, 1850, 4, 5, 6, 7, '🚀'),
(9, 'PhamThiD', NULL, 8, 9850, 590, 1980, 5, 6, 5, 9, '⭐'),
(10, 'VuThiF', NULL, 6, 7100, 480, 1620, 6, 7, 7, 5, '💪'),
(11, 'DangVanG', NULL, 5, 6200, 420, 1400, 7, 8, 8, 3, '📚');

-- Insert UserProgress data cho current user (englishlearner01)
INSERT INTO `UserProgress` (`UserID`, `CurrentLevel`, `CurrentXP`, `XPToNextLevel`, `ProgressPercentage`, `WeeklyGoal`, `WeeklyProgress`, `MonthlyGoal`, `MonthlyProgress`, `StreakDays`, `ExercisesThisWeek`, `LessonsThisMonth`, `WordsLearnedTotal`, `RecentAchievements`) VALUES
(8, 7, 200, 800, 20, 1000, 620, 4000, 1850, 7, 12, 9, 820, '["First Steps", "Week Warrior", "Grammar Master"]');

-- Insert sample ReadingExercise data
INSERT INTO `ReadingExercise` (`Name`, `Content`, `Level`, `Type`, `SourceType`, `Topic`, `Questions`) VALUES
('TOEIC Reading Practice - Business Email', 
'From: manager@company.com\nTo: all-staff@company.com\nSubject: Quarterly Meeting\n\nDear Team,\n\nI am writing to inform you about our upcoming quarterly meeting scheduled for next Friday at 2 PM in Conference Room A. Please prepare your departmental reports and bring any questions you may have about the new project timeline.\n\nBest regards,\nSarah Manager', 
'Intermediate', 'Part 7', 'uploaded', 'Business Communication',
'[
  {
    "question": "What is the purpose of this email?",
    "options": ["To schedule a meeting", "To request reports", "To announce a promotion", "To cancel an event"],
    "correctAnswer": 0,
    "explanation": "The email clearly states it is to inform about the upcoming quarterly meeting."
  },
  {
    "question": "When is the meeting scheduled?",
    "options": ["This Friday at 2 PM", "Next Friday at 2 PM", "Next Monday at 2 PM", "This Monday at 2 PM"],
    "correctAnswer": 1,
    "explanation": "The email mentions next Friday at 2 PM."
  },
  {
    "question": "Where will the meeting take place?",
    "options": ["Conference Room B", "Conference Room A", "Main Hall", "Manager office"],
    "correctAnswer": 1,
    "explanation": "Conference Room A is mentioned as the location."
  }
]');

INSERT INTO `ReadingExercise` (`Name`, `Content`, `Level`, `Type`, `SourceType`, `Topic`, `Questions`) VALUES
('Grammar Practice - Verb Tenses',
'Choose the correct answer to complete the sentence.',
'Beginner', 'Part 5', 'ai', 'Grammar',
'[
  {
    "question": "The company _____ a new product next month.",
    "options": ["launch", "launches", "will launch", "launching"],
    "correctAnswer": 2,
    "explanation": "Future tense is needed with next month."
  },
  {
    "question": "She _____ in this office for five years.",
    "options": ["work", "works", "has worked", "working"],
    "correctAnswer": 2,
    "explanation": "Present perfect is used for duration with for."
  }
]');

-- ==========================================================
-- 8. CREATE VIEW cho API endpoints
-- ==========================================================

-- View cho leaderboard với time filtering
CREATE VIEW `LeaderboardView` AS
SELECT 
  l.UserID,
  l.Username,
  l.Avatar,
  l.Level,
  l.TotalXP,
  l.WeeklyXP,
  l.MonthlyXP,
  l.CurrentRank,
  l.WeeklyRank,
  l.MonthlyRank,
  l.StreakDays,
  l.Badge,
  u.Status,
  up.FullName
FROM `LeaderboardEntry` l
JOIN `User` u ON l.UserID = u.UserID
LEFT JOIN `UserProfile` up ON l.UserID = up.UserID
WHERE u.Status = 'active'
ORDER BY l.TotalXP DESC;

-- View cho user stats
CREATE VIEW `UserStatsView` AS
SELECT 
  u.UserID,
  u.Username,
  up.FullName,
  a.Level,
  a.TotalXP,
  a.WeeklyXP,
  a.MonthlyXP,
  a.StreakDays,
  a.LastActivity,
  a.ExercisesCompleted,
  a.LessonsCompleted,
  a.WordsLearned,
  a.ReadingScore,
  a.ListeningScore,
  a.GrammarScore,
  a.VocabularyScore,
  a.Progress,
  a.Ranking AS CurrentRank,
  a.WeeklyRank,
  a.MonthlyRank
FROM `User` u
LEFT JOIN `Achievement` a ON u.UserID = a.UserID
LEFT JOIN `UserProfile` up ON u.UserID = up.UserID
WHERE u.Status = 'active';

-- ==========================================================
-- 9. PROCEDURES cho API calls (optional - có thể dùng trong backend)
-- ==========================================================

DELIMITER $$

CREATE PROCEDURE GetLeaderboard(IN timeFilter VARCHAR(20), IN limitCount INT)
BEGIN
  IF timeFilter = 'weekly' THEN
    SELECT * FROM LeaderboardView ORDER BY WeeklyXP DESC LIMIT limitCount;
  ELSEIF timeFilter = 'monthly' THEN  
    SELECT * FROM LeaderboardView ORDER BY MonthlyXP DESC LIMIT limitCount;
  ELSE
    SELECT * FROM LeaderboardView ORDER BY TotalXP DESC LIMIT limitCount;
  END IF;
END$$

CREATE PROCEDURE GetUserProgress(IN userId INT)
BEGIN
  SELECT 
    p.*,
    u.Username,
    up.FullName
  FROM UserProgress p
  JOIN User u ON p.UserID = u.UserID
  LEFT JOIN UserProfile up ON p.UserID = up.UserID
  WHERE p.UserID = userId;
END$$

CREATE PROCEDURE UpdateUserXP(IN userId INT, IN xpGained INT)
BEGIN
  DECLARE currentXP INT DEFAULT 0;
  DECLARE currentLevel INT DEFAULT 1;
  DECLARE newLevel INT DEFAULT 1;
  
  -- Get current XP and level
  SELECT TotalXP, Level INTO currentXP, currentLevel 
  FROM Achievement WHERE UserID = userId;
  
  -- Calculate new level (every 1000 XP = 1 level)
  SET newLevel = FLOOR((currentXP + xpGained) / 1000) + 1;
  
  -- Update Achievement
  UPDATE Achievement SET
    TotalXP = TotalXP + xpGained,
    WeeklyXP = WeeklyXP + xpGained,
    MonthlyXP = MonthlyXP + xpGained,
    Level = newLevel,
    LastActivity = NOW()
  WHERE UserID = userId;
  
  -- Update LeaderboardEntry
  UPDATE LeaderboardEntry SET
    TotalXP = TotalXP + xpGained,
    WeeklyXP = WeeklyXP + xpGained,
    MonthlyXP = MonthlyXP + xpGained,
    Level = newLevel
  WHERE UserID = userId;
  
  -- Update UserProgress
  UPDATE UserProgress SET
    CurrentXP = (currentXP + xpGained) MOD 1000,
    XPToNextLevel = 1000 - ((currentXP + xpGained) MOD 1000),
    CurrentLevel = newLevel,
    ProgressPercentage = ROUND(((currentXP + xpGained) MOD 1000) / 1000 * 100),
    WeeklyProgress = WeeklyProgress + xpGained,
    MonthlyProgress = MonthlyProgress + xpGained
  WHERE UserID = userId;
END$$

DELIMITER ;