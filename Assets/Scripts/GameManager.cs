using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

// Подписывается на все события ячеек и шашек, их родителя
// Запрещает ходить более одного раза одной стороне, предупреждет в консоли

// Заданные условия победы
// - простые - первая шашка проходит в дамки или съедены все фишки противника
// - традиционные - съедены все фишки противника

// Условия перемещения
// Общие
// - при наведении курсора на клетку она подсвечивается, при перемещении теряет подсветку
// - при выборе фишки нужно подсветить на какие она может перемещаться
// - выбранная шашка подсвечивается
// - шашка перемещается только на пустую клетку
// - шашка перемещается только вперед
// - шашка перемещается только по черным клеткам и только на 1 клетку

// Условия боя
// - при уничтожеии шашки противника текущая шашка встает за ней

// Передаа хода
// - ввод блокируется
// - камера через корутину плавно перемещается на другую сторону

// Не может содержать публичные методы
// Перемещение фишек - плавно и через корутину
// Во время перемещения пользовательский ввод блокируется

namespace Checkers
{
    public class GameManager : MonoBehaviour
    {
        #region Variables and constants

        /// <summary>Игровая доска с ячейками и шашками.</summary>
        private BoardComponent boardComponent;
        /// <summary>Текущий игрок.</summary>
        private int currentPlayer;
        /// <summary>Материал для выделения сущностей.</summary>
        private Material selectMaterial;
        /// <summary> Выбранная шашка.</summary>
        private CheckerComponent selectedChecker;
        /// <summary> Спискок вариантов ходов для выбранной шашки.</summary>
        private List<CellComponent> moveOptions = new List<CellComponent>();
        
        #endregion

        #region MonoBehaviour methods

        private void Awake()
        {
            selectMaterial = (Material)Resources.Load("Materials/OrangeMaterial", typeof(Material));
        }

        private void Start()
        {
            boardComponent = gameObject.AddComponent<BoardComponent>();
            boardComponent.CompletingBoardEvent += CompletingBoardHandler;

            currentPlayer = 1;
        }

        #endregion

        #region Event handlers

        /// <summary>Обработчик события завершение формирования доски.</summary>
        /// <param name="sender">Издатель события.</param>
        /// <param name="complete">Флаг окончания формирования игровой доски.</param>
        /// <remark>Плдписывается на все заинтересованные события.</remark>>
        private void CompletingBoardHandler(object sender, bool complete)
        {
            SetSubscribeToCellEvents();
            SetSubscribeToCheckerEvents();
        }

        /// <summary> Осуществляет подписку на события ячейки.</summary>
        private void SetSubscribeToCellEvents()
        {
            foreach (GameObject cellGO in boardComponent.cellCollection)
            {
                CellComponent cellComponent;
                cellGO.TryGetComponent(out cellComponent);
                if (cellComponent != null & cellComponent.colorComponent == ColorType.Black)
                {
                    cellComponent.OnFocusEventHandler += OnFocusedBaseClickComponentHandler;
                    cellComponent.OnClickEventHandler += OnClickBaseClickComponentHandler;
                }
            }
        }

        /// <summary> Осуществляет подписку на события шашки.</summary>
        private void SetSubscribeToCheckerEvents()
        {
            foreach (GameObject checkerGO in boardComponent.checkerCollection)
            {
                CheckerComponent checkerComponent;
                checkerGO.TryGetComponent(out checkerComponent);
                if (checkerComponent != null)
                {
                    checkerComponent.OnClickEventHandler += OnClickBaseClickComponentHandler;
                }
            }
        }

        /// <summary>Обрабатывает фокусировку на компоненте типа BaseClickComponent под курсором.</summary>
        /// <param name="component">Компонент, попадающий под курсор.</param>
        /// <param name="isFocused">Сфокусирован ли курсор на объекте.</param>
        /// <remark>Исключается фокусировка на ячейках, определенных по клику на шашку
        /// как возможных для перемещения.</remark>
        private void OnFocusedBaseClickComponentHandler(BaseClickComponent component, bool isFocused)
        {
            if (moveOptions.Contains(component)) return;

            if (isFocused)
                AddGameObjectHighlight(component);
            else
                RemoveGameObjectHighlight(component);
        }

        /// <summary>Обрабатывает клик по компоненту типа BaseClickComponent.</summary>
        /// <param name="component">Компонент, попадающий под клик.</param>
        private void OnClickBaseClickComponentHandler(BaseClickComponent component)
        {
            CheckerComponent checkerComponent = component as CheckerComponent;
            CellComponent cellComponent = component as CellComponent;

            // Обработка клика по шашке
            if (checkerComponent != null & cellComponent == null)
            {
                // Запрет обработки клика по шашкам противника.
                if (checkerComponent.playerCode != currentPlayer) return;

                ClearSelectionFromAllComponents();

                AddGameObjectHighlight(checkerComponent);
                selectedChecker = checkerComponent;

                List<BoardIndex> availableCellIndices = GetAvailableIndexesToMove(component);

                foreach (var boardIndex in availableCellIndices)
                {
                    GameObject gameObject = boardComponent.cellCollection[boardIndex.Index.row, boardIndex.Index.column];

                    CellComponent cComponent;
                    gameObject.TryGetComponent(out cComponent);
                    if (cComponent != null)
                    {
                        AddGameObjectHighlight(cComponent);
                        moveOptions.Add(cComponent);
                    }
                }
            }
            // Обработка клика по ячейке
            else if (cellComponent != null & checkerComponent == null)
            {
                ClearSelectionFromAllComponents();
            }
        }

