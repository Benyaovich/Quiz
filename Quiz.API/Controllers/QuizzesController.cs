using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz.DataAccess.Services;
using Quiz.Shared.Requests;
using Quiz.Shared.Responses;
using Quiz.SignalR.Services;

namespace Quiz.API.Controllers
{
    [ApiController]
    [Route("/quizzes")]
    public class QuizzesController(
        IMapper mapper,
        IQuizzesService quizzesService,
        IQuizNotificationService quizNotificationService)
        : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(PagedQuizzesResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublishedQuizzes([FromQuery] int page = 1)
        {
            var (publishedQuizzes, totalCount, pageSize) = await quizzesService.GetPublishedQuizzesAsync(page);
            var quizResponseDtoList = mapper.Map<List<QuizResponseDto>>(publishedQuizzes);

            return Ok(
                new PagedQuizzesResponseDto
                {
                    Quizzes = quizResponseDtoList,
                    Pagination = new PaginationResponseDto
                    {
                        TotalCount = totalCount,
                        PageSize = pageSize
                    }
                }
            );
        }

        [HttpPost]
        [Route("{quizId}")]
        [ProducesResponseType(typeof(JoinResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuizById([FromRoute] int quizId,[FromBody] QuizAccessRequestDto quizAccessRequestDto)
        {
            _ = int.TryParse(quizAccessRequestDto.Pin, out var pin);
            var quiz = await quizzesService.GetQuizByIdAsync(quizId, quizAccessRequestDto.UserName, pin);
            var quizResponse = mapper.Map<JoinResponseDto>(quiz);
            return Ok(quizResponse);
        }

        [HttpPost]
        [Authorize]
        [Route("create")]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizRequestDto quizRequestDto)
        {
            var mappedQuiz = mapper.Map<DataAccess.Models.Quiz>(quizRequestDto);
            var newQuiz = await quizzesService.CreateQuizAsync(mappedQuiz);
            var newQuizResponseDto = mapper.Map<QuizResponseDto>(newQuiz);
            return CreatedAtAction(nameof(CreateQuiz),newQuizResponseDto);
        }

        [HttpGet]
        [Authorize]
        [Route("{quizId}/publish")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> PublishQuiz([FromRoute] int quizId)
        {
            int pin = await quizzesService.PublishQuizAsync(quizId);
            return Ok(pin.ToString("D6"));
        }

        [HttpPost]
        [Route("{quizId}/join")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(JoinResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> JoinQuiz([FromRoute] int quizId,[FromBody] JoinRequestDto joinRequestDto)
        {
            _ = int.TryParse(joinRequestDto.QuizAccess.Pin, out var pin);
            var quiz = await quizzesService.JoinQuizAsync(quizId, pin, joinRequestDto.QuizAccess.UserName);
            var joinResponseDto = mapper.Map<JoinResponseDto>(quiz);
            return Ok(joinResponseDto);
        }

        [HttpGet]
        [Authorize]
        [Route("{quizId}/start")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> StartQuiz([FromRoute] int quizId)
        {
            await quizzesService.StartQuizAsync(quizId);
            await quizNotificationService.NotifyQuizStart(quizId.ToString());
            return Ok();
        }

        [HttpPost]
        [Route("{quizId}/activeQuestion")]
        [ProducesResponseType(typeof(QuestionResponseDto),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> GetActiveQuestion([FromRoute] int quizId, [FromBody] QuizAccessRequestDto quizAccessRequestDto)
        {
            _ = int.TryParse(quizAccessRequestDto.Pin, out var pin);
            var question = await quizzesService.GetActiveQuestionAsync(quizId, quizAccessRequestDto.UserName, pin);
            var questionResponseDto = mapper.Map<QuestionResponseDto>(question);
            return Ok(questionResponseDto);
        }

        [HttpGet]
        [Authorize]
        [Route("{quizId}/nextQuestion")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> NextQuestion([FromRoute] int quizId)
        {
            await quizzesService.NextQuestionAsync(quizId);
            await quizNotificationService.NotifyNextQuestion(quizId.ToString());
            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("{quizId}/closeQuestion")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CloseQuestion([FromRoute] int quizId)
        {
            await quizzesService.CloseActiveQuestionAsync(quizId);
            await quizNotificationService.NotifyActualQuestionClose(quizId.ToString());
            return Ok();
        }

        [HttpPost]
        [Route("{quizId}/activeQuestion/answer")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> ShowAnswer([FromRoute] int quizId, [FromBody] QuizAccessRequestDto quizAccessRequestDto)
        {
            _ = int.TryParse(quizAccessRequestDto.Pin, out int pin);
            int indexOfCorrectAnswer = await quizzesService.ShowAnswer(quizId, quizAccessRequestDto.UserName, pin);
            return Ok(indexOfCorrectAnswer);
        }

        [HttpPost]
        [Route("{quizId}/chat")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SendMessage([FromRoute] int quizId, [FromBody] ChatRequestDto chatRequestDto)
        {
            _ = int.TryParse(chatRequestDto.QuizAccess.Pin, out var pin);
            await quizzesService.SendMessage(quizId, chatRequestDto.QuizAccess.UserName, pin, chatRequestDto.Message);
            await quizNotificationService.NotifyNewChatMessage(quizId.ToString());
            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("my-quizzes/{quizId}")]
        [ProducesResponseType(typeof(ExtendedQuizResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyQuiz([FromRoute] int quizId)
        {
            var quiz = await quizzesService.GetMyQuiz(quizId);
            var extendedQuizResponseDto = mapper.Map<ExtendedQuizResponseDto>(quiz);

            return Ok(extendedQuizResponseDto);
        }
    }
}
