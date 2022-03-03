using Checkers.Helpers;
using Checkers.Interfaces;

namespace Checkers.Managers
{
    // Записывает и вопроизводит ходы игрока

    public class ObserverManager : IObserver
    {
        public void Update(PlayStep playStep)
        {
            // - считывает из playStep 
            // Player 1: checker B4 move to B4
            // Checker B4 remove checker B6
            // записывает эти действия в отдельный файл

            // по запросу и при наличии файла может распарсить текст в список PlayStep и вернуть в основой объект для вопроизведения
        }
    }
}

// - файл читаемый человеком