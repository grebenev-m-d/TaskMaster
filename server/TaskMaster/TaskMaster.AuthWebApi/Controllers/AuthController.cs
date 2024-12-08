using CommonLib.Services.EmailServices;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using TaskMaster.AuthWebApi.Helpers;
using TaskMaster.AuthWebApi.Models;
using TaskMaster.AuthWebApi.Service.JwtTokenService;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.RoleRepository;
using TaskMaster.DataAccessModule.Repository.UserRepository;
using TaskMaster.Validation;

namespace TaskMaster.AuthWebApi.Controllers
{
	/// <summary>
	/// Контроллер для аутентификации и регистрации пользователей.
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		/// <summary>
		/// Репозиторий пользователей.
		/// </summary>
		private readonly IUserRepository _userRepository;

		/// <summary>
		/// Сервис для генерации JWT-токенов.
		/// </summary>
		private readonly IJwtTokenService _jwtTokenService;

		/// <summary>
		/// Сервис для отправки электронной почты.
		/// </summary>
		private readonly IEmailServices _emailServices;

		/// <summary>
		/// Словарь для хранения ожидающих запросов на подтверждение email.
		/// </summary>
		private static readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _pendingRequests =
			new ConcurrentDictionary<string, TaskCompletionSource<object>>();

		/// <summary>
		/// Конструктор контроллера AuthController.
		/// </summary>
		/// <param name="userRepository">Репозиторий пользователей.</param>
		/// <param name="jwtTokenService">Сервис для генерации JWT-токенов.</param>
		/// <param name="emailServices">Сервис для отправки электронной почты.</param>
		public AuthController(
			IUserRepository userRepository,
			IJwtTokenService jwtTokenService,
			IEmailServices emailServices)
		{
			_userRepository = userRepository;
			_jwtTokenService = jwtTokenService;
			_emailServices = emailServices;
		}

		/// <summary>
		/// Метод для проверки подтверждения email.
		/// </summary>
		/// <param name="emailVerificationId">Идентификатор подтверждения email.</param>
		/// <returns>Результат проверки.</returns>
		[HttpGet("check-email")]
		public async Task<ActionResult<string>> CheckEmail(string emailVerificationId)
		{
			if (_pendingRequests.TryRemove(emailVerificationId, out var tcs))
			{
				// Завершить ожидающий TaskCompletionSource
				tcs.TrySetResult(null);
				return Ok("Подтверждено");
			}

			return NotFound("Не подтверждено");
		}

