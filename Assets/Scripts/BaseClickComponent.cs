using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    /// <summary> Перечисление, определяющее цвета ячеек и шашек.</summary>
    public enum ColorType { White, Black }

    /// <summary> Сущность для фиксации расположения игрового элемента в границах игровой доски.</summary>
    public struct BoardIndex
    {
        /// <summary> Имя компонента (соответсвует индексу шахматной доски A1, B2 и тп).</summary>
        public string Name { get; private set; }
        /// <summary> Парный индекс строки и столбца элемента на игровой доске.</summary>
        public (int row, int column) Index { get; private set; }

        public BoardIndex(string name, int row, int column)
        {
            this.Name = name;
            this.Index = (row, column);
        }

        public override bool Equals(object obj)
        {
            return obj is BoardIndex && ((BoardIndex)obj).Name == Name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public abstract class BaseClickComponent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region Variables and constants

        public BoardIndex boardIndex;
        public ColorType colorComponent;
        public bool IsSelected { get; set; }

        /// <summary>Делегат события наведения и сброса наведения c игрового объекта.</summary>
        public delegate void FocusEventHandler(BaseClickComponent component, bool isSelect);
        /// <summary>Событие фокусировки на игровом объекте.</summary>
        public event FocusEventHandler OnFocusEventHandler;

        /// <summary>Делегат события клика по игровому объекту.</summary>
        public delegate void ClickEventHandler(BaseClickComponent component);
        /// <summary> Событие клика на игровом объекте.</summary>
        public event ClickEventHandler OnClickEventHandler;

        #endregion

        #region MonoBehaviour methods

        protected void Start()
        {
            IsSelected = false;
        }
        #endregion

        #region Event handlers

        /// <summary>...</summary>
        /// <param name="target"></param>
        /// <param name="isSelect"></param>
        /// <remark>
        /// Если есть дочерние классы, этот метод можно вызывать в них -
        /// вызов пробрасыывется от дочернего класса в родительский.
        /// </remark>
        protected void CallBackEvent(BaseClickComponent target, bool isSelect)
        {
            OnFocusEventHandler?.Invoke(target, isSelect);
        }

        /// <summary>Вызывается при наведении курсора на игровой объект.</summary>
        /// <param name="eventData"></param>
        /// <remark>
        /// При наведении на фигуру должна подсвечиваться клетка под ней.
        /// При наведении на клетку должна подсвечиваться сама клетка.
        /// </remark>remark>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            CallBackEvent(this, true);
        }

        /// <summary>Вызывается при завершении наведения курсора на игровой объект.</summary>
        /// <param name="eventData"></param>
        /// <remark>Необходимо снимать подсветку с клетки.</remark>>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            CallBackEvent(this, false);
        }

        /// <summary>Вызывается по клику курсом на объект.</summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickEventHandler?.Invoke(this);
        }

        #endregion

        #region Methods

        #endregion
    }
}
