using System.ComponentModel.DataAnnotations;

namespace QuizApplication.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public List<Question> Questions { get; set; }
    }
}
