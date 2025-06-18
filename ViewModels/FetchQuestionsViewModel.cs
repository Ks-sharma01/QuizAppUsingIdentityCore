using QuizApplication.Models;

namespace QuizApplication.ViewModels
{
    public class FetchQuestionsViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int CategoryId { get; set; }
        public List<FetchOptionsViewModel> Options { get; set; }
        public string CategoryName { get; set; }
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }

    }
    public class FetchOptionsViewModel
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
    }
   
}
