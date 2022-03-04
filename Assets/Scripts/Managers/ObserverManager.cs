using UnityEngine;
using Checkers.Helpers;
using Checkers.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace Checkers.Managers
{
    // Записывает и вопроизводит ходы игрока

    public class ObserverManager : IObserver
    {
        private static string RECORD_FILE_NAME = @"/Record.txt";
        private string recordFilePath = @"/Record.txt";

        public ObserverManager()
        {
            recordFilePath = CheckRecordFile();
        }

        public void Update(PlayStep playStep)
        {
            RecordPlayStep(recordFilePath, playStep.StepToDescription());



            // по запросу и при наличии файла может распарсить текст в список PlayStep и вернуть в основой объект для вопроизведения
        }

        /// <summary>Проверяет доступность файла для записи.</summary>
        /// <returns>Пут к файлу.</returns>
        private string CheckRecordFile()
        {
            string path = Environment.CurrentDirectory + RECORD_FILE_NAME;

            if (!File.Exists(path))
            {
                FileStream fileStream = File.Create(path);
                fileStream.Close();
            }

            return path;
        }

        private async void RecordPlayStep(string path, string playStepDescrition)
        {
            if (File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    await writer.WriteLineAsync(playStepDescrition);
                }
            }
        }
    }
}

// - файл читаемый человеком

// Player 1: select checker A1
// Player 1: move checker A1 A3
// Player 2: remove checker A2