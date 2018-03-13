using System;
using System.Text;
using System.Threading;

namespace AndreiSmallUtils.Utils
{
	public class ProgressBar : IDisposable, IProgress<double> 
	{
		#region Constants

		private const int BLOCK_COUNT = 10;
		private const string ANIMATION = @"|/-\";

		#endregion

		#region Fields

		private readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0 / 8);

		private readonly Timer _timer;

		private double _currentProgress;
		private string _currentText = string.Empty;
		private bool _disposed;
		private int _animationIndex;

		#endregion

		#region Constructors

		public ProgressBar() {
			_timer = new Timer(TimerHandler);
			
			if (!Console.IsOutputRedirected) 
			{
				ResetTimer();
			}
		}

		#endregion

		#region Public methods

		public void Report(double value)
		{
			value = Math.Max(0, Math.Min(1, value));
			Interlocked.Exchange(ref _currentProgress, value);
		}

		#endregion

		#region Private methods

		private void TimerHandler(object state) {
			lock (_timer) 
			{
				if (_disposed) 
					return;

				var progressBlockCount = (int) (_currentProgress * BLOCK_COUNT);
				var percent = (int) (_currentProgress * 100);
				var text = $"[{new string('#', progressBlockCount)}" +
						   $"{new string('-', BLOCK_COUNT - progressBlockCount)}] " +
						   $"{percent,3}% {ANIMATION[_animationIndex++ % ANIMATION.Length]}";

				UpdateText(text);

				ResetTimer();
			}
		}

		private void UpdateText(string text) 
		{
			var commonPrefixLength = 0;
			var commonLength = Math.Min(_currentText.Length, text.Length);

			while (commonPrefixLength < commonLength && text[commonPrefixLength] == _currentText[commonPrefixLength])
				commonPrefixLength++;
			
			var outputBuilder = new StringBuilder();
			outputBuilder.Append('\b', _currentText.Length - commonPrefixLength);
			
			outputBuilder.Append(text.Substring(commonPrefixLength));
			
			var overlapCount = _currentText.Length - text.Length;

			if (overlapCount > 0) 
			{
				outputBuilder.Append(' ', overlapCount);
				outputBuilder.Append('\b', overlapCount);
			}

			Console.Write(outputBuilder);
			_currentText = text;
		}

		private void ResetTimer() 
		{
			_timer.Change(_animationInterval, TimeSpan.FromMilliseconds(-1));
		}

		#endregion

		#region Implementation of IDisposible

		public void Dispose() 
		{
			lock (_timer) 
			{
				_disposed = true;
				UpdateText(string.Empty);
			}
		}

		#endregion

	}
}
