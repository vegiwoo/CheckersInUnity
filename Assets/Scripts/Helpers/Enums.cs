using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Checkers.Helpers
{
	/// <summary> Типы партии в шашки.</summary>
	public enum CheckersPartyMode {
		[Description("Simple")]
		Simple, // Обычный
		[Description("Record")]
		Record, // Обычный с записью
		[Description("Replay")]
		Replay  // Воспроизведение 
	}

	/// <summary>Тип активности актора (игрового объекта).</summary>
	public enum ActorActionType {
		[Description("Select")]
		Select,
		[Description("Move")]
		Move,
		[Description("Remove")]
		Remove
	}

}

namespace Checkers.Helpers
{
    public static class MyExtensions
    {
        /// <summary>Перемешивает элементы списка.</summary>
        /// <typeparam name="T">Тип элемента списка.</typeparam>
        /// <param name="list">Список, указывающий на себя.</param>
        /// <returns>Список с перемешанными элементами.</returns>
        public static List<T> Shuffle<T>(this List<T> list)
        {
            Random random = new Random();

            for (int i = list.Count() - 1; i >= 1; i--)
            {
                int j = random.Next(i + 1);
                var temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }

            return list;
        }

        /// <summary>Преобразует списое в стек.</summary>
        /// <typeparam name="T">Тип элемента списка.</typeparam>
        /// <param name="list">Список, указывающий на себя.</param>
        /// <returns>Стек с элементами списка.</returns>
        public static Stack<T> ToStack<T>(this List<T> list)
        {
            Stack<T> stack = new Stack<T>(list.Count());

            foreach (var item in list)
            {
                stack.Push(item);
            }
            return stack;
        }

        /// <summary>Возращает строковое описание кейса перечисления.</summary>
        /// <param name="value">Перечисление</param>
        /// <returns>Строковое описание кейса.</returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }
}