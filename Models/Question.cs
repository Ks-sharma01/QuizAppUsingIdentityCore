using System.ComponentModel.DataAnnotations;

namespace QuizApplication.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        public string Text { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }

        public List<Answer> Answers { get; set; }
    }
}
