using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuizApplication.Data;
using QuizApplication.Models;
using QuizApplication.ViewModels;
using System.Data;
using System.Linq;

namespace QuizApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Leaderboard(int categoryId)
        {
            var LeaderboardData = await _context.Results.Where(x => x.CategoryId == categoryId)
                .OrderByDescending(s => s.Score)
                .Include(s => s.User)
                .Include(s => s.Category).ToListAsync();
            var viewModel =  LeaderboardData.Select((s, index) => new LeaderboardViewModel
            {
                Rank = index + 1,
                Username = s.User.UserName,
                CategoryName = s.Category.Name,
                Score = s.Score,
            }).ToList();
            return Json(viewModel);
        }

        public IActionResult SelectCategory()
        {
            var categories = _context.Category.ToList();
            return View(categories);
        }

        public async Task<IActionResult> AssignRoleAsync()
        {
            var users = _userManager.Users.ToList();
            var model = new List<AssignRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new AssignRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    CurrentRole = roles.FirstOrDefault() ?? "None"
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(int userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var userRoles = await _userManager.GetRolesAsync(user);

            // Remove old roles
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            // Assign new role
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction("AssignRole");
        }

        public async Task<IActionResult> Questions()
        {
            var categories = await _context.Category.ToListAsync();

            var questionWithCategories = await _context.Questions
                .Include(q => q.Category)
                .Include(q => q.Answers)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
            return View(questionWithCategories);
        }

        public async Task<IActionResult> AddQuestion()
        {
            var categories = await _context.Category.ToListAsync();
            var viewModel = new QuestionInputViewModel
            {
              CategoryList = new SelectList(categories, "CategoryId", "Name"),
              Answers = new List<AnswerInputViewModel>
              {
                new AnswerInputViewModel()
              }
            };

            return View(viewModel);
        }

        public IActionResult AddCategory()
        {
            var categories = _context.Category.ToList();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category model)
        {
            var parameters = new[]
            {
                new SqlParameter("@Operation", "insert"),
                new SqlParameter("@Name", model.Name),
            };
            await _context.Database.ExecuteSqlRawAsync("EXEC usp_CategoryModification @Operation,null, @Name", parameters);
            return RedirectToAction("AddQuestion");
        }
        

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var parameters = new[]
            {
                new SqlParameter("@Operation", "delete"),
                new SqlParameter("@CategoryId", id)
            };
            await _context.Database.ExecuteSqlRawAsync("EXEC spCategoryOperation @Operation, @CategoryId", parameters);
            return RedirectToAction("AddCategory");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(QuestionInputViewModel model)
        {
            var QuestionsParameters = new[]
            {
            new SqlParameter("@Text", model.Text),
            new SqlParameter("@CategoryId", model.CategoryId),
            };

            // Insert Question through Stored Procedure
            await _context.Database.ExecuteSqlRawAsync("EXEC usp_AddQuestion @Text, @CategoryId", QuestionsParameters);

            // Get the inserted question to get its ID
            var insertedQuestion = await _context.Questions
                .OrderByDescending(q => q.QuestionId)
                .FirstOrDefaultAsync(q => q.Text == model.Text);
            if (insertedQuestion != null)
            {    
                foreach (var answer in model.Answers)
                {   
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC usp_AddAnswer @QuestionId = {0}, @Text = {1}, @IsCorrect = {2}",
                       insertedQuestion.QuestionId, answer.Text, answer.IsCorrect 
                    );
                }
            }
            return RedirectToAction("Questions", "Admin"); // Redirect to the question list
        }

        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _context.Questions.Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == id);
            if(question == null)
            {
                return NotFound();
            }
            var categories = await _context.Category.ToListAsync();
            var viewModel = new EditQuestionViewModel
            {
                QuestionId = question.QuestionId,
                Text = question.Text,
                CategoryId = question.CategoryId,
                editAnswers = question.Answers.Select(x => new EditAnswerViewModel
                {
                    AnswerId = x.AnswerId,
                    Text = x.Text,
                    IsCorrect = x.IsCorrect,
                }).ToList(),
                 CategoryList = new SelectList(categories, "CategoryId", "Name")
            };
            return View(viewModel);
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(EditQuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                return View(model);
            }
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == model.QuestionId);

            if(question == null)
            {
                return NotFound();
            }
            question.Text = model.Text;
            question.CategoryId = model.CategoryId;

            foreach(var answermodel in model.editAnswers)
            {
                var answer = question.Answers.FirstOrDefault(a => a.AnswerId == answermodel.AnswerId);
                if(answer != null)
                {
                    answer.Text = answermodel.Text;
                    answer.IsCorrect = answermodel.IsCorrect;
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Questions", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();
            var answerIds = question.Answers.Select(a => a.AnswerId).ToList();
            var userAnswers = _context.UserAnswer.Where(ua => answerIds.Contains(ua.SelectedAnswerId));
            _context.UserAnswer.RemoveRange(userAnswers);

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("Questions", "Admin");
        }



    }
}
