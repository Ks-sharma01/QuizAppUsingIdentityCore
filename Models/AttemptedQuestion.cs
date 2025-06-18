using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuizApplication.Data;
using Microsoft.AspNetCore.Identity;

namespace QuizApplication.Models
{
    public class AttemptedQuestion
    {
        [Key]
        public int AttemptId { get; set; }

        [ForeignKey("User")]
        public int Id { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public DateTime AttemptedOn { get; set; }
    }
}
