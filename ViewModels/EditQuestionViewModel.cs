using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuizApplication.ViewModels
{
    public class EditQuestionViewModel
    {
        public int QuestionId {  get; set; }
        public string Text { get; set; }
        public int CategoryId { get; set; }

        public List<EditAnswerViewModel> editAnswers { get; set; }
        public SelectList CategoryList { get; set; }
    }

    public class EditAnswerViewModel
    {
        public int AnswerId { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }

    }
}
