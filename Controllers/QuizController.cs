using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuizApplication.Data;
using QuizApplication.Models;
using QuizApplication.ViewModels;

namespace QuizDishtv.Controllers
{
    [Authorize(Roles = "User")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Categories()
        {
            var categories = _context.Category.ToList();
            return View(categories);
        }

        public async Task<IActionResult> StartQuiz(int categoryId, int questionIndex = 0)
        {
            var TotalQuestions = await _context.Questions
                                    .Where(q => q.CategoryId == categoryId)
                                    .Include(x => x.Answers)
                                    .OrderBy(x => Guid.NewGuid())
                                    .ToListAsync();

            var randomQuestion = TotalQuestions.Distinct().ToList();
            
            if (randomQuestion.Count == 0)
            {
                return NotFound("No questions found for this category.");
            }

            if (questionIndex >= randomQuestion.Count)
            {
                return RedirectToAction("ShowResult", new { categoryId });
            }

            var question = randomQuestion[questionIndex];
            var checkAttempted = _context.AttemptedQuestions.FirstOrDefault(x => x.CategoryId == categoryId && x.QuestionId == question.QuestionId);
            if (checkAttempted != null)
            {
                var attemptedCount = _context.AttemptedQuestions.Where(x => x.CategoryId == categoryId).ToList();
                if(attemptedCount.Count == randomQuestion.Count)
                {
                    return RedirectToAction("ShowResult", new { categoryId });

                }
                
                return RedirectToAction("StartQuiz", new { categoryId, questionIndex });

            }
            ViewBag.CategoryId = categoryId;
            ViewBag.QuestionIndex = questionIndex;
            ViewBag.TotalQuestions = randomQuestion.Count;

            return View(question);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int categoryId, int questionId, int selectedAnswerId, int questionIndex)   //int categoryId, int questionId, int selectedAnswerId,
        {
            var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var existing = await _context.UserAnswer.Where(x => x.UserAnswerId == userId && x.QuestionId == questionId).FirstOrDefaultAsync();

            if (existing == null)
            {
                existing = new UserAnswer
                {
                    Id = userId,
                    QuestionId = questionId,
                    SelectedAnswerId = selectedAnswerId,
                    CategoryId = categoryId,

                };
                _context.UserAnswer.Add(existing);
                await _context.SaveChangesAsync();
            }
            else
            {
                existing.SelectedAnswerId = selectedAnswerId;
                _context.UserAnswer.Update(existing);
            }
            await _context.SaveChangesAsync();
            ViewBag.CategoryId = categoryId;
            var storedAttemptedQuestion = new AttemptedQuestion
            {
                QuestionId = questionId,
                Id = userId,
                CategoryId = categoryId,
                AttemptedOn = DateTime.Now,
            };
            _context.AttemptedQuestions.Add(storedAttemptedQuestion);
            await _context.SaveChangesAsync();


            return RedirectToAction("StartQuiz", new { categoryId, questionIndex = questionIndex + 1 });
        }

        public async Task<IActionResult> ShowResult(int categoryId)
        {
            var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;
            var answers = await _context.UserAnswer
                .Include(ua => ua.SelectedAnswer)
                .Include(ua => ua.Question)
                .ThenInclude(ua => ua.Answers)
                .Include(ua => ua.Category)
                .Where(ua => ua.Id == userId && ua.Question.CategoryId == categoryId)
                .ToListAsync();
            int score = answers.Count(a => a.SelectedAnswer.IsCorrect);
            var result = await _context.Results
                .FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.Id == userId);
           
            if (result == null)
            {
                result = new Result
                {
                    Score = score,
                    CategoryId = categoryId,
                    Id = userId,
                    AttemptedOn = DateTime.Now,
                };
                _context.Results.Add(result);
            }
            else
            {
                result.Score = score;
                result.AttemptedOn = DateTime.Now;
                _context.Results.Update(result);
            }
           await _context.SaveChangesAsync();

            ViewBag.Score = score;
            TempData["Total"] = answers.Count;
            ViewBag.CategoryId = categoryId;

            return View(answers);

        }

        public async Task<IActionResult> RestartQuiz(int categoryId)
        {
            var userId = _context.Users.FirstOrDefault(x => x.UserName == User.Identity.Name)?.Id;
            if (userId == null)
            {
                return Unauthorized();
            }

            var attemptsToRemove = await _context.AttemptedQuestions
                .Where(a => a.Id == userId && a.Question.CategoryId == categoryId).ToListAsync();
            _context.AttemptedQuestions.RemoveRange(attemptsToRemove);
            await _context.SaveChangesAsync();

            return RedirectToAction("Categories");
        }

        [Authorize]
        public IActionResult SelectQuiz()
        {
            var subject =  _context.Category.Select(p => new { p.CategoryId, p.Name });
            return View(subject);
        }

        public async Task<IActionResult> QuizSummary()
        {
            var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Id;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            var quizAttempts = await _context.Results.Include(a => a.Category)
                .Where(a => a.Id == userId)
                .OrderByDescending(a => a.AttemptedOn).ToListAsync();
            if (quizAttempts == null)
            {
                return NotFound("No Quiz Attempts");
            }

            var viewModel = new ResultViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                QuizScores = quizAttempts.Select(a => new QuizScoreDto
                {
                    QuizTitle = a.Category.Name,
                    Score = a.Score,
                    AttemptedOn = a.AttemptedOn,

                }).ToList()
            };

            return View(viewModel);

        }
    
    }
}
