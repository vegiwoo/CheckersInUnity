using UnityEngine.EventSystems;

namespace Checkers
{
    public class CheckerComponent : BaseClickComponent
    {
        // Код игрока
        public int playerCode = 0;

        // Является ли шашка дамкой
        public bool isLady = false;
    }
}
