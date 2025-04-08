using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    public static class ReplaysFileWriter
    {
        static ExcelPackage package;
        private static ExcelWorksheet sheetMovesList;
        private static ExcelWorksheet sheetReplaysList;
        private static ExcelWorksheet sheetRulesList;

        /// <summary>
        /// Открывает поток и выгружает список реплеев в таблицу Excel
        /// </summary>
        /// <param name="replays"> Список выгружаемых реплеев </param>
        /// <returns> True, если операция прошла успешно. Иначе false. </returns>
        public static bool ExportReplays(List<Replay> replays)
        {
            if (replays == null)
                return false;

            InitExcelPackage();
            return TryExport(replays);
        }

        private static void InitExcelPackage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            package = new ExcelPackage();

            sheetMovesList = package.Workbook.Worksheets.Add("List of moves");
            sheetReplaysList = package.Workbook.Worksheets.Add("List of replays");
            sheetRulesList = package.Workbook.Worksheets.Add("List of rules");

            sheetMovesList.Protection.IsProtected = true;
            sheetReplaysList.Protection.IsProtected = true;
            sheetRulesList.Protection.IsProtected = true;
        }

        private static bool TryExport(List<Replay> replays)
        {
            try
            {
                foreach (Replay replay in replays)
                {
                    int ind = replays.IndexOf(replay);
                    Export(replay, ind + 1);
                }

                File.WriteAllBytes("replays.xlsx", package.GetAsByteArray());
                return true;
            }

            catch { return false; }
            finally { package?.Dispose(); }
        }

        private static void Export(Replay replay, int ind)
        {
            if (replay == null)
                return;

            WriteToMainList(replay, ind);
            WriteMoves(replay, ind);
            WriteRules(replay, ind);
        }

        private static void WriteRules(Replay replay, int ind)
        {
            sheetRulesList.Cells[ind, 1].Value = replay.ID;
            Rule rule = replay.Rule;
            sheetRulesList.Cells[ind, 2].Value = (int)rule.GameMode;
            sheetRulesList.Cells[ind, 3].Value = rule.SkippedMovesLimit;
            sheetRulesList.Cells[ind, 4].Value = rule.MoveDuration;
            sheetRulesList.Cells[ind, 5].Value = rule.IsPossibleToRotate;
        }

        private static void WriteMoves(Replay replay, int ind)
        {
            sheetMovesList.Cells[ind, 1].Value = replay.ID;
            var moves = replay.GetMovesList();
            foreach (Move move in moves)
            {
                sheetMovesList.Cells[ind, move.Index + 2].Value = move.ToFullString();
            }
        }

        private static void WriteToMainList(Replay replay, int ind)
        {
            sheetReplaysList.Cells[ind, 1].Value = replay.ID;
            sheetReplaysList.Cells[ind, 2].Value = replay.Name;
            sheetReplaysList.Cells[ind, 3].Value = replay.EndingTime.ToString();
            sheetReplaysList.Cells[ind, 4].Value = replay.CreatorStartRectangle;
            sheetReplaysList.Cells[ind, 5].Value = replay.ClientStartRectangle;
            sheetReplaysList.Cells[ind, 6].Value = replay.MapSize;
        }
    }
}
