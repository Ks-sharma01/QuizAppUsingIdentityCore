namespace QuizApplication.ViewModels
{
    public class ResultViewModel
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public List<QuizScoreDto> QuizScores { get; set; }

    }

    public class QuizScoreDto
    {
        public string QuizTitle { get; set; }
        public int Score { get; set; }
        public DateTime AttemptedOn { get; set; }
    }
}