        #endregion

        #region Other methods

        /// <summary>Получает полные игровые диагонали, связанные с переданным BoardIndex.</summary>
        /// <param name="boardIndex">BoardIndex как условие для поиска.</param>
        /// <returns>Возвращает список связанных списков диаоналей игровой доски, имеющие отношение к исходному BoardIndex.</returns>
        private List<LinkedList<BoardIndex>> GetDiagonalsForCheckers(BoardIndex boardIndex)
        {
            return boardComponent.boardDiagonals
                .Where(list => list.Contains(boardIndex))
                .ToList();
        }

        /// <summary>Получает список элементов BoardIndex, доступных для хода текущей шашки.</summary>
        /// <param name="component">Компонент текущей шашки.</param>
        /// <returns>Список доступных BoardIndex.</returns>
        private List<BoardIndex> GetAvailableIndexesToMove(BaseClickComponent component)
        {
            List<LinkedList<BoardIndex>> boardIndicesList = GetDiagonalsForCheckers(component.boardIndex);

            List<BoardIndex> availableCellIndices = new List<BoardIndex>();

            foreach (var boardIndicesLinkedList in boardIndicesList)
            {
                // Находим целевую ноду в конкретной диагонали
                LinkedListNode<BoardIndex> targetNode = boardIndicesLinkedList
                    .Find(component.boardIndex);

                //// Находим предыдущую ноду относительно целевой
                //LinkedListNode<BoardIndex> previousNode = targetNode.Previous;
                //if (previousNode != null) availableCellIndices.Add(previousNode.Value);

                // Находим следующую ноду относительно целевой
                LinkedListNode<BoardIndex> nextNode = targetNode.Next;

                if (nextNode != null && CheckingCheckerOnCell(nextNode.Value) == null)
                {
                    availableCellIndices.Add(nextNode.Value);
                }
            }

            return availableCellIndices;
        }

        /// <summary> Находит компонент шашки для по указанному индексу.</summary>
        /// <param name="boardIndex">Указанный индекс для поиска.</param>
        /// <returns>Найденный компонент шашаки или null</returns>
        private CheckerComponent CheckingCheckerOnCell(BoardIndex boardIndex)
        {
            return boardComponent.checkerCollection
                .Select(go => go.GetComponent<CheckerComponent>())
                .Where(ch => ch.boardIndex.Name == boardIndex.Name)
                .FirstOrDefault();
        }

        /// <summary>Добавляет выделение игровому объекту.</summary>
        /// <param name="gameObject">Переданный игровой объект.</param>
        private void AddGameObjectHighlight(BaseClickComponent component)
        {
            Material[] currentMaterials = component.gameObject.GetComponent<Renderer>().materials;

            if (currentMaterials.Count() == 2) return;

            Array.Resize(ref currentMaterials, currentMaterials.Length + 1);
            currentMaterials[currentMaterials.Length - 1] = selectMaterial;

            component.gameObject.GetComponent<Renderer>().materials = currentMaterials;
            component.IsSelected = true;
        }

        /// <summary>Снимает ранее добавленное выделение игровому объекту.</summary>
        /// <param name="gameObject">Переданный игровой объект.</param>
        private void RemoveGameObjectHighlight(BaseClickComponent component)
        {
            Material[] currentMaterials = component.gameObject.GetComponent<Renderer>().materials;

            if (currentMaterials.Count() == 1) return;

            Array.Resize(ref currentMaterials, currentMaterials.Length - 1);

            component.gameObject.GetComponent<Renderer>().materials = currentMaterials;
            component.IsSelected = false;
        }

        /// <summary> Очищает выделение со всех компонентов.</summary>
        private void ClearSelectionFromAllComponents()
        {
            selectedChecker = default;
            moveOptions.Clear();

            List<BaseClickComponent> baseClickComponents = new List<BaseClickComponent>();

            List<CellComponent> cellComponentsa =
                (
                    from i in Enumerable.Range(0, boardComponent.cellCollection.GetLength(0))
                    from j in Enumerable.Range(0, boardComponent.cellCollection.GetLength(1))
                    select (boardComponent.cellCollection[i, j].GetComponent<CellComponent>())
                )
                .Where(comp => comp.IsSelected)
                .ToList();

            baseClickComponents.AddRange(cellComponentsa);

            List<CheckerComponent> checkerComponents = boardComponent.checkerCollection
                .Select(ch => ch.GetComponent<CheckerComponent>())
                .Where(cc => cc.IsSelected)
                .ToList();

            baseClickComponents.AddRange(checkerComponents);

            foreach (var item in baseClickComponents)
            {
                RemoveGameObjectHighlight(item);
            }
        }

        /// <summary>Производит смену игрока.</summary>
        private void SwitchPlayer()
        {
            currentPlayer = currentPlayer == 1 ? 2 : 1;
        }

        #endregion
    }
}