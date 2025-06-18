using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuizApplication.Data;

namespace QuizApplication.Models
{
    public class UserAnswer
    {
        [Key]
        public int UserAnswerId { get; set; }

        [ForeignKey("User")]
        public int Id { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public Question Question { get; set; }

        [ForeignKey("Answer")]
        public int SelectedAnswerId { get; set; }
        public Answer SelectedAnswer { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
