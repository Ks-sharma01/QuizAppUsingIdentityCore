using Microsoft.AspNetCore.Mvc.Rendering;
using QuizApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace QuizApplication.ViewModels
{
    public class QuestionInputViewModel
    {
        public string Text { get; set; }

        public int CategoryId { get; set; }
        public List<AnswerInputViewModel> Answers { get; set; }
        public List<Category> categories { get; set; }

        public SelectList CategoryList { get; set; }
    }
    public class AnswerInputViewModel
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
