-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Máy chủ: 127.0.0.1
-- Thời gian đã tạo: Th12 07, 2025 lúc 06:35 PM
-- Phiên bản máy phục vụ: 10.4.32-MariaDB
-- Phiên bản PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

DROP DATABASE IF EXISTS english_mentor_buddy;
CREATE DATABASE english_mentor_buddy CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE english_mentor_buddy;

--
-- Cơ sở dữ liệu: `english_mentor_buddy_db`
--

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `dictionary_entries`
--

CREATE TABLE `dictionary_entries` (
  `id` int(11) NOT NULL,
  `word` varchar(100) NOT NULL,
  `meaning` text NOT NULL,
  `example` text DEFAULT NULL,
  `type` varchar(50) DEFAULT NULL,
  `audio_url` varchar(255) DEFAULT NULL,
  `created_at` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `exercises`
--

CREATE TABLE `exercises` (
  `id` int(11) NOT NULL,
  `title` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `content` text DEFAULT NULL,
  `questions_json` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL CHECK (json_valid(`questions_json`)),
  `correct_answers_json` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL CHECK (json_valid(`correct_answers_json`)),
  `level` varchar(50) DEFAULT NULL,
  `type` varchar(50) DEFAULT NULL,
  `category` varchar(50) DEFAULT NULL,
  `estimated_minutes` int(11) DEFAULT NULL,
  `time_limit` int(11) DEFAULT NULL,
  `audio_url` varchar(255) DEFAULT NULL,
  `original_file_name` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `description` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `source_type` varchar(50) DEFAULT NULL,
  `created_by` int(11) DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT 1,
  `created_at` datetime DEFAULT current_timestamp(),
  `updated_at` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `ai_generated` tinyint(1) DEFAULT 0 COMMENT 'Bài tập có được AI tạo không (1=AI, 0=Admin upload)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `exercises`
--

INSERT INTO `exercises` (`id`, `title`, `content`, `questions_json`, `correct_answers_json`, `level`, `type`, `category`, `estimated_minutes`, `time_limit`, `audio_url`, `original_file_name`, `description`, `source_type`, `created_by`, `is_active`, `created_at`, `updated_at`, `ai_generated`) VALUES
(1, 'A1 Vocabulary - Animals', NULL, '[{\"q\":\"Cat?\",\"options\":[\"Mèo\",\"Chó\",\"Gà\"]}]', '[\"A\"]', 'A1', 'vocabulary', 'animals', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0),
(2, 'A2 Vocabulary - Jobs', NULL, '[{\"q\":\"Doctor?\",\"options\":[\"Bác sĩ\",\"Giáo viên\",\"Nông dân\"]}]', '[\"A\"]', 'A2', 'vocabulary', 'jobs', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0),
(3, 'A1 Grammar - Present', NULL, '[{\"q\":\"He ___ football.\",\"options\":[\"play\",\"plays\",\"playing\"]}]', '[\"B\"]', 'A1', 'grammar', 'tense', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0),
(4, 'Reading Easy A1', NULL, '[{\"q\":\"Đoạn văn A1\"}]', '[\"C\"]', 'A1', 'reading', 'short', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0),
(5, 'Listening A2 Short', NULL, '[{\"q\":\"Nghe chọn\",\"audio\":\"a1.mp3\"}]', '[\"B\"]', 'A2', 'listening', 'short', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0),
(6, 'Vocabulary - Food', NULL, '[{\"q\":\"Apple?\",\"options\":[\"Táo\",\"Chuối\",\"Bưởi\"]}]', '[\"A\"]', 'A1', 'vocabulary', 'food', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0),
(7, 'Vocabulary - House', NULL, '[{\"q\":\"Table?\",\"options\":[\"Bàn\",\"Ghế\",\"Giường\"]}]', '[\"A\"]', 'A1', 'vocabulary', 'house', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0),
(8, 'Grammar Past Simple', NULL, '[{\"q\":\"He ___ yesterday.\",\"options\":[\"go\",\"went\",\"goed\"]}]', '[\"B\"]', 'A2', 'grammar', 'tense', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0),
(9, 'Listening Basic', NULL, '[{\"q\":\"Nghe A1\",\"audio\":\"a2.mp3\"}]', '[\"C\"]', 'A1', 'listening', 'basic', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0),
(10, 'Reading News', NULL, '[{\"q\":\"Tin tức A2\"}]', '[\"A\"]', 'A2', 'reading', 'news', NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 0);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `exercise_completions`
--

CREATE TABLE `exercise_completions` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `exercise_id` int(11) NOT NULL,
  `user_answers_json` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`user_answers_json`)),
  `score` decimal(5,2) DEFAULT NULL,
  `total_questions` int(11) DEFAULT NULL,
  `started_at` datetime DEFAULT NULL,
  `completed_at` datetime DEFAULT NULL,
  `is_completed` tinyint(1) DEFAULT 0,
  `time_spent_minutes` int(11) DEFAULT NULL,
  `attempts` int(11) DEFAULT 1,
  `ai_graded` tinyint(1) DEFAULT 0 COMMENT 'Bài này có được AI chấm không',
  `review_status` enum('pending','approved','rejected','needs_regrade') DEFAULT 'pending' COMMENT 'Trạng thái review',
  `reviewed_by` int(11) DEFAULT NULL COMMENT 'Admin review (user_id)',
  `reviewed_at` datetime DEFAULT NULL COMMENT 'Thời gian review',
  `original_score` decimal(5,2) DEFAULT NULL COMMENT 'Điểm gốc của AI',
  `final_score` decimal(5,2) DEFAULT NULL COMMENT 'Điểm sau khi admin chấm lại',
  `review_notes` text DEFAULT NULL COMMENT 'Ghi chú của admin khi review',
  `confidence_score` decimal(3,2) DEFAULT NULL COMMENT 'Độ tự tin của AI (0.00-1.00)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `exercise_completions`
