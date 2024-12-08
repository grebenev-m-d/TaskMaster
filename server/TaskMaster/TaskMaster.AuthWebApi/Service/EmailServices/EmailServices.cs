using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataWebApi.Helpers;
using TaskMaster.Validation;

namespace CommonLib.Services.EmailServices
{
	/// <summary>
	/// Сервис для отправки электронных писем.
	/// </summary>
	public class EmailServices : IEmailServices
	{
		private readonly ILogger<EmailServices> _logger;
		private readonly IConfiguration _configuration;

		// Путь к шаблону письма
		private const string TemplateRelativePath = @"server\TaskMaster\TaskMaster.AuthWebApi\Resources\EmailConfirmation.html";

		/// <summary>
		/// Конструктор класса EmailServices.
		/// </summary>
		/// <param name="logger">Логгер.</param>
		/// <param name="configuration">Конфигурация.</param>
		public EmailServices(ILogger<EmailServices> logger,
			IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
		}

		/// <summary>
		/// Метод для отправки электронного письма.
		/// </summary>
		/// <param name="email">Email получателя.</param>
		/// <param name="linkConfirmation">Ссылка для подтверждения.</param>
		/// <returns>Успешность отправки письма.</returns>
		public async Task<bool> SendConfirmationEmailAsync(string email, string linkConfirmation)
		{
			// Проверка валидности email
			if (EmailValidation.Validate(email))
			{
				throw new ArgumentException("Некорректная почта.");
			}

			// Получение настроек SMTP
			var smtpServer = _configuration["SmtpSettings:SmtpServer"];
			var port = int.Parse(_configuration["SmtpSettings:Port"]);

			// Получение информации о шаблоне письма и отправителе
			var template = _configuration["EmailTemplates:TemplateUrl"];
			var templateUrl = string.Format(template, linkConfirmation);
			var senderName = _configuration["SenderInfo:CompanyName"];
			var recipientName = _configuration["RecipientInfo:RecipientName"];
			var recipientEmail = _configuration["RecipientInfo:RecipientEmail"];
			var subject = _configuration["EmailSubject"];
			var authenticateEmail = _configuration["Authenticate:Email"];
			var authenticatePassword = _configuration["Authenticate:Password"];

			try
			{
				MimeMessage message = new MimeMessage();
				message.From.Add(new MailboxAddress(senderName, email));
				message.To.Add(new MailboxAddress(recipientName, recipientEmail));
				message.Subject = subject;

				// Формирование тела письма с использованием HTML-шаблона
				message.Body = new BodyBuilder()
				{
					HtmlBody = string.Format(GetEmailConfirmationTemplate, templateUrl)
				}.ToMessageBody();

				// Отправка письма через SMTP
				using (MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
				{
					client.Connect(smtpServer, port, SecureSocketOptions.StartTls);
					client.Authenticate(authenticateEmail, authenticatePassword);
					client.Send(message);
					client.Disconnect(true);
					_logger.LogInformation("Message sent successfully.");
				}

				return true;
			}
			catch (Exception e)
			{
				// Логирование ошибки при отправке письма
				_logger.LogError(e.GetBaseException().Message);
				return false;
			}
		}

		// Получение содержимого шаблона письма из файла
		private string GetEmailConfirmationTemplate => File.ReadAllText(FilePathHelper.GetFullPath(TemplateRelativePath));
	}
}
