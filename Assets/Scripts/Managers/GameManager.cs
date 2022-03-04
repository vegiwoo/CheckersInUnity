using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Checkers.Interfaces;
using Checkers.Helpers;

namespace Checkers.Managers
{
    public class GameManager : MonoBehaviour, ISubject
    {
        #region Variables and constants

        private CameraComponent cameraComponent;

        /// <summary>Игровая доска с ячейками и шашками.</summary>
        private BoardComponent boardComponent;
        /// <summary>Текущий игрок.</summary>
        private int currentPlayer;
        /// <summary>Режим игровой партии.</summary>
        [SerializeField] private CheckersPartyMode partyMode = CheckersPartyMode.Simple;

        /// <summary>Материал для выделения сущностей.</summary>
        private Material selectMaterial;

        /// <summary> Скорость перемещения шашек.</summary>
        [SerializeField] private float checkersMovementSpeed;
        /// <summary> Шашка, выбранная по клику.</summary>
        private CheckerComponent chosenChecker;
        private CheckerComponent ChosenChecker;
        /// <summary> Съеденная шашка.</summary>
        private (CheckerComponent victim, BoardIndex clickCellIndex) eatenChecker;
        /// <summary> Список вариантов ходов для выбранной шашки.</summary>
        private List<(CellComponent cell, bool isSelectedToMove)> moveOptionsForChosenChecker = new List<(CellComponent, bool)>();
        /// <summary> Новая позиция для выбранной шашки.</summary>
        private Vector3 newPositionOfSelectedChecker = Vector3.zero;
        private Coroutine movingCheckerCoroutine;

        /// <summary>Список подписчиков.</summary>
        private List<IObserver> observers = new List<IObserver>();
        /// <summary>Текущий игровой ход.</summary>
        private PlayStep? currentPlayStep;
        private PlayStep? CurrentPlayStep {
            get => currentPlayStep;
            set
            {
                currentPlayStep = value;
                if (currentPlayStep != null && observers.Count() > 0) Notify();
            }
        }

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

            checkersMovementSpeed = 0.8f;

            cameraComponent = Camera.main.GetComponent<CameraComponent>();
            cameraComponent.CameraRotationComplete += CompletingBoardHandler;
        }

        #endregion

        #region Event handlers

        /// <summary>Обработчик события завершение формирования доски.</summary>
        /// <param name="sender">Издатель события.</param>
        /// <param name="complete">Флаг окончания формирования игровой доски.</param>
        /// <remark>Подписывается на все заинтересованные события.</remark>>
        private void CompletingBoardHandler(object sender, bool complete)
        {
            SetSubscribeToCellEvents(true);
            SetSubscribeToCheckerEvents(true);
        }

        /// <summary> Осуществляет подписку на события ячейки.</summary>
        /// <param name="isSubscribe">Флаг подписки (true - подписаться, false - отписаться).</param>
        private void SetSubscribeToCellEvents(bool isSubscribe)
        {
            foreach (GameObject cellGO in boardComponent.CellCollection)
            {
                CellComponent cellComponent;
                cellGO.TryGetComponent(out cellComponent);
                if (cellComponent != null & cellComponent.colorComponent == ColorType.Black)
                {
                    if (isSubscribe)
                    {
                        cellComponent.OnFocusEventHandler += OnFocusedBaseClickComponentHandler;
                        cellComponent.OnClickEventHandler += OnClickBaseClickComponentHandler;
                    }
                    else
                    {
                        cellComponent.OnFocusEventHandler -= OnFocusedBaseClickComponentHandler;
                        cellComponent.OnClickEventHandler -= OnClickBaseClickComponentHandler;
                    }
                }
            }
        }

