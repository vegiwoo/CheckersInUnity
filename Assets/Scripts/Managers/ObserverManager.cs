using Checkers.Helpers;
using Checkers.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Checkers.Managers
{
    ///<summary>Записывает и воспроизводит ходы игроков.</summary>
    public class ObserverManager : IObserver
    {
        #region Variables and constants

        private static string RECORD_FILE_NAME = "Record.txt";
        private string rootPath = default;
        private string recordFilePath = default;

        #endregion

        #region Constructors

        public ObserverManager()
        {
            rootPath = Environment.CurrentDirectory;
            recordFilePath = CheckRecordFile();
        }

        #endregion

        #region Methods

        /// <summary>Проверяет существование файла записи.</summary>
        /// <returns>Флаг проверки.</returns>
        private bool RecordFileExists()
        {
            return File.Exists(Path.Combine(rootPath, RECORD_FILE_NAME));
        }

        /// <summary>Удаляет файл записи обозрения.</summary>
        public void Record()
        {
            if (RecordFileExists())
            {
                try
                {
                    File.Delete(Path.Combine(rootPath, RECORD_FILE_NAME));
                    Debug.Log("Observation record rile Deleted.");
                }
                catch (IOException ioException)
                {
                    Debug.LogError(ioException.Message);
                }
            }

            recordFilePath = CheckRecordFile();
        }

        public Stack<(IPlayStepable playSter, string description)> Replay<IPlayStepable>()
        {
            List<(IPlayStepable playSter, string description)> list = new List<(IPlayStepable playSter, string description)>();

            if (RecordFileExists())
            {
                using (StreamReader reader = new StreamReader(recordFilePath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        IPlayStepable playStep = (IPlayStepable)PlayStep.DesciptionToStep(line);
                        list.Add((playStep, line));
                    }
                }
            }

            list.Reverse();
            return list.ToStack();
        }

        /// <summary>Метод обновления логики обозревателя.</summary>
        /// <param name="playStep">Игровой шаг.</param>
        public void Update(PlayStep playStep)
        {
            RecordPlayStep(recordFilePath, playStep.StepToDescription());
        }

        /// <summary>Проверяет доступность файла для записи.</summary>
        /// <returns>Путь к файлу.</returns>
        private string CheckRecordFile()
        {
            if (!RecordFileExists())
            {
                FileStream fileStream = File.Create(Path.Combine(rootPath, RECORD_FILE_NAME));
                Debug.Log("Observation record file recreated.");
                fileStream.Close();
            }

            return Path.Combine(rootPath, RECORD_FILE_NAME);
        }

        /// <summary>Записывает игровой шаг.</summary>
        /// <param name="path">Путь к файлу записи обозрения.</param>
        /// <param name="playStepDescrition">Описание игрового шага.</param>
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

        #endregion
    }
}