		/// <summary>
		/// Метод для регистрации нового пользователя.
		/// </summary>
		/// <param name="formRegistration">Данные формы регистрации.</param>
		/// <returns>Результат регистрации.</returns>
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] FormRegistration formRegistration)
		{
			try
			{
				if (formRegistration is null)
				{
					return BadRequest("Неверный формат запроса.");
				}

				// Проверка наличия обязательных полей
				if (string.IsNullOrEmpty(formRegistration.Name) ||
					string.IsNullOrEmpty(formRegistration.Email) ||
					string.IsNullOrEmpty(formRegistration.Password) ||
					formRegistration.Password != formRegistration.ConfirmPassword)
				{
					return BadRequest("Проверьте правильность заполнения полей.");
				}

				if (await _userRepository.GetByEmailAsync(formRegistration.Email) != null)
				{
					return BadRequest("Пользователь с таким email уже существует.");
				}

				PasswordValidation.Validate(formRegistration.Password);

				// Проверка наличия пользователя с таким email
				var existingUser = await _userRepository.GetByEmailAsync(formRegistration.Email);
				if (existingUser != null)
				{
					return BadRequest("Пользователь с таким адресом электронной почты уже существует.");
				}

				// Создание уникального идентификатора для подтверждения email
				string emailVerificationId = Guid.NewGuid().ToString();

				// Отправка письма с запросом на подтверждение email
				if (!await _emailServices.SendConfirmationEmailAsync(formRegistration.Email, emailVerificationId))
				{
					return BadRequest("Не удалось отправить письмо, попробуйте еще раз.");
				}

				// Добавление запроса на подтверждение email в словарь ожидающих запросов
				var tcs = new TaskCompletionSource<object>();
				_pendingRequests.TryAdd(emailVerificationId, tcs);

				// Ожидание подтверждения email
				await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromMinutes(1)));

				// Если email не подтвержден, возвращаем ошибку
				if (!tcs.Task.IsCompleted)
				{
					return BadRequest("Ожидание подтверждения email завершено.");
				}

				// Хэширование пароля
				var passwordHash = BCrypt.Net.BCrypt.HashPassword(formRegistration.Password);

				// Создание нового пользователя
				var user = new DbUser
				{
					Name = formRegistration.Name,
					Email = formRegistration.Email,
					PasswordHash = passwordHash,
					Role = new DbRole() { Type = RoleType.user }
				};

				// Добавление нового пользователя в базу данных
				var addedUser = await _userRepository.AddAsync(user);

				// Генерация JWT-токенов для нового пользователя
				var payload = СlientModelMappers.MapDbUserToJwtPayload(addedUser);
				var accessToken = await _jwtTokenService.GenerateToken(payload);

				// Возвращаем успешный результат с сгенерированными токенами
				return Ok(accessToken);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return BadRequest("Неизвестная ошибка.");

			}
		}
		class JWT
		{
		 public	string Token { get; set; }
		}

		/// <summary>
		/// Метод для аутентификации пользователя.
		/// </summary>
		/// <param name="formLogin">Данные формы входа.</param>
		/// <returns>Результат аутентификации.</returns>
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] FormLogin formLogin)
		{
			// Проверка наличия данных формы входа
			if (formLogin is null)
			{
				return BadRequest("Неверный формат запроса.");
			}

			// Проверка наличия адреса электронной почты
			if (string.IsNullOrEmpty(formLogin.Email))
			{
				return BadRequest("Email не может быть пустым.");
			}

			// Проверка наличия пароля
			if (string.IsNullOrEmpty(formLogin.Password))
			{
				return BadRequest("Пароль не может быть пустым.");
			}

			// Поиск пользователя по адресу электронной почты
			var user = await _userRepository.GetByEmailAsync(formLogin.Email);
			if (user is null)
			{
				return Unauthorized("Неверный адрес электронной почты.");
			}

			// Проверка совпадения хэша пароля
			if (!BCrypt.Net.BCrypt.Verify(formLogin.Password, user.PasswordHash))
			{
				return Unauthorized("Неверный пароль.");
			}

			// Создание JWT-токенов для пользователя
			var payload = СlientModelMappers.MapDbUserToJwtPayload(user);
			var accessToken = await _jwtTokenService.GenerateToken(payload);

			// Возвращение успешного результата с токенами доступа и обновления
			return Ok(accessToken);
		}

		/// <summary>
		/// Метод для сброса пароля пользователя.
		/// </summary>
		/// <param name="formChangePassword">Данные формы смены пароля.</param>
		/// <returns>Результат сброса пароля.</returns>
		[HttpPost("restore-password")]
		public async Task<IActionResult> RestorePassword(FormRestorePassword formChangePassword)
		{
			// Проверка наличия данных формы смены пароля
			if (formChangePassword == null)
			{
				return BadRequest("Отсутствуют данные для смены пароля");
			}
			if (formChangePassword.NewPassword != formChangePassword.ConfirmNewPassword)
			{
				return BadRequest("Пароли не совпадают");
			}

			try
			{
				PasswordValidation.Validate(formChangePassword.NewPassword);

				// Проверка наличия пользователя с таким email
				var existingUser = await _userRepository.GetByEmailAsync(formChangePassword.Email);
				if (existingUser != null)
				{
					return BadRequest("Пользователь с таким адресом электронной почты уже существует.");
				}

				// Создание уникального идентификатора для подтверждения email
				string emailVerificationId = Guid.NewGuid().ToString();

				// Отправка письма с запросом на подтверждение email
				if (!await _emailServices.SendConfirmationEmailAsync(formChangePassword.Email, emailVerificationId))
				{
					return BadRequest("Не удалось отправить письмо, попробуйте еще раз.");
				}

				// Добавление запроса на подтверждение email в словарь ожидающих запросов
				var tcs = new TaskCompletionSource<object>();
				_pendingRequests.TryAdd(emailVerificationId, tcs);

				// Ожидание подтверждения email
				await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromMinutes(1)));

				// Если email не подтвержден, возвращаем ошибку
				if (!tcs.Task.IsCompleted)
				{
					return BadRequest("Ожидание подтверждения email завершено.");
				}


				// Поиск пользователя по адресу электронной почты
				var user = await _userRepository.GetByEmailAsync(formChangePassword.Email);

				if (user is null)
				{
					return Unauthorized("Неверный адрес электронной почты.");
				}

				// Хэширование нового пароля и обновление в базе данных
				user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(formChangePassword.NewPassword);
				await _userRepository.UpdateAsync(user);

				// Создание JWT-токенов для пользователя
				var payload = СlientModelMappers.MapDbUserToJwtPayload(user);
				var accessToken = await _jwtTokenService.GenerateToken(payload);

				// Возвращение успешного результата с токенами доступа и обновления
				return Ok(accessToken);
			}
			catch
			{
				// Обработка ошибки
				return StatusCode(500, "Произошла ошибка.");
			}
		}
	}
}
