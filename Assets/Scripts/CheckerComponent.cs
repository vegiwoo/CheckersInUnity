using UnityEngine.EventSystems;

namespace Checkers
{
    public class CheckerComponent : BaseClickComponent
    {
        /// <summary>Код игрока и цвета шашки</summary>
        public int playerCode = 0;

        /// <summary>Является ли шашка дамкой.</summary>
        public bool isLady = false;
    }
}
