using Checkers.Helpers;

namespace Checkers.Interfaces
{
	/// <summary>Интерфейс наблюдателя.</summary>
	public interface IObserver
	{
		/// <summary>Получает обновление от издателя.</summary>
		/// <param name="playStep">Игровой ход.</param>
		void Update(PlayStep playStep);
	}
}