using Entities;
using System.ComponentModel;

namespace Entities.Enums
{
    public enum ListeningGenre
    {
        [Name("Hội thoại đời sống"), Description("Daily Conversation: Hội thoại đời sống thân thiện, tự nhiên")]
        DailyConversation = 1,

        [Name("Tin tức ngắn"), Description("News Report: Bản tin thời sự ngắn gọn, súc tích")]
        NewsReport = 2,

        [Name("Chuyện kể"), Description("Storytelling: Câu chuyện kể giàu cảm xúc")]
        Storytelling = 3,

        [Name("Bài giảng học thuật"), Description("Academic Lecture: Bài giảng học thuật, giàu thông tin")]
        AcademicLecture = 4,

        [Name("Cuộc họp công việc"), Description("Business Meeting: Cuộc họp công việc tập trung vào giải pháp")]
        BusinessMeeting = 5
    }
}
