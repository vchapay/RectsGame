using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using WhyIDidntKnowThisGameEarlies.ConverterClass;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    /// <summary>
    /// Предоставляет поток и функционал для заргузки реплеев из файла.
    /// </summary>
    public static class ReplayFileReader
    {
        private static ExcelPackage package;
        private static ExcelWorksheet listOfMoves;
        private static ExcelWorksheet listOfReplays;
        private static ExcelWorksheet listOfRules;
        private static StreamReader stream;

        /// <summary>
        /// Пробует загрузить список реплеев из файла по указанному пути 
        /// либо используя путь по умолчанию (replays.xlsx). При отсутствии файла или записей в нем
        /// возвращает пустую коллекцию. Файл должен иметь расширение .xlsx
        /// </summary>
        /// <param name="path"> Путь к целевому файлу </param>
        /// <returns> Коллекция загруженных реплеев </returns>
        public static List<Replay> TryLoadReplays(string path = "replays.xlsx")
        {
            if (path == null)
                path = "replays.xlsx";

            try
            {
                stream = new StreamReader(path);
                PreparePackage();
                return Load();
            }

            catch
            {
                return new List<Replay>();
            }

            finally 
            {
                stream?.Close(); 
                package?.Dispose(); 
            }
        }

        private static List<Replay> Load()
        {
            List<Replay> replays = new List<Replay>();
            int repInd = 1;
            string id = listOfReplays.Cells[repInd, 1].Value?.ToString();

            if (id == null)
                return replays;

            while (id != null)
            {
                try
                {
                    Replay rep = LoadReplay(repInd, id);
                    replays.Add(rep);
                }
                catch { }

                repInd++;
                id = listOfReplays.Cells[repInd, 1].Value?.ToString();
            }

            return replays;
        }

        private static Replay LoadReplay(int repInd, string id)
        {
            ReplayBase replayBase = CreateReplayBase(repInd, id);
            Rule rule = CreateReplayRule(repInd);
            List<Move> moves = ReadMovesList(repInd);
            Replay rep = Replay.Construct(replayBase, rule, moves);
            return rep;
        }

        private static List<Move> ReadMovesList(int repInd)
        {
            int moveInd = 2;
            string moveStr = listOfMoves.Cells[repInd, moveInd].Value?.ToString();

            List<Move> moves = new List<Move>();

            while (moveStr != null)
            {
                if (!Move.TryParse(moveStr, out Move move))
                    return moves;

                moves.Add(move);
                moveInd++;
                moveStr = listOfMoves.Cells[repInd, moveInd].Value?.ToString();
            }

            return moves;
        }

        private static Rule CreateReplayRule(int ind)
        {
            string gmStr = listOfRules.Cells[ind, 2].Value.ToString();
            GameModes gameMode = StringConverter.ToGameMode(gmStr);

            string limitStr = listOfRules.Cells[ind, 3].Value.ToString();
            int limit = int.TryParse(limitStr, out limit) ? limit : 2;

            string timeStr = listOfRules.Cells[ind, 4].Value.ToString();
            int time = int.TryParse(timeStr, out time) ? time : 60;

            string rotatableStr = listOfRules.Cells[ind, 5].Value.ToString();
            bool rotatable = StringConverter.ToBoolean(rotatableStr);

            return new Rule(gameMode, limit, time, rotatable);
        }

        private static void PreparePackage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            package = new ExcelPackage(stream.BaseStream);

            listOfMoves = package.Workbook.Worksheets.First(s => s.Name == "List of moves");
            listOfReplays = package.Workbook.Worksheets.First(s => s.Name == "List of replays");
            listOfRules = package.Workbook.Worksheets.First(s => s.Name == "List of rules");

            if (listOfMoves == null || listOfReplays == null || listOfRules == null)
                throw new InvalidOperationException();
        }

        private static ReplayBase CreateReplayBase(int ind, string id)
        {
            string endingTimeStr = listOfReplays.Cells[ind, 3].Value.ToString();
            DateTime endingTime = Convert.ToDateTime(endingTimeStr);

            ReplayBase replayBase = new ReplayBase(id, endingTime)
            {
                Name = listOfReplays.Cells[ind, 2].Value.ToString()
            };

            SetStartPositionForReplay(ind, replayBase);

            return replayBase;
        }

        private static void SetStartPositionForReplay(int ind, ReplayBase replayBase)
        {
            string crRectStr = listOfReplays.Cells[ind, 4].Value.ToString();
            string clRectStr = listOfReplays.Cells[ind, 5].Value.ToString();
            string mapSizeStr = listOfReplays.Cells[ind, 6].Value.ToString();

            Rectangle crRect = StringConverter.ToRectangle(crRectStr);
            Rectangle clRect = StringConverter.ToRectangle(clRectStr);
            Size mapSize = StringConverter.ToSize(mapSizeStr);

            replayBase.SetStartPosition(mapSize, crRect, clRect);
        }
    }
}
