namespace Checkers.Interfaces
{
	/// <summary>Интерфейс издателя.</summary>
	public interface ISubject
	{
		///<summary>Присоединяет наблюдателя к издателю.</summary>>
		void Attach(IObserver observer);

		///<summary>Отсоединяет наблюдателя от издателя.</summary>>
		void Detach(IObserver observer);

		///<summary>Уведомляет всех наблюдателей о событии.</summary>>
		void Notify();
	}
}

