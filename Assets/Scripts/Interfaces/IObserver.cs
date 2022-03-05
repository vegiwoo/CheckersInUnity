using System.Collections;
using System.Collections.Generic;
using Checkers.Helpers;

namespace Checkers.Interfaces
{
	/// <summary>Интерфейс наблюдателя.</summary>
	public interface IObserver
	{
		/// <summary>Получает команду на сброс прошлого обозрения и запись нового.</summary>
		void Record();

		/// <summary>Получает стек задач и их описаний на реплей обозрения.</summary>
		/// <typeparam name="IPlayStepable">Тип для парсинга данных.</typeparam>
		/// <returns>Стек с этапами обозрения и их описанием.</returns>
		Stack<(IPlayStepable playSter, string description)> Replay<IPlayStepable>();

		/// <summary>Получает обновление от издателя.</summary>
		/// <param name="playStep">Игровой ход.</param>
		void Update(PlayStep playStep);
	}

	public interface IPlayStepable
	{
		/// <summary>Имя игрока.</summary>
		public int PlayerNumber { get; set; }
		/// <summary>Имя актора (игрового объекта).</summary>
		public string ActorName { get; set; }
		/// <summary>Начальное положение актора (игрового объекта).</summary>
		public string ActorSource { get; set; }
		/// <summary>Тип активности актора (игрового объекта).</summary>
		public ActorActionType ActorAction { get; set; }
		/// <summary>Конечное положение актора (игрового объекта).</summary>
		public string ActorTarget { get; set; }
	}
}