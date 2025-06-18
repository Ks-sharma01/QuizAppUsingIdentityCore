using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuizApplication.Data;

namespace QuizApplication.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [ForeignKey("User")]
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public int Score { get; set; }

        public DateTime AttemptedOn { get; set; }

        public ApplicationUser User { get; set; }
        public Category Category { get; set; }
    }
}
