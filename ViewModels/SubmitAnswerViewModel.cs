namespace QuizApplication.ViewModels
{
    public class SubmitAnswerViewModel
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
        public int TimeTaken { get; set; } // In seconds
    }
}
