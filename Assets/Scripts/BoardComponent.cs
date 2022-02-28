using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using Unity.Jobs;
using UnityEngine;

namespace Checkers
{
    public class BoardComponent : MonoBehaviour
    {
        #region Variables and constants

        public event EventHandler<bool> CompletingBoardEvent;

        // Доска
        public const int BOARD_SIZE = 8;
        public GameObject[,] CellCollection { get; private set; }
        private int[,] InitialGameBoardMap { get; set; }

        /// <summary>Коллекция коллекций диагоналей на доске (черные диагонали для шашек).</summary>
        public List<LinkedList<BoardIndex>> BoardDiagonals { get; private set; }

        // Ячейки
        private Vector3 cellSize = new Vector3(1.0f, 0.2f, 1.0f);

        // Фишки 
        public List<GameObject> CheckerCollection { get; private set; }
        private Vector3 checkerScale = new Vector3(0.7f, 0.1f, 0.7f);
        /// <summary>Высота, на которую поднимается шашка относительно доски./summary>
        public Vector3 checkerStartTranslate = new Vector3(0f, 0.2f, 0f);

        // Материалы
        private Material blackCellMaterial;
        private Material blackChipMaterial;
        private Material whiteMaterial;

        #endregion

        #region MonoBehaviour methods

        private void Awake()
        {
            blackCellMaterial = (Material)Resources.Load("Materials/BlackCellMaterial", typeof(Material));
            blackChipMaterial = (Material)Resources.Load("Materials/BlackChipMaterial", typeof(Material));
            whiteMaterial = (Material)Resources.Load("Materials/WhiteMaterial", typeof(Material));
        }

        private void Start()
        {

            InitializeCollcections();
            InitializeBoard();
        }
        #endregion

        #region Methods

        private void InitializeCollcections()
        {
            CellCollection = new GameObject[BOARD_SIZE, BOARD_SIZE];

            CheckerCollection = new List<GameObject>(24);

            InitialGameBoardMap = new int[BOARD_SIZE, BOARD_SIZE] {
                { 0, 2, 0, 2, 0, 2, 0, 2 },
                { 2, 0, 2, 0, 2, 0 ,2, 0 },
                { 0, 2, 0, 2, 0, 2 ,0, 2 },
                { 0, 0, 0, 0, 0, 0 ,0 ,0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 1, 0, 1, 0, 1, 0, 1, 0 },
                { 0, 1, 0, 1, 0, 1, 0, 1 },
                { 1, 0, 1, 0, 1, 0, 1, 0 }
            };

            BoardDiagonals = new List<LinkedList<BoardIndex>>(11)
        {
            // goldWay 
            new List<BoardIndex>(8)
            {
                new BoardIndex("A1", 7, 0),
                new BoardIndex("B2", 6, 1),
                new BoardIndex("C3", 5, 2),
                new BoardIndex("D4", 4, 3),
                new BoardIndex("E5", 3, 4),
                new BoardIndex("F6", 2, 5),
                new BoardIndex("G7", 1, 6),
                new BoardIndex("H8", 0, 7),
            }.ToLinkedList(),

            // doubleWayG1A7
            new List<BoardIndex>(7)
            {
                new BoardIndex("G1", 7, 6),
                new BoardIndex("F2", 6, 5),
                new BoardIndex("E3", 5, 4),
                new BoardIndex("D4", 4, 3),
                new BoardIndex("C5", 3, 2),
                new BoardIndex("B6", 2, 1),
                new BoardIndex("A7", 1, 0)
            }.ToLinkedList(),

            // doubleWayH2B8
            new List<BoardIndex>(7)
            {
                new BoardIndex("H2", 6, 7),
                new BoardIndex("G3", 5, 6),
                new BoardIndex("F4", 4, 5),
                new BoardIndex("E5", 3, 4),
                new BoardIndex("D6", 2, 3),
                new BoardIndex("C7", 1, 2),
                new BoardIndex("B8", 0, 1)
            }.ToLinkedList(),

            // tripleWayC1A3
            new List<BoardIndex>(3)
            {
                new BoardIndex("C1", 7, 2),
                new BoardIndex("B2", 6, 1),
                new BoardIndex("A3", 5, 0)
            }.ToLinkedList(),

            // tripleWayC1H6
            new List<BoardIndex>(6)
            {
                new BoardIndex("C1", 7, 2),
                new BoardIndex("D2", 6, 3),
                new BoardIndex("E3", 5, 4),
                new BoardIndex("F4", 4, 5),
                new BoardIndex("G5", 3, 6),
                new BoardIndex("H6", 2, 7)
            }.ToLinkedList(),

            // tripleWayH6F8
            new List<BoardIndex>(3)
            {
                new BoardIndex("H6", 2, 7),
                new BoardIndex("G7", 1, 6),
                new BoardIndex("F8", 0, 5)
            }.ToLinkedList(),

            // tripleWayA3F8
            new List<BoardIndex>(6)
            {
                new BoardIndex("A3", 5, 0),
                new BoardIndex("B4", 4, 1),
                new BoardIndex("C5", 3, 2),
                new BoardIndex("D6", 2, 3),
                new BoardIndex("E7", 1, 4),
                new BoardIndex("F8", 0, 5)
            }.ToLinkedList(),

            // ultraWayA5D8
            new List<BoardIndex>(4)
            {
                new BoardIndex("A5", 3, 0),
                new BoardIndex("B6", 2, 1),
                new BoardIndex("C7", 1, 2),
                new BoardIndex("D8", 0, 3)
            }.ToLinkedList(),

            // ultraWayH4D8
            new List<BoardIndex>(5)
            {
                new BoardIndex("H4", 4, 7),
                new BoardIndex("G5", 3, 6),
                new BoardIndex("F6", 2, 5),
                new BoardIndex("E7", 1, 4),
                new BoardIndex("D8", 0, 3)
            }.ToLinkedList(),

            // ultraWayE1A5
            new List<BoardIndex>(5)
            {
                new BoardIndex("E1", 7, 4),
                new BoardIndex("D2", 6, 3),
                new BoardIndex("C3", 5, 2),
                new BoardIndex("B4", 4, 1),
                new BoardIndex("A5", 3, 0)
            }.ToLinkedList(),

            // ultraWayE1H4
            new List<BoardIndex>(4)
            {
                new BoardIndex("E1", 7, 4),
                new BoardIndex("F2", 6, 5),
                new BoardIndex("G3", 5, 6),
                new BoardIndex("H4", 4, 7)
            }.ToLinkedList(),

            // shortWay01
            new List<BoardIndex>(2)
            {
                new BoardIndex("A7", 1, 0),
                new BoardIndex("B8", 0, 1)
            }.ToLinkedList(),

            // shortWay02
            new List<BoardIndex>(2)
            {
                new BoardIndex("G1", 7, 6),
                new BoardIndex("H2", 6, 7)
            }.ToLinkedList(),
        };
        }

