using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApplication.Models
{
    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }

        public string Text { get; set; }

        public bool IsCorrect { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public Question Questions { get; set; }
    }
}
