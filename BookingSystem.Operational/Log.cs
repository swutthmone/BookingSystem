	using System;
	using Microsoft.Extensions.Logging;
	namespace BookingSystem.Operational
	{
		public static class Log
		{
			public static ILogger _logger;

			public static void Info(string message)
			{
				_logger.LogInformation(message);
			}

			public static void Warn(string message)
			{
				_logger.LogWarning(message);
			}

			public static void Error(string message)
			{
				_logger.LogError(message);
			}

			public static void Error(Exception ex, string message="")
			{
				_logger.LogError(ex, message);
			}

		}
	}