        /// <summary> Осуществляет подписку на события шашки.</summary>
        /// <param name="isSubscribe">Флаг подписки (true - подписаться, false - отписаться).</param>
        private void SetSubscribeToCheckerEvents(bool isSubscribe)
        {
            foreach (GameObject checkerGO in boardComponent.CheckerCollection)
            {
                CheckerComponent checkerComponent;
                checkerGO.TryGetComponent(out checkerComponent);
                if (checkerComponent != null)
                {
                    if(isSubscribe)
                        checkerComponent.OnClickEventHandler += OnClickBaseClickComponentHandler;
                    else
                        checkerComponent.OnClickEventHandler -= OnClickBaseClickComponentHandler;
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
            var targetCell = moveOptionsForChosenChecker
                    .Where(cell => cell.cell.boardIndex.Index == component.boardIndex.Index)
                    .FirstOrDefault();

            if (targetCell.cell != null) return;

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
                ChosenChecker = checkerComponent;

                //// Точка записи партии
                if (partyMode == CheckersPartyMode.Record)
                {                                                    
                     PlayStep step = new PlayStep(currentPlayer, "checker", ChosenChecker.boardIndex.Name, ActorActionType.Select);
                     SetCurrentPlayStep(step);
                }

                // Поиск ячеек для хода
                List<BoardIndex> availableCellIndices = GetAvailableIndexesToMove(component);

                foreach (var boardIndex in availableCellIndices)
                {
                    GameObject gameObject = boardComponent.CellCollection[boardIndex.Index.row, boardIndex.Index.column];

                    CellComponent cComponent;
                    gameObject.TryGetComponent(out cComponent);
                    if (cComponent != null)
                    {
                        AddGameObjectHighlight(cComponent);
                        moveOptionsForChosenChecker.Add((cComponent, false));
                    }
                }
            }
            // Обработка клика по ячейке
            else if (cellComponent != null & checkerComponent == null)
            {
                // Поиск ячейки, по которой кликнули в списке ячеек, доступных для хода выбранной шашки.
                var targetCell = moveOptionsForChosenChecker
                    .Where(cell => cell.cell.boardIndex.Index == cellComponent.boardIndex.Index)
                    .FirstOrDefault();

                if (targetCell.cell != null)
                {
                    targetCell.isSelectedToMove = true;

                    Vector3 cellPosition = cellComponent.gameObject.transform.position;
                    newPositionOfSelectedChecker = new Vector3(cellPosition.x, cellPosition.y + boardComponent.checkerStartTranslate.y, cellPosition.z);

                    // Блокировка пользовательского ввода
                    SetSubscribeToCellEvents(false);
                    SetSubscribeToCheckerEvents(false);

                    if (partyMode == CheckersPartyMode.Record)
                    {
                        PlayStep movePlayStep = new PlayStep(currentPlayer, "checker", ChosenChecker.boardIndex.Name, ActorActionType.Move, targetCell.cell.boardIndex.Name);
                        SetCurrentPlayStep(movePlayStep);
                    }

                    // Запуск корутины перещения шашки
                    movingCheckerCoroutine = StartCoroutine(MovingCheckerCoroutine(checkersMovementSpeed, targetCell.cell.boardIndex));
                }
                else
                {
                    ClearSelectionFromAllComponents();
                }
            }
        }

        #endregion

        #region Coroutines
        /// <summary>Корутина перемещения шашки на новую позицию.</summary>
        /// <param name="duration">Продолжительность перемещения.</param>
        /// <param name="targetIndex">Новый BoardIndex для перемещаемой шашки.</param>
        /// <returns>IEnumerator как результат выполнения корутины.</returns>
        private IEnumerator MovingCheckerCoroutine(float duration, BoardIndex targetIndex)
        {
            if (eatenChecker.clickCellIndex.Index == targetIndex.Index)
            {
                RemoveChecker(eatenChecker.victim.boardIndex);
            }

            float time = 0;
            Vector3 startPosition = ChosenChecker.transform.position;

            while (time < duration && newPositionOfSelectedChecker != Vector3.zero)
            {
                ChosenChecker.transform.position = Vector3.Lerp(startPosition, newPositionOfSelectedChecker, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            ChosenChecker.transform.position = newPositionOfSelectedChecker;

            movingCheckerCoroutine = null;

            CompletionAndTransferGameMove(targetIndex);

            yield break;
        }

        #endregion

        #region Subject methods

        public void Attach(IObserver observer)
        {
            observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void Notify()
        {
            PlayStep? playStep = CurrentPlayStep.GetValueOrDefault();

            if (playStep != null)
            {
                foreach (var observer in observers)
                {
                    observer.Update((PlayStep)playStep);
                }
            }
            CurrentPlayStep = null;
        }

        #endregion

        #region Other methods

        /// <summary>Получает полные игровые диагонали, связанные с переданным BoardIndex.</summary>
        /// <param name="boardIndex">BoardIndex как условие для поиска.</param>
        /// <returns>Возвращает список связанных списков диаоналей игровой доски, имеющие отношение к исходному BoardIndex.</returns>
        private List<LinkedList<BoardIndex>> GetDiagonalsForCheckers(BoardIndex boardIndex)
        {
            return boardComponent.BoardDiagonals
                .Where(list => list.Contains(boardIndex))
                .ToList();
        }

        /// <summary>Получает список элементов BoardIndex, доступных для хода или боя текущей шашки.</summary>
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

                LinkedListNode<BoardIndex> nextNode = default;

                switch (currentPlayer)
                {
                    case 1:
                        /// Находим предыдущую ноду относительно целевой
                        nextNode = targetNode.Next;
                        break;
                    case 2:
                        /// Находим следующую ноду относительно целевой
                        nextNode = targetNode.Previous;
                        break;
                    default:
                        break;
                }

                CheckerComponent checkerOnNextCell;

                if (nextNode != null)
                {
                    checkerOnNextCell = CheckingCheckerOnCell(nextNode.Value);

                    if (checkerOnNextCell == null)
                    {
                        availableCellIndices.Add(nextNode.Value);
                    }
                    else
                    {
                        if ((int)checkerOnNextCell.colorComponent != currentPlayer)
                        {
                            LinkedListNode<BoardIndex> fightNode = default;

                            switch (currentPlayer)
                            {
                                case 1:
                                    fightNode = nextNode.Next;
                                    break;
                                case 2:
                                    fightNode = nextNode.Previous;
                                    break;
                                default:
                                    break;
                            }

                            if (fightNode != null)
                            {
                                CheckerComponent hindrance02 = CheckingCheckerOnCell(fightNode.Value);

                                if (hindrance02 == null)
                                {
                                    eatenChecker.victim = CheckingCheckerOnCell(nextNode.Value);
                                    eatenChecker.clickCellIndex = fightNode.Value;

                                    availableCellIndices.Add(fightNode.Value);
                                }
                            }
                        }
                    }
                }
            }
            return availableCellIndices;
        }

        /// <summary> Находит компонент шашки для по указанному индексу.</summary>
        /// <param name="boardIndex">Указанный индекс для поиска.</param>
        /// <returns>Найденный компонент шашаки или null</returns>
        private CheckerComponent CheckingCheckerOnCell(BoardIndex boardIndex)
        {
            return boardComponent.CheckerCollection
                .Select(go => go.GetComponent<CheckerComponent>())
                .Where(ch => ch.boardIndex.Name == boardIndex.Name)
                .FirstOrDefault();
        }

        /// <summary>Удаляет шашку с игрового поля для по указанному индексу.</summary>
        /// <param name="boardIndex">Указанный индекс для поиска.</param>
        /// <returns>Булев флаг выполнения операции.</returns>
        private bool RemoveChecker(BoardIndex boardIndex)
        {
            CheckerComponent checkerComponent = CheckingCheckerOnCell(boardIndex);

            if (checkerComponent != null)
            {
                // Точка записи партии
                if (partyMode == CheckersPartyMode.Record)
                {
                    PlayStep playStep = new PlayStep(currentPlayer, ActorActionType.Remove, checkerComponent.boardIndex.Name);
                    SetCurrentPlayStep(playStep);
                }

                boardComponent.CheckerCollection.Remove(checkerComponent.gameObject);
                Destroy(checkerComponent.gameObject);
                eatenChecker = default;
                return true;
            }
            else
            {
                return false;
            }
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

        /// <summary>Очищает выделение со всех компонентов на доске.</summary>
        /// <remark>Дополнительно удаляет выбранную шашку и ее возможные ходы.</remark>>
        private void ClearSelectionFromAllComponents()
        {
            ChosenChecker = default;
            moveOptionsForChosenChecker.Clear();

            List<BaseClickComponent> baseClickComponents = new List<BaseClickComponent>();

            List<CellComponent> cellComponentsa =
                (
                    from i in Enumerable.Range(0, boardComponent.CellCollection.GetLength(0))
                    from j in Enumerable.Range(0, boardComponent.CellCollection.GetLength(1))
                    select (boardComponent.CellCollection[i, j].GetComponent<CellComponent>())
                )
                .Where(comp => comp.IsSelected)
                .ToList();

            baseClickComponents.AddRange(cellComponentsa);

            List<CheckerComponent> checkerComponents = boardComponent.CheckerCollection
                .Select(ch => ch.GetComponent<CheckerComponent>())
                .Where(cc => cc.IsSelected)
                .ToList();

            baseClickComponents.AddRange(checkerComponents);

            foreach (var item in baseClickComponents)
            {
                RemoveGameObjectHighlight(item);
                eatenChecker = default;
            }
        }

        /// <summary> Выполняет операции после завершении хода шашкой.</summary>
        /// <param name="targetIndex">Новый BoardIndex для перемещаемой шашки.</param>
        private void CompletionAndTransferGameMove(BoardIndex targetIndex)
        {
            newPositionOfSelectedChecker = Vector3.zero;

            ChosenChecker.boardIndex = targetIndex;

            // Проверка условия победы  - если целевая шашка дамка или у оппонента занончились фигуры
            int opponentCheckersCount = 12;
            switch (ChosenChecker.colorComponent)
            {
                case ColorType.White:
                    opponentCheckersCount = boardComponent.CheckersCountByColor(ColorType.Black);
                    if (ChosenChecker.boardIndex.Index.row == 0)
                    {
                        ChosenChecker.isLady = true;
                    }
                    break;
                case ColorType.Black:
                    opponentCheckersCount = boardComponent.CheckersCountByColor(ColorType.White);
                    if (ChosenChecker.boardIndex.Index.row == 7)
                    {
                        ChosenChecker.isLady = true;
                    }
                    break;
                default:
                    break;
            }

            if (ChosenChecker.isLady || opponentCheckersCount == 0)
            {
                string side = ChosenChecker.colorComponent == ColorType.White ? "Белые" : "Черные";

                print($"{side} победили!");
                UnityEditor.EditorApplication.isPaused = true;
            }

            ClearSelectionFromAllComponents();

            SwitchPlayer();

            cameraComponent.MoveHorizontal(1.0f, currentPlayer == 1 ? ColorType.White : ColorType.Black);
        }

        /// <summary>Производит смену игрока.</summary>
        private void SwitchPlayer()
        {
            currentPlayer = currentPlayer == 1 ? 2 : 1;
        }

        /// <summary>Назначает переданный игровой ход текущим.</summary>
        /// <param name="playStep"></param>
        private void SetCurrentPlayStep(PlayStep playStep)
        {
            CurrentPlayStep = playStep;
        }

        #endregion
    }
}

// Новый компонент:
// 
// - воспроиводит записанную партию по выбору в редакторе, пользовательский ввод выклчается, задержка между ходами, лог кажого хода
// - в редакторе выбиратся флажок записи игры / флажок для воспроизведения записанной игры 
// - запись после каждого хода