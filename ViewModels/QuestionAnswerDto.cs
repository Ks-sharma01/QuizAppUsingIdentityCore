namespace QuizApplication.ViewModels
{
    public class QuestionAnswerDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int CategoryId { get; set; }
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
        public string CategoryName { get; set; }
    }
}
