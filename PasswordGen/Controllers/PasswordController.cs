using Microsoft.AspNetCore.Mvc;
using PasswordGen.Models;
using PasswordGen.Services;

namespace PasswordGen.Controllers;

[ApiController]
[Route("api/password")]
[Produces("application/json")]
public class PasswordController : ControllerBase
{
    private readonly HistoryService _historyService;
    private readonly LeakCheckerService _leakChecker;

    // Внедряем ОБА сервиса через конструктор
    public PasswordController(HistoryService historyService, LeakCheckerService leakChecker)
    {
        _historyService = historyService;
        _leakChecker = leakChecker;
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(List<HistoryItem>), StatusCodes.Status200OK)]
    public ActionResult<List<HistoryItem>> GetHistory()
    {
        return Ok(_historyService.Items.ToList());
    }

    [HttpPost("generate")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(GeneratePasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeneratePasswordResponse>> Generate([FromBody] GeneratePasswordRequest? request)
    {
        if (request is null)
            return BadRequest(new ErrorResponse { Message = "Отсутствует тело запроса." });

        if (request.Length is < 4 or > 128)
            return BadRequest(new ErrorResponse { Message = "Длина должна быть от 4 до 128 символов." });

        var pool = PasswordService.BuildPool(request.UseUppercase, request.UseLowercase, request.UseDigits, request.UseSymbols, request.ExcludeLookAlike);
        if (pool.Length == 0)
            return BadRequest(new ErrorResponse { Message = "Пустой пул символов: выберите хотя бы один тип символов." });

        var classes = PasswordService.SelectedClasses(request);
        var password = PasswordService.Generate(request.Length, pool, classes);
        var entropy = PasswordService.EntropyBits(request.Length, pool.Length);

        // НОВАЯ ЛОГИКА: Проверяем пароль на утечки перед отправкой
        int breachCount = await _leakChecker.CheckPasswordLeakAsync(password);

        var response = new GeneratePasswordResponse
        {
            Password = password,
            Length = request.Length,
            PoolSize = pool.Length,
            EntropyBits = Math.Round(entropy, 2),
            Strength = PasswordService.Strength(entropy),
            BreachCount = breachCount, // Передаем результат проверки
            GeneratedAt = DateTime.Now
        };

        _historyService.Add(new HistoryItem(response.Password, response.EntropyBits, response.Strength, response.GeneratedAt));

        return Ok(response);
    }
}