using System.ComponentModel;

namespace Entities.Enums
{
    public enum ListeningQuestionType
    {
        [Name("Hiểu tổng quan"), Description("General Comprehension: Hiểu nội dung tổng quan")]
        GeneralComprehension = 1,

        [Name("Chi tiết cụ thể"), Description("Specific Details: Chi tiết cụ thể trong đoạn hội thoại")]
        SpecificDetails = 2,

        [Name("Suy luận"), Description("Inference: Suy luận từ nội dung nghe được")]
        Inference = 3,

        [Name("Thái độ/Cảm xúc"), Description("Attitude/Emotion: Thái độ và cảm xúc của người nói")]
        AttitudeEmotion = 4,

        [Name("Từ vựng trong ngữ cảnh"), Description("Vocabulary in Context: Nghĩa của từ trong ngữ cảnh")]
        VocabularyInContext = 5,

        [Name("Mục đích"), Description("Purpose: Mục đích của đoạn hội thoại")]
        Purpose = 6,

        [Name("Điền từ thiếu"), Description("Fill in the Blanks: Điền từ còn thiếu trong đoạn hội thoại")]
        FillInTheBlanks = 7,

        [Name("Nhận dạng âm thanh"), Description("Sound Recognition: Nhận dạng âm thanh cụ thể")]
        SoundRecognition = 8
    }
}