--

INSERT INTO `exercise_completions` (`id`, `user_id`, `exercise_id`, `user_answers_json`, `score`, `total_questions`, `started_at`, `completed_at`, `is_completed`, `time_spent_minutes`, `attempts`, `ai_graded`, `review_status`, `reviewed_by`, `reviewed_at`, `original_score`, `final_score`, `review_notes`, `confidence_score`) VALUES
(1, 2, 1, '[\"A\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(2, 3, 1, '[\"A\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(3, 4, 1, '[\"B\"]', 0.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(4, 5, 2, '[\"A\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(5, 6, 2, '[\"C\"]', 0.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(6, 7, 3, '[\"B\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(7, 8, 3, '[\"A\"]', 0.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(8, 9, 4, '[\"C\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(9, 10, 4, '[\"B\"]', 0.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(10, 2, 5, '[\"B\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(11, 3, 5, '[\"A\"]', 0.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(12, 5, 6, '[\"A\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(13, 6, 6, '[\"B\"]', 0.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(14, 7, 7, '[\"A\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(15, 8, 7, '[\"C\"]', 0.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(16, 9, 8, '[\"B\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(17, 10, 8, '[\"A\"]', 0.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(18, 3, 9, '[\"C\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(19, 4, 9, '[\"A\"]', 0.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL),
(20, 2, 10, '[\"A\"]', 100.00, 1, NULL, NULL, 1, NULL, 1, 0, 'pending', NULL, NULL, NULL, NULL, NULL, NULL);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `exercise_completion_scores`
--

CREATE TABLE `exercise_completion_scores` (
  `id` int(11) NOT NULL,
  `completion_id` int(11) NOT NULL,
  `question_number` int(11) NOT NULL,
  `user_answer` text DEFAULT NULL,
  `correct_answer` text DEFAULT NULL,
  `is_correct` tinyint(1) DEFAULT NULL,
  `points` decimal(5,2) DEFAULT NULL,
  `created_at` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `exercise_completion_scores`
--

INSERT INTO `exercise_completion_scores` (`id`, `completion_id`, `question_number`, `user_answer`, `correct_answer`, `is_correct`, `points`, `created_at`) VALUES
(1, 1, 1, 'A', 'A', 1, 100.00, '2025-12-01 15:22:43'),
(2, 2, 1, 'A', 'A', 1, 100.00, '2025-12-01 15:22:43'),
(3, 3, 1, 'B', 'A', 0, 0.00, '2025-12-01 15:22:43'),
(4, 4, 1, 'A', 'A', 1, 100.00, '2025-12-01 15:22:43'),
(5, 5, 1, 'C', 'A', 0, 0.00, '2025-12-01 15:22:43'),
(6, 6, 1, 'B', 'B', 1, 100.00, '2025-12-01 15:22:43'),
(7, 7, 1, 'A', 'B', 0, 0.00, '2025-12-01 15:22:43'),
(8, 8, 1, 'C', 'C', 1, 100.00, '2025-12-01 15:22:43'),
(9, 9, 1, 'B', 'C', 0, 0.00, '2025-12-01 15:22:43'),
(10, 10, 1, 'B', 'B', 1, 100.00, '2025-12-01 15:22:43'),
(11, 11, 1, 'A', 'B', 0, 0.00, '2025-12-01 15:22:43'),
(12, 12, 1, 'A', 'A', 1, 100.00, '2025-12-01 15:22:43'),
(13, 13, 1, 'B', 'A', 0, 0.00, '2025-12-01 15:22:43'),
(14, 14, 1, 'A', 'A', 1, 100.00, '2025-12-01 15:22:43'),
(15, 15, 1, 'C', 'A', 0, 0.00, '2025-12-01 15:22:43'),
(16, 16, 1, 'B', 'B', 1, 100.00, '2025-12-01 15:22:43'),
(17, 17, 1, 'A', 'B', 0, 0.00, '2025-12-01 15:22:43'),
(18, 18, 1, 'C', 'C', 1, 100.00, '2025-12-01 15:22:43'),
(19, 19, 1, 'A', 'C', 0, 0.00, '2025-12-01 15:22:43'),
(20, 20, 1, 'A', 'A', 1, 100.00, '2025-12-01 15:22:43');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `packages`
--

CREATE TABLE `packages` (
  `id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `description` text DEFAULT NULL,
  `price` decimal(10,2) NOT NULL,
  `duration_months` int(11) DEFAULT NULL COMMENT 'NULL = vĩnh viễn',
  `is_active` tinyint(1) DEFAULT 1,
  `created_at` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `packages`
--

INSERT INTO `packages` (`id`, `name`, `description`, `price`, `duration_months`, `is_active`, `created_at`) VALUES
(1, 'Premium 1 Month', 'Truy cập 1 tháng', 99000.00, 1, 1, '2025-12-01 15:22:43'),
(2, 'Premium 3 Months', 'Truy cập 3 tháng', 249000.00, 3, 1, '2025-12-01 15:22:43'),
(3, 'Premium 12 Months', 'Truy cập 1 năm', 799000.00, 12, 1, '2025-12-01 15:22:43'),
(4, 'Premium Lifetime', 'Truy cập vĩnh viễn', 1999000.00, NULL, 1, '2025-12-01 15:22:43'),
(5, 'Trial 7 Days', 'Dùng thử 7 ngày', 0.00, 0, 1, '2025-12-01 15:22:43');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `payments`
--

CREATE TABLE `payments` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `package_id` int(11) NOT NULL,
  `amount` decimal(10,2) NOT NULL,
  `method` varchar(50) DEFAULT NULL,
  `status` enum('pending','completed','failed') DEFAULT 'pending',
  `is_lifetime` tinyint(1) DEFAULT 1,
  `transaction_history` text DEFAULT NULL,
  `created_at` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `payments`
--

INSERT INTO `payments` (`id`, `user_id`, `package_id`, `amount`, `method`, `status`, `is_lifetime`, `transaction_history`, `created_at`) VALUES
(1, 2, 1, 99000.00, 'momo', 'completed', 0, NULL, '2025-12-01 15:22:43'),
(2, 3, 2, 249000.00, 'bank', 'completed', 0, NULL, '2025-12-01 15:22:43'),
(3, 3, 4, 1999000.00, 'bank', 'completed', 1, NULL, '2025-12-01 15:22:43'),
(4, 8, 1, 99000.00, 'paypal', 'completed', 0, NULL, '2025-12-01 15:22:43'),
(5, 10, 4, 1999000.00, 'credit', 'completed', 1, NULL, '2025-12-01 15:22:43'),
(6, 5, 5, 0.00, 'trial', 'completed', 0, NULL, '2025-12-01 15:22:43'),
(7, 7, 1, 99000.00, 'momo', 'completed', 0, NULL, '2025-12-01 15:22:43'),
(8, 9, 2, 249000.00, 'bank', 'completed', 0, NULL, '2025-12-01 15:22:43'),
(9, 6, 3, 799000.00, 'paypal', 'completed', 0, NULL, '2025-12-01 15:22:43'),
(10, 4, 1, 99000.00, 'momo', 'failed', 0, NULL, '2025-12-01 15:22:43');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `questions`
--

CREATE TABLE `questions` (
  `id` int(11) NOT NULL,
  `test_id` int(11) NOT NULL,
  `question_number` int(11) NOT NULL,
  `question_type` varchar(50) NOT NULL,
  `question_text` text DEFAULT NULL,
  `question_group_id` varchar(50) DEFAULT NULL,
  `audio_url` varchar(500) DEFAULT NULL,
  `image_url` varchar(500) DEFAULT NULL,
  `correct_answer_label` varchar(1) DEFAULT NULL,
  `answer_explanation` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `questions`
--

INSERT INTO `questions` (`id`, `test_id`, `question_number`, `question_type`, `question_text`, `question_group_id`, `audio_url`, `image_url`, `correct_answer_label`, `answer_explanation`) VALUES
(1, 1, 1, 'single', '2+2=?', NULL, NULL, NULL, 'A', NULL),
(2, 1, 2, 'single', 'Màu trời?', NULL, NULL, NULL, 'B', NULL),
(3, 1, 3, 'single', 'Fast = ?', NULL, NULL, NULL, 'C', NULL),
(4, 1, 4, 'single', 'Dog = ?', NULL, NULL, NULL, 'A', NULL),
(5, 1, 5, 'single', 'Book = ?', NULL, NULL, NULL, 'C', NULL),
(6, 2, 1, 'single', '5+5=?', NULL, NULL, NULL, 'C', NULL),
(7, 2, 2, 'single', 'Sun màu gì?', NULL, NULL, NULL, 'A', NULL),
(8, 2, 3, 'single', 'Small = ?', NULL, NULL, NULL, 'B', NULL),
(9, 2, 4, 'single', 'Big = ?', NULL, NULL, NULL, 'C', NULL),
(10, 2, 5, 'single', 'Tall = ?', NULL, NULL, NULL, 'C', NULL),
(11, 3, 1, 'single', 'Listen Q1', NULL, NULL, NULL, 'A', NULL),
(12, 3, 2, 'single', 'Listen Q2', NULL, NULL, NULL, 'B', NULL),
(13, 3, 3, 'single', 'Listen Q3', NULL, NULL, NULL, 'C', NULL),
(14, 3, 4, 'single', 'Listen Q4', NULL, NULL, NULL, 'A', NULL),
(15, 4, 1, 'single', 'Go (quá khứ)?', NULL, NULL, NULL, 'B', NULL),
(16, 4, 2, 'single', 'Eat (quá khứ)?', NULL, NULL, NULL, 'A', NULL),
(17, 4, 3, 'single', 'Drink (quá khứ)?', NULL, NULL, NULL, 'C', NULL),
(18, 4, 4, 'single', 'Write (quá khứ)?', NULL, NULL, NULL, 'B', NULL),
(19, 4, 5, 'single', 'Read (quá khứ)?', NULL, NULL, NULL, 'A', NULL),
(20, 5, 1, 'single', 'Reading A1 Q1', NULL, NULL, NULL, 'A', NULL),
(21, 5, 2, 'single', 'Reading A1 Q2', NULL, NULL, NULL, 'C', NULL),
(22, 5, 3, 'single', 'Reading A1 Q3', NULL, NULL, NULL, 'B', NULL);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `question_options`
--

CREATE TABLE `question_options` (
  `id` int(11) NOT NULL,
  `question_id` int(11) NOT NULL,
  `option_label` varchar(1) NOT NULL,
  `option_text` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `question_options`
--

INSERT INTO `question_options` (`id`, `question_id`, `option_label`, `option_text`) VALUES
(1, 1, 'A', '4'),
(2, 1, 'B', '3'),
(3, 1, 'C', '5'),
(4, 2, 'A', 'Green'),
(5, 2, 'B', 'Blue'),
(6, 2, 'C', 'Red'),
(7, 3, 'A', 'Slow'),
(8, 3, 'B', 'Weak'),
(9, 3, 'C', 'Quick'),
(10, 4, 'A', 'Chó'),
(11, 4, 'B', 'Mèo'),
(12, 4, 'C', 'Chim'),
(13, 5, 'A', 'Bàn'),
(14, 5, 'B', 'Ghế'),
(15, 5, 'C', 'Sách'),
(16, 6, 'A', '8'),
(17, 6, 'B', '9'),
(18, 6, 'C', '10'),
(19, 7, 'A', 'Yellow'),
(20, 7, 'B', 'Blue'),
(21, 7, 'C', 'White'),
(22, 8, 'A', 'Large'),
(23, 8, 'B', 'Small'),
(24, 8, 'C', 'Huge'),
(25, 9, 'A', 'Small'),
(26, 9, 'B', 'Tiny'),
(27, 9, 'C', 'Big'),
(28, 10, 'A', 'Short'),
(29, 10, 'B', 'Tall'),
(30, 10, 'C', 'Very tall'),
(31, 11, 'A', 'Answer A'),
(32, 11, 'B', 'Answer B'),
(33, 11, 'C', 'Answer C'),
(34, 12, 'A', 'Audio A'),
(35, 12, 'B', 'Audio B'),
(36, 12, 'C', 'Audio C'),
(37, 13, 'A', 'Audio A'),
(38, 13, 'B', 'Audio B'),
(39, 13, 'C', 'Audio C'),
(40, 14, 'A', 'Audio A'),
(41, 14, 'B', 'Audio B'),
(42, 14, 'C', 'Audio C'),
(43, 15, 'A', 'Go'),
(44, 15, 'B', 'Went'),
(45, 15, 'C', 'Goed'),
(46, 16, 'A', 'Ate'),
(47, 16, 'B', 'Eat'),
(48, 16, 'C', 'Eated'),
(49, 17, 'A', 'Drink'),
(50, 17, 'B', 'Drank'),
(51, 17, 'C', 'Drunk'),
(52, 18, 'A', 'Wrote'),
(53, 18, 'B', 'Write'),
(54, 18, 'C', 'Written'),
(55, 19, 'A', 'Read'),
(56, 19, 'B', 'Red'),
(57, 19, 'C', 'Reading'),
(58, 20, 'A', 'A1'),
(59, 20, 'B', 'A2'),
(60, 20, 'C', 'A3'),
(61, 21, 'A', 'B1'),
(62, 21, 'B', 'B2'),
(63, 21, 'C', 'B3'),
(64, 22, 'A', 'C1'),
(65, 22, 'B', 'C2'),
(66, 22, 'C', 'C3');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `tests`
--

CREATE TABLE `tests` (
  `id` int(11) NOT NULL,
  `title` varchar(255) NOT NULL,
  `total_questions` int(11) NOT NULL,
  `duration_minutes` int(11) DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT 1,
  `is_premium` tinyint(1) DEFAULT 0,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `tests`
--

INSERT INTO `tests` (`id`, `title`, `total_questions`, `duration_minutes`, `is_active`, `is_premium`, `created_at`) VALUES
(1, 'TOEIC Mini Test 1', 5, 10, 1, 0, '2025-12-01 08:22:43'),
(2, 'TOEIC Mini Test 2', 5, 10, 1, 0, '2025-12-01 08:22:43'),
(3, 'IELTS Listening Sample', 4, 10, 1, 1, '2025-12-01 08:22:43'),
(4, 'A2 Grammar Test', 5, 15, 1, 0, '2025-12-01 08:22:43'),
(5, 'Reading Test A1', 3, 10, 1, 0, '2025-12-01 08:22:43');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `test_completions`
--

CREATE TABLE `test_completions` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `test_id` int(11) NOT NULL,
  `score` decimal(5,2) DEFAULT NULL,
  `date_completed` datetime DEFAULT NULL,
  `attempt` int(11) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `test_completions`
--

INSERT INTO `test_completions` (`id`, `user_id`, `test_id`, `score`, `date_completed`, `attempt`) VALUES
(1, 2, 1, 80.00, '2025-12-01 15:22:43', 1),
(2, 3, 1, 100.00, '2025-12-01 15:22:43', 1),
(3, 4, 1, 40.00, '2025-12-01 15:22:43', 1),
(4, 5, 2, 60.00, '2025-12-01 15:22:43', 1),
(5, 6, 2, 20.00, '2025-12-01 15:22:43', 1),
(6, 7, 3, 100.00, '2025-12-01 15:22:43', 1),
(7, 8, 3, 50.00, '2025-12-01 15:22:43', 1),
(8, 9, 3, 75.00, '2025-12-01 15:22:43', 1),
(9, 10, 4, 90.00, '2025-12-01 15:22:43', 1),
(10, 2, 4, 70.00, '2025-12-01 15:22:43', 1),
(11, 3, 4, 85.00, '2025-12-01 15:22:43', 1),
(12, 4, 5, 60.00, '2025-12-01 15:22:43', 1),
(13, 5, 5, 30.00, '2025-12-01 15:22:43', 1),
(14, 6, 5, 50.00, '2025-12-01 15:22:43', 1),
(15, 7, 1, 100.00, '2025-12-01 15:22:43', 2),
(16, 8, 2, 70.00, '2025-12-01 15:22:43', 2),
(17, 9, 2, 95.00, '2025-12-01 15:22:43', 2),
(18, 10, 3, 100.00, '2025-12-01 15:22:43', 2),
(19, 2, 5, 80.00, '2025-12-01 15:22:43', 2),
(20, 3, 5, 90.00, '2025-12-01 15:22:43', 2);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `username` varchar(50) NOT NULL COMMENT 'Tên đăng nhập thủ công',
  `password_hash` varchar(255) NOT NULL COMMENT 'Mật khẩu mã hóa (bcrypt)',
  `email` varchar(100) NOT NULL COMMENT 'Email chính',
  `google_id` varchar(255) DEFAULT NULL COMMENT 'ID từ Google (sub)',
  `facebook_id` varchar(255) DEFAULT NULL COMMENT 'ID từ Facebook',
  `phone` varchar(20) DEFAULT NULL COMMENT 'Số điện thoại',
  `full_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT 'Họ tên đầy đủ',
  `bio` text DEFAULT NULL COMMENT 'Tiểu sử cá nhân',
  `address` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT 'Địa chỉ',
  `status` enum('active','inactive','banned') DEFAULT 'active' COMMENT 'Trạng thái tài khoản',
  `account_type` enum('free','premium') DEFAULT 'free' COMMENT 'Loại tài khoản',
  `premium_expires_at` datetime DEFAULT NULL COMMENT 'NULL = Premium vĩnh viễn',
  `total_study_time` int(11) DEFAULT 0 COMMENT 'Tổng phút học',
  `total_xp` int(11) DEFAULT 0 COMMENT 'Tổng điểm XP',
  `avatar_url` varchar(255) DEFAULT NULL COMMENT 'Link ảnh đại diện',
  `last_active_at` datetime DEFAULT NULL COMMENT 'Hoạt động cuối',
  `created_at` datetime DEFAULT current_timestamp(),
  `updated_at` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `role` enum('customer','admin','superadmin') NOT NULL DEFAULT 'customer' COMMENT 'Phân quyền người dùng'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `users`
--

INSERT INTO `users` (`id`, `username`, `password_hash`, `email`, `google_id`, `facebook_id`, `phone`, `full_name`, `bio`, `address`, `status`, `account_type`, `premium_expires_at`, `total_study_time`, `total_xp`, `avatar_url`, `last_active_at`, `created_at`, `updated_at`, `role`) VALUES
(1, 'admin', '$2b$10$aaaaaaaaaaaaaaaaaaaaaa', 'admin@example.com', NULL, NULL, NULL, 'System Admin', NULL, NULL, 'active', 'premium', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(2, 'john', '$2b$10$bbbbbbbbbbbbbbbbbbbbbb', 'john@example.com', NULL, NULL, NULL, 'John Doe', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(3, 'mary', '$2b$10$cccccccccccccccccccccc', 'mary@example.com', NULL, NULL, NULL, 'Mary Jane', NULL, NULL, 'active', 'premium', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(4, 'tom', '$2b$10$dddddddddddddddddddddd', 'tom@example.com', NULL, NULL, NULL, 'Tom Cruise', NULL, NULL, 'inactive', 'free', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(5, 'anna', '$2b$10$eeeeeeeeeeeeeeeeeeeeee', 'anna@example.com', NULL, NULL, NULL, 'Anna Smith', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(6, 'henry', '$2b$10$ffffffffffffffffffffff', 'henry@example.com', NULL, NULL, NULL, 'Henry Ford', NULL, NULL, 'banned', 'premium', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(7, 'susan', '$2b$10$gggggggggggggggggggggg', 'susan@example.com', NULL, NULL, NULL, 'Susan King', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(8, 'peter', '$2b$10$hhhhhhhhhhhhhhhhhhhhhh', 'peter@example.com', NULL, NULL, NULL, 'Peter Parker', NULL, NULL, 'active', 'premium', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(9, 'bruce', '$2b$10$iiiiiiiiiiiiiiiiiiiiii', 'bruce@example.com', NULL, NULL, NULL, 'Bruce Wayne', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(10, 'clark', '$2b$10$jjjjjjjjjjjjjjjjjjjjjj', 'clark@example.com', NULL, NULL, NULL, 'Clark Kent', NULL, NULL, 'active', 'premium', NULL, 0, 0, NULL, NULL, '2025-12-01 15:22:43', '2025-12-01 15:22:43', 'customer'),
(11, 'hoangdat', '$2a$12$GOqXhkf/2meuvyenr9w.4.vzdGpZnDrkPMQy//WPI3mhPncRqbV1u', 'nhdat3009@gmail.com', NULL, NULL, NULL, 'NGUYỄN HOÀNG ĐẠT', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, '2025-12-04 06:18:13', '2025-12-01 08:27:48', '2025-12-04 14:51:46', 'customer'),
(12, 'hoangdat1', '$2a$12$4jKB6cdHE76.Xtpz8oRKM.YwpOz5wespPaYjeTIdcBtKU89IMJxk.', 'nhdassst3009@gmail.com', NULL, NULL, NULL, 'NGUYỄN HOÀNG ĐẠT', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, '2025-12-01 08:28:41', '2025-12-01 08:28:16', '2025-12-04 14:51:51', 'customer'),
(13, 'hdat', '$2a$12$djaXebUcznXZVw4ZS89vlutI3juz5ANoAcHdEkAezGsvXYWSUY01q', 'hdat@spzoi10.edu.pl', NULL, NULL, NULL, 'NGUYỄN HOÀNG ĐẠT1', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, '2025-12-04 08:01:58', '2025-12-01 08:29:27', '2025-12-04 08:01:58', 'admin'),
(14, 'hdat123', '$2a$12$.K.H69mGwATfDGyGMpog/.ZZJdTxZIgyiVAHttA0ht57wvP.c8OOu', 'nhdat322009@gmail.com', NULL, NULL, NULL, 'NGUYỄN HOÀNG ĐẠT23', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, '2025-12-04 07:54:07', '2025-12-04 07:53:59', '2025-12-04 14:58:06', 'customer'),
(15, 'hd23', '$2a$12$9rHLnK3WY/wRcEDhsYrsz.mjYA6wfAbEWb7lrD8Tiz0VuObiAFxcq', 'hd222at@spzoi10.edu.pl', NULL, NULL, NULL, 'NGUYỄN HOÀNG ĐẠT121', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, NULL, '2025-12-04 07:57:14', '2025-12-04 14:58:06', 'customer'),
(16, 'hd1', '$2a$12$9CX6XJpMllAXhwhJb509fe6F5kUbi6yF73mZz5oGDK4.RDai52xVm', 'hdat@spzoi10.edu.pl1', NULL, NULL, NULL, 'NGUYỄN HOÀNG ĐẠT11', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, NULL, '2025-12-04 07:58:34', '2025-12-04 15:00:32', 'customer'),
(17, 'hdq', '$2a$12$MRXiQRsC9PtpnpVZ2vVrN.F243IF1B7sFQvZ3GwvKZRbgr4fbQ50G', 'hdat@3spzoi10.edu.pl', NULL, NULL, NULL, 'NGUYỄN HOÀNG ĐẠT3', NULL, NULL, 'active', 'free', NULL, 0, 0, NULL, '2025-12-04 08:01:37', '2025-12-04 08:00:55', '2025-12-04 15:05:11', 'customer');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `user_status_history`
--

CREATE TABLE `user_status_history` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `from_status` enum('active','inactive','banned') NOT NULL,
  `to_status` enum('active','inactive','banned') NOT NULL,
  `reason_code` varchar(32) DEFAULT NULL,
  `reason_note` text DEFAULT NULL,
  `changed_by` int(11) DEFAULT NULL,
  `changed_at` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `user_status_history`
--

INSERT INTO `user_status_history` (`id`, `user_id`, `from_status`, `to_status`, `reason_code`, `reason_note`, `changed_by`, `changed_at`) VALUES
(1, 4, 'active', 'inactive', 'timeout', 'User inactive 60 days', 1, '2025-12-01 15:22:43'),
(2, 6, 'active', 'banned', 'violate', 'Spam content', 1, '2025-12-01 15:22:43'),
(3, 2, 'inactive', 'active', 'return', 'User reactivated', 1, '2025-12-01 15:22:43');

--
-- Chỉ mục cho các bảng đã đổ
--

--
-- Chỉ mục cho bảng `dictionary_entries`
--
ALTER TABLE `dictionary_entries`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `word` (`word`),
  ADD KEY `idx_word` (`word`);

--
-- Chỉ mục cho bảng `exercises`
--
ALTER TABLE `exercises`
  ADD PRIMARY KEY (`id`),
  ADD KEY `created_by` (`created_by`),
  ADD KEY `idx_title` (`title`),
  ADD KEY `idx_type` (`type`),
  ADD KEY `idx_level` (`level`),
  ADD KEY `idx_active` (`is_active`),
  ADD KEY `idx_ai_generated` (`ai_generated`);

--
-- Chỉ mục cho bảng `exercise_completions`
--
ALTER TABLE `exercise_completions`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_user_exercise_attempt` (`user_id`,`exercise_id`,`attempts`),
  ADD KEY `idx_user` (`user_id`),
  ADD KEY `idx_exercise` (`exercise_id`),
  ADD KEY `idx_score` (`score`),
  ADD KEY `idx_review_status` (`review_status`),
  ADD KEY `idx_ai_graded` (`ai_graded`),
  ADD KEY `idx_reviewed_by` (`reviewed_by`);

--
-- Chỉ mục cho bảng `exercise_completion_scores`
--
ALTER TABLE `exercise_completion_scores`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_completion` (`completion_id`);

--
-- Chỉ mục cho bảng `packages`
--
ALTER TABLE `packages`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_active` (`is_active`);

--
-- Chỉ mục cho bảng `payments`
--
ALTER TABLE `payments`
  ADD PRIMARY KEY (`id`),
  ADD KEY `package_id` (`package_id`),
  ADD KEY `idx_user` (`user_id`),
  ADD KEY `idx_status` (`status`);

--
-- Chỉ mục cho bảng `questions`
--
ALTER TABLE `questions`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_test_question` (`test_id`,`question_number`),
  ADD KEY `idx_type` (`question_type`),
  ADD KEY `idx_group` (`question_group_id`);

--
-- Chỉ mục cho bảng `question_options`
--
ALTER TABLE `question_options`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_question_option` (`question_id`,`option_label`);

--
-- Chỉ mục cho bảng `tests`
--
ALTER TABLE `tests`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_title` (`title`),
  ADD KEY `idx_premium` (`is_premium`),
  ADD KEY `idx_active` (`is_active`);

--
-- Chỉ mục cho bảng `test_completions`
--
ALTER TABLE `test_completions`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_user_test_attempt` (`user_id`,`test_id`,`attempt`),
  ADD KEY `idx_user` (`user_id`),
  ADD KEY `idx_test` (`test_id`),
  ADD KEY `idx_score` (`score`),
  ADD KEY `idx_date` (`date_completed`);

--
-- Chỉ mục cho bảng `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `username` (`username`),
  ADD UNIQUE KEY `email` (`email`),
  ADD UNIQUE KEY `google_id` (`google_id`),
  ADD UNIQUE KEY `facebook_id` (`facebook_id`),
  ADD KEY `idx_email` (`email`),
  ADD KEY `idx_google` (`google_id`),
  ADD KEY `idx_facebook` (`facebook_id`),
  ADD KEY `idx_status` (`status`),
  ADD KEY `idx_account_type` (`account_type`),
  ADD KEY `idx_xp` (`total_xp`);

--
-- Chỉ mục cho bảng `user_status_history`
--
ALTER TABLE `user_status_history`
  ADD PRIMARY KEY (`id`),
  ADD KEY `changed_by` (`changed_by`),
  ADD KEY `idx_user` (`user_id`),
  ADD KEY `idx_changed_at` (`changed_at`);

--
-- AUTO_INCREMENT cho các bảng đã đổ
--

--
-- AUTO_INCREMENT cho bảng `dictionary_entries`
--
ALTER TABLE `dictionary_entries`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `exercises`
--
ALTER TABLE `exercises`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT cho bảng `exercise_completions`
--
ALTER TABLE `exercise_completions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=21;

--
-- AUTO_INCREMENT cho bảng `exercise_completion_scores`
--
ALTER TABLE `exercise_completion_scores`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=21;

--
-- AUTO_INCREMENT cho bảng `packages`
--
ALTER TABLE `packages`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT cho bảng `payments`
--
ALTER TABLE `payments`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT cho bảng `questions`
--
ALTER TABLE `questions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=23;

--
-- AUTO_INCREMENT cho bảng `question_options`
--
ALTER TABLE `question_options`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=67;

--
-- AUTO_INCREMENT cho bảng `tests`
--
ALTER TABLE `tests`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT cho bảng `test_completions`
--
ALTER TABLE `test_completions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=21;

--
-- AUTO_INCREMENT cho bảng `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=18;

--
-- AUTO_INCREMENT cho bảng `user_status_history`
--
ALTER TABLE `user_status_history`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- Các ràng buộc cho các bảng đã đổ
--

--
-- Các ràng buộc cho bảng `exercises`
--
ALTER TABLE `exercises`
  ADD CONSTRAINT `exercises_ibfk_1` FOREIGN KEY (`created_by`) REFERENCES `users` (`id`) ON DELETE SET NULL;

--
-- Các ràng buộc cho bảng `exercise_completions`
--
ALTER TABLE `exercise_completions`
  ADD CONSTRAINT `exercise_completions_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `exercise_completions_ibfk_2` FOREIGN KEY (`exercise_id`) REFERENCES `exercises` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `exercise_completions_ibfk_3` FOREIGN KEY (`reviewed_by`) REFERENCES `users` (`id`) ON DELETE SET NULL;

--
-- Các ràng buộc cho bảng `exercise_completion_scores`
--
ALTER TABLE `exercise_completion_scores`
  ADD CONSTRAINT `exercise_completion_scores_ibfk_1` FOREIGN KEY (`completion_id`) REFERENCES `exercise_completions` (`id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `payments`
--
ALTER TABLE `payments`
  ADD CONSTRAINT `payments_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `payments_ibfk_2` FOREIGN KEY (`package_id`) REFERENCES `packages` (`id`);

--
-- Các ràng buộc cho bảng `questions`
--
ALTER TABLE `questions`
  ADD CONSTRAINT `questions_ibfk_1` FOREIGN KEY (`test_id`) REFERENCES `tests` (`id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `question_options`
--
ALTER TABLE `question_options`
  ADD CONSTRAINT `question_options_ibfk_1` FOREIGN KEY (`question_id`) REFERENCES `questions` (`id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `test_completions`
--
ALTER TABLE `test_completions`
  ADD CONSTRAINT `test_completions_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `test_completions_ibfk_2` FOREIGN KEY (`test_id`) REFERENCES `tests` (`id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `user_status_history`
--
ALTER TABLE `user_status_history`
  ADD CONSTRAINT `user_status_history_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `user_status_history_ibfk_2` FOREIGN KEY (`changed_by`) REFERENCES `users` (`id`) ON DELETE SET NULL;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