        /// <summary> Инициализирует игровую доску </summary>
        /// <remark>
        /// 1. Создает игровые объекты ячеек и назначает им CellComponent.
        /// 2. Масштабирует, перемещеат и окрашивает игровые объекты ячеек согласно правил шахматной доски.
        /// 3. Именует игровые объекты ячеек согласно правил шахматной доски.
        /// 4. Назначает родительским объектом для игровые объекты ячеек текущий игровой объект.
        /// </remark>
        private void InitializeBoard()
        {
            List<string> letters = new List<string>(8) { "A", "B", "C", "D", "E", "F", "G", "H" };
            List<int> numbers = new List<int>(8) { 8, 7, 6, 5, 4, 3, 2, 1 };

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int column = 0; column < BOARD_SIZE; column++)
                {
                    GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cell.name = $"{letters[column]}{numbers[row]}";
                    CellComponent сellComponent = cell.AddComponent<CellComponent>();
                    сellComponent.boardIndex = new BoardIndex(cell.name, row, column);

                    cell.transform.localScale = cellSize;
                    Vector3 position = Vector3.zero;
                    Material cellMaterial = whiteMaterial;

                    if (row == 0)
                    {
                        if (column == 0)
                        {
                            position = new Vector3(-3, 0, 3);
                            сellComponent.colorComponent = ColorType.White;
                        }
                        else
                        {
                            GameObject cellOnRight = CellCollection[row, column - 1];
                            position = new Vector3(cellOnRight.transform.position.x + 1, cellOnRight.transform.position.y, cellOnRight.transform.position.z);
                            if (cellOnRight.GetComponent<Renderer>().material.color == whiteMaterial.color)
                            {
                                cellMaterial = blackCellMaterial;
                                сellComponent.colorComponent = ColorType.Black;
                            }
                        }
                    }
                    else
                    {
                        GameObject topCell = (CellCollection[row - 1, column]);
                        position = new Vector3(topCell.transform.position.x, topCell.transform.position.y, topCell.transform.position.z - 1);
                        if (topCell.GetComponent<Renderer>().material.color == whiteMaterial.color)
                        {
                            cellMaterial = blackCellMaterial;
                            сellComponent.colorComponent = ColorType.Black;
                        }
                    }

                    cell.transform.Translate(position);
                    cell.GetComponent<Renderer>().materials = new Material[] { cellMaterial };
                    cell.transform.parent = transform;

                    CellCollection[row, column] = cell;

                    MakeChecker(cell.name, row, column, position);

                    // Публикация события о завершении формирования игровой доски.
                    if (row == 7 & column == 7)
                    {
                        CompletingBoardEvent?.Invoke(this, true);
                    }
                }
            }
        }

        /// <summary>Создает игровой объект фигуры шашки.</summary>
        /// <param name="row">Индекс строки.</param>
        /// <param name="column">Индекс столбца.</param>
        /// <param name="position">Позиция связанной ячейки.</param>
        /// <param name="cellName">Имя ячейки для привязки шашки.</param>
        /// <remark>
        /// 1. Создает, именует, трансформирует игровой объект фигуры шашки.
        /// 2. В зависимости от индекса в gameBoardMap выставляет игровой объект шашки
        /// на заданную позицию и присваивает ей цвет.
        /// 3. Добавляет объекту шашки CheckerComponent и сохраняет объект в список.
        /// </remark>>
        private void MakeChecker(string cellName, int row, int column, Vector3 position)
        {
            if (InitialGameBoardMap[row, column] == 0) return;

            GameObject checker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            checker.name = "Checker";
            checker.transform.localScale = checkerScale;
            checker.transform.position = position;
            checker.transform.Translate(checkerStartTranslate);

            CheckerComponent checkerComponent = checker.AddComponent<CheckerComponent>();
            checkerComponent.boardIndex = new BoardIndex(cellName, row, column);

            if (InitialGameBoardMap[row, column] == 1)
            {
                checkerComponent.colorComponent = ColorType.White;
                checker.GetComponent<Renderer>().materials = new Material[] { whiteMaterial };
                checkerComponent.playerCode = 1;
            }
            else if(InitialGameBoardMap[row, column] == 2)
            {
                checkerComponent.colorComponent = ColorType.Black;
                checker.GetComponent<Renderer>().materials = new Material[] { blackChipMaterial };
                checkerComponent.playerCode = 2;
            }

            CheckerCollection.Add(checker);
        }

        /// <summary>Возвращает количетво шашек из коллекции игровых объектов по заданному цвету.</summary>
        /// <param name="colorType">Заданный цвет для поиска.</param>
        /// <returns>Количество найденных шашек.</returns>
        public int CheckersCountByColor(ColorType colorType)
        {
            return CheckerCollection
                .Select(ch => ch.GetComponent<CheckerComponent>())
                .Where(ch => ch.colorComponent == colorType)
                .Count();
        }
        #endregion
    }

    // 0,0 | 0,1 | 0,2 | 0,3 | 0,4 | 0,5 | 0,6 | 0,7
    // 1,0 | 1,1 | 1,2 | 1,3 | 1,4 | 1,5 | 1,6 | 1,7
    // 2,0 | 2,1 | 2,2 | 2,3 | 2,4 | 2,5 | 2,6 | 2,7
    // 3,0 | 3,1 | 3,2 | 3,3 | 3,4 | 3,5 | 3,6 | 3,7
    // 4,0 | 4,1 | 4,2 | 4,3 | 4,4 | 4,5 | 4,6 | 4,7
    // 5,0 | 5,1 | 5,2 | 5,3 | 5,4 | 5,5 | 5,6 | 5,7
    // 6,0 | 6,1 | 6,2 | 6,3 | 6,4 | 6,5 | 6,6 | 6,7
    // 7,0 | 7,1 | 7,2 | 7,3 | 7,4 | 7,5 | 7,6 | 7,7
}

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        /// <summary>Универсальное расширение для IEnumerable, формирующее LinkedList.</summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="collection">Исходная коллекция.</param>
        /// <returns>LinkedList элементов заданного типа, сформированный  из исходной коллекции.</returns>
        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> collection)
        {
            LinkedList<T> linkedList = new LinkedList<T>();

            foreach (var item in collection)
            {
                LinkedListNode<T> lastNode = linkedList.Last;
                if (lastNode == null)
                    linkedList.AddFirst(item);
                else
                    linkedList.AddAfter(lastNode, item);
            }
            return linkedList;
        }
    }
}