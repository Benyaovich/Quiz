using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quiz.DataAccess.Models;

namespace Quiz.DataAccess
{
    public static class DbInitializer
    {
        public static async Task Initialize(QuizDbContext context, UserManager<User> userManager)
        {
            await context.Database.MigrateAsync();

            if (await context.Users.AnyAsync()) return;
            User user = new User
            {
                UserName = "UserTeszt",
                Email = "userteszt@userteszt.com"
            };
            await userManager.CreateAsync(user, "Teszt123*");

            if (await context.Quizzes.AnyAsync()) return;
            Models.Quiz[] quizzes = [
                new()
                {
                    Pin = 999999,
                    Title = "TesztKvíz",
                    Questions = new List<Question>{
                        new(){
                            QuestionText = "Kérdés 1",
                            Answers =
                            [
                                "Válasz1",
                                "Válasz2",
                                "Válasz3",
                                "Válasz4"
                            ],
                            IndexOfCorrectAnswer = 0
                        },
                        new(){
                            QuestionText = "Kérdés 2",
                            Answers =
                            [
                                "Válasz1",
                                "Válasz2"
                            ],
                            IndexOfCorrectAnswer = 1
                        }
                    },
                    IsPublished = true,
                    User = user
                },
                new()
                {
                    Pin = 0,
                    Title = "TesztKvíz2",
                    Questions = new List<Question>{
                        new(){
                            QuestionText = "Kérdés 1",
                            Answers =
                            [
                                "Válasz1",
                                "Válasz2",
                                "Válasz3",
                                "Válasz4"
                            ],
                            IndexOfCorrectAnswer = 0
                        },
                        new(){
                            QuestionText = "Kérdés 2",
                            Answers =
                            [
                                "Válasz1",
                                "Válasz2",
                                "Válasz3",
                                "Válasz4"
                            ],
                            IndexOfCorrectAnswer = 1
                        }
                    },
                    IsPublished = true,
                    User = user
                },
                new()
                {
                    Title = "TesztKvíz3",
                    Questions = new List<Question>{
                        new(){
                            QuestionText = "Kérdés 1",
                            Answers =
                            [
                                "Válasz1",
                                "Válasz2",
                                "Válasz3",
                                "Válasz4"
                            ],
                            IndexOfCorrectAnswer = 0
                        },
                        new(){
                            QuestionText = "Kérdés 2",
                            Answers =
                            [
                                "Válasz1",
                                "Válasz2",
                                "Válasz3",
                                "Válasz4"
                            ],
                            IndexOfCorrectAnswer = 1
                        }
                    },
                    User = user
                }
            ];
            context.Quizzes.AddRange(quizzes);

            await context.SaveChangesAsync();
        }
    }
}
