using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WhyIDidntKnowThisGameEarlier.MapLogic;
using WhyIDidntKnowThisGameEarlier.SessionLogic;

namespace WhyIDidntKnowThisGameEarlier
{
    public partial class Form1 : Form
    {
        private readonly AIBot aiBot = new AIBot();
        private readonly List<Replay> replays;
        private readonly ReplayDrawer replayDrawer;
        private Replay showingReplay;
        private Session session;
        private readonly Color movingPlayerColor = Color.FromArgb(240, 230, 210);
        private readonly Color waitingPlayerColor = Color.FromArgb(240, 240, 240);
        private readonly AutoSession menuAutoSession;
        private Point startSelectingAreaPoint;
        private bool selectingArea;
        private Point endSelectingAreaPoint;
        private bool isSessionExist;

        private bool IsInMenu => Controls.Contains(MainMenuPanel);

        private bool IsOnSessionsSettingsScene => Controls.Contains(SettingsPanel);

        private bool IsOnReplaysListScene => Controls.Contains(ReplayListPanel);

        private bool IsOnSessionScene => Controls.Contains(SessionMainPanel);

        private bool IsOnReplayScene => Controls.Contains(ReplayPanel);

        private const string streakRuleDescription = "Счетчик пропущенных ходов сбрасывается полностью, " +
            "если сделан ход. Игра заканчивается при достижении счетчиком указанного значения.";

        private const string accumulateRuleDescription = "Счетчик пропущенных ходов уменьшается каждый раз, " +
            "когда сделан ход. Игра заканчивается при достижении счетчиком указанного значения.";

        public Form1()
        {
            InitializeComponent();
            Controls.Remove(HiddenTabControl);
            SwitchScene(MainMenuPanel);
            replays = ReplayFileReader.TryLoadReplays();

            menuAutoSession = new AutoSession(MenuSessionDrawerControl);
            replayDrawer = new ReplayDrawer();

            SessionDrawerControl.MouseWheel += new MouseEventHandler(DrawerMouseWhell);

            SessionStatesUpdater.Tick += (sender, e) => UpdateSessionStateControls();

            ReplayPreviewDrawerControl.Paint += (sender, e) => replayDrawer.Draw(e.Graphics);
            ReplayDrawerControl.Paint += (sender, e) => replayDrawer.Draw(e.Graphics);
            ReplaysGrid.Font = new Font("Comic Sans MS", 13);

            replayDrawer.ShowingMoveChanged += (sender, e) => ChangeCurrentMoveInList();

            StartNewAutoSession();
        }

        private void SwitchScene(Control targetScene)
        {
            Controls.Clear();
            Controls.Add(targetScene);
            targetScene.Dock = DockStyle.Fill;

            if (IsInMenu && menuAutoSession != null 
                && menuAutoSession.Session.IsEnding)
                StartNewAutoSession();

            Invalidate(true);
        }

        private void DrawSessionMap(object sender, PaintEventArgs e)
        {
            if (session != null)
                DrawMapForCurrentSession(e.Graphics);
        }

        private void DrawMapForCurrentSession(Graphics g)
        {
            session.Draw(g);

            if (session.Turn == Participants.Creator)
                session.DrawActiveRectangle(g);
        }

        private void DrawerMouseClick(object sender, MouseEventArgs e)
        {
            if (session == null || session == menuAutoSession.Session)
                return;

            if (session.Turn != Participants.Creator)
                return;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    session.TryMakeMove();
                    break;
                case MouseButtons.Right:
                    session.SkipMove();
                    break;
            }
        }

        private void DrawerMouseWhell(object sender, MouseEventArgs e)
        {
            if (session == null || session == menuAutoSession.Session)
                return;

            session.RotateActiveRectangle();
            SessionDrawerControl.Invalidate();
        }

        private void OpenSettingsForNewOfflineGame(object sender, MouseEventArgs e)
        {
            SwitchScene(SettingsPanel);

            UserNamePl2Lbl.Text = "Bot";
            UserNamePl1Lbl.Text = "Creator";

            if (RulesBox.SelectedIndex == -1) 
                RulesBox.SelectedIndex = 0;
        }

        private void InitializeOfflineSession()
        {
            session = new Session();
            InitializeMap();
            SetRules();

            session.TurnChanged += (sender, e) => UpdateSessionStateControls();
            session.Tick += (sender, e) => UpdateSessionStateControls();
            session.Ended += (sender, e) => replays.Add(session.CreateReplay());

            aiBot.Join(session);
            aiBot.Activate();

            session.Start();
            SessionStatesUpdater.Start();

            SetTitlesColors();
        }

        private void SetRules()
        {
            if (int.TryParse(SkipLimitBox.Text, out int limit))
                session.SkippedMovesLimit = limit;

            if (int.TryParse(MoveDurationBox.Text, out int duration))
                session.MoveDuration = duration;

            session.GameMode = (GameModes)RulesBox.SelectedIndex;
            session.IsPossibleToRotate = RuleForRotating.Checked;
        }

        private void InitializeMap()
        {
            GetMapSize(out int width, out int height);
            session.InitialzeMap(new Size(width, height));
            session.MapDrawer.FitIn(SessionDrawerControl.ClientSize);
            session.MapDrawer.CenterIn(SessionDrawerControl.ClientSize);
        }

        private void GetMapSize(out int width, out int height)
        {
            if (!int.TryParse(WidthBox.Text, out width))
                width = Map.DefaultSize;

            if (!int.TryParse(HeightBox.Text, out height))
                height = Map.DefaultSize;
        }

        private void UpdateActiveRectangle(object sender, MouseEventArgs e)
        {
            if (session == menuAutoSession.Session)
                return;

            session.ToMousePosition(e.Location);
            SessionDrawerControl.Invalidate();
        }

        private void UpdateSessionStateControls()
        {
            if (session == null)
                return;

            Invoke((MethodInvoker)delegate
            {
                UpdateUsersPanels();
                SkippedMovesCounter.Text = session.AllSkippedMoves.ToString();
                RuleCounter.Text = $"{session.SkippedMovesCounter}/{session.SkippedMovesLimit}";

                SessionDrawerControl.Invalidate();
            });
        }

        private void UpdateUsersPanels()
        {
            UpdatePoints();
            int timeLeft = session.MoveDuration - session.MoveTimeCounter;

            switch (session.Turn)
            {
                case Participants.Creator:
                    TimeLeftToMovePl1Lbl.Text = timeLeft.ToString();
                    PanelPl1.BackColor = movingPlayerColor;
                    PanelPl2.BackColor = waitingPlayerColor;
                    break;
                case Participants.Client:
                    TimeLeftToMovePl2Lbl.Text = timeLeft.ToString();
                    PanelPl2.BackColor = movingPlayerColor;
                    PanelPl1.BackColor = waitingPlayerColor;
                    break;
            }
        }

        private void SetTitlesColors()
        {
            UserNamePl1Lbl.BackColor = session.MapDrawer.Player1Color;
            UserNamePl2Lbl.BackColor = session.MapDrawer.Player2Color;
        }

        private void UpdatePoints()
        {
            PointsPl1Lbl.Text = session.MapInterface.SpacePlayer1.ToString();
            PointsPl2Lbl.Text = session.MapInterface.SpacePlayer2.ToString();
        }

        private void Play(object sender, EventArgs e)
        {
            if (session != menuAutoSession.Session && session != null)
                return;

            isSessionExist = true;
            SwitchScene(SessionMainPanel);
            InitializeOfflineSession();
            SessionDrawerControl.Invalidate();
        }

        private void ExitToMenu(object sender, MouseEventArgs e)
        {
            SwitchScene(MainMenuPanel);
            replayDrawer.DeselectAll();

            if (session != menuAutoSession?.Session && session != null)
                EndSession();

            else
            {
                SessionStatesUpdater.Stop();
                menuAutoSession?.SetNewSurface(MenuSessionDrawerControl);
                MenuSessionDrawerControl.Invalidate();
                session = null;
            }
        }

        private void EndSession()
        {
            SessionStatesUpdater.Stop();
            aiBot.Disable();
            session?.End();
            session = null;
            isSessionExist = false;
        }

        private void PreviewDrawerPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            GetMapSize(out int width, out int height);
            MapDrawer map = new MapDrawer(new Map(width, height));
            map.FitIn(PreviewDrawerControl.Size);
            map.CenterIn(PreviewDrawerControl.Size);

            map.Draw(e.Graphics);
        }

        private void UpdatePreviewDrawer(object sender, EventArgs e)
        {
            PreviewDrawerControl.Invalidate();
        }

        private void ShowDescription(object sender, EventArgs e)
        {
            switch (RulesBox.SelectedIndex)
            {
                case 0:
                    RuleDescription.Text = streakRuleDescription;
                    break;
                case 1:
                    RuleDescription.Text = accumulateRuleDescription;
                    break;
            }
        }

        private void SetBotMaxMoveDuration(object sender, EventArgs e)
        {
            if (int.TryParse(BotMoveMaxDurationBox.Text, out int value))
                aiBot.MaxTimeToMove = value;
        }

        private void SetBotMinMoveDuration(object sender, EventArgs e)
        {
            if (int.TryParse(BotMoveMinDurationBox.Text, out int value))
                aiBot.MinTimeToMove = value;
        }

        private void StartNewAutoSession()
        {
            menuAutoSession.CreateNewSession();

            if (!isSessionExist)
                session = menuAutoSession.Session;

            menuAutoSession.Session.Ended += new EventHandler(CreateNewAutoSession);
        }

        private void CreateNewAutoSession(object sender, EventArgs e)
        {
            Thread.Sleep(1000);
            StartNewAutoSession();
        }

        private void SeeBotsSession(object sender, EventArgs e)
        {
            if (menuAutoSession == null)
                return;

            SwitchScene(SessionMainPanel);
            menuAutoSession.SetNewSurface(SessionDrawerControl);
            session = menuAutoSession.Session;
            
            SessionStatesUpdater.Start();

            UserNamePl2Lbl.Text = "Bot";
            UserNamePl1Lbl.Text = "Bot";
        }

        private void GoToReplaysList(object sender, MouseEventArgs e)
        {
            NoReplaysMessageLbl.Visible = false;
            ReplaysGrid.Rows.Clear();

            for (int repInd = replays.Count - 1; repInd >= 0; repInd--)
            {
                string name = replays[repInd].Name;
                string id = replays[repInd].ID;
                DateTime endingTime = replays[repInd].EndingTime;
                ReplaysGrid.Rows.Add(name, id, endingTime);
            }

            if (ReplaysGrid.Rows.Count == 0)
                NoReplaysMessageLbl.Visible = true;

            ReplaysGrid.ClearSelection();
            for (int rowNum = 0; rowNum < ReplaysGrid.Rows.Count; rowNum++)
            {
                ReplaysGrid.Rows[rowNum].Selected = false;
            }

            replayDrawer.FitIn(ReplayPreviewDrawerControl.Size);
            replayDrawer.CenterIn(ReplayPreviewDrawerControl.Size);
            SwitchScene(ReplayListPanel);
        }

        private void WhenReplaySelected(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (ReplaysGrid.SelectedCells.Count == 0)
                return;

            string id = ReplaysGrid.SelectedRows[0].Cells[1].Value.ToString();
            Replay newReplay = replays.Find(r => r.ID == id);
            if (newReplay == null)
                return;

            showingReplay = newReplay;
            UpdateMovesList();
            replayDrawer.ChangeShowingReplay(newReplay);
            replayDrawer.ToEnding();
            replayDrawer.FitIn(ReplayPreviewDrawerControl.Size);
            replayDrawer.CenterIn(ReplayPreviewDrawerControl.Size);
            ReplayPreviewDrawerControl.Invalidate();
        }

        private void UpdateMovesList()
        {
            ReplayMovesList.Rows.Clear();
            var movesList = showingReplay.GetMovesList();
            for (int moveInd = 1; moveInd < movesList.Count; moveInd++)
            {
                ReplayMovesList.Rows.Add(movesList.ElementAt(moveInd).ToString());
            }
        }

        private void OpenReplay(object sender, DataGridViewCellEventArgs e)
        {
            if (ReplaysGrid.SelectedRows.Count == 0)
                return;

            SwitchScene(ReplayPanel);
            replayDrawer.ToBeginning();
            replayDrawer.FitIn(ReplayDrawerControl.Size);
            replayDrawer.CenterIn(ReplayDrawerControl.Size);

            UpdateReplayStateControls();

            string rule;
            if (replayDrawer.Replay.Rule.GameMode == GameModes.StreakRule)
                rule = "Серия";
            else
                rule = "Баланс";
            ReplayRuleLbl.Text = "Правило: " + rule;
            ReplayIDLbl.Text = showingReplay.ID;
        }

        private void UpdateReplaysNames(object sender, DataGridViewCellEventArgs e)
        {
            if (ReplaysGrid.SelectedRows.Count == 0)
                return;

            DataGridViewRow row = ReplaysGrid.SelectedRows[0];
            string id = row.Cells[1].Value.ToString();
            Replay replay = replays.Find(r => r.ID == id);
            if (replay == null) 
                return;

            replay.Name = row.Cells[0].Value.ToString();
        }

        private void ShowNextMove(object sender, MouseEventArgs e)
        {
            replayDrawer.Next();
            ReplayDrawerControl.Invalidate();
            UpdateReplayStateControls();
        }

        private void ShowPreviousMove(object sender, MouseEventArgs e)
        {
            replayDrawer.Previous();
            ReplayDrawerControl.Invalidate();
            UpdateReplayStateControls();
        }

        private void UpdateReplayStateControls()
        {
            if (showingReplay == null)
                return;

            Move move = showingReplay.GetMove();

            PanelPL1Replay.BackColor = waitingPlayerColor;
            PanelPL2Replay.BackColor = waitingPlayerColor;
            UpdateStatesForMovedUser(move, showingReplay.Rule.MoveDuration);

            RuleCounterReplay.Text = $"{move.SkippedMovesCounter} / {showingReplay.Rule.SkippedMovesLimit}";
            SkippedMovesCounterReplay.Text = showingReplay.SkippedMoves.ToString();
            UpdateReplayProgressState();

            float pointsSum = move.ClientPoints + move.CreatorPoints;
            float creatorPart = 50;
            if (move.CreatorPoints > 0)
                creatorPart = move.CreatorPoints / pointsSum * 100;
            RatioBarReplay.FirstPart = (float)Math.Round(creatorPart, 1);
            RatioBarReplay.Invalidate();
        }

        private void UpdateReplayProgressState()
        {
            int now = showingReplay.CurrentHalfMove;
            float finish = showingReplay.Lenght - 1;
            double progress = Math.Round(now / finish, 3) * 100;
            if (progress.Equals(double.NaN))
                progress = 0;
            ReplayProgress.Text = $"{progress} %";
        }

        private void UpdateStatesForMovedUser(Move move, int maxMoveDuration)
        {
            switch (move.MovedPlayer)
            {
                case Participants.Creator:
                    TimeLeftToMovePl1ReplayLbl.Text = (maxMoveDuration - move.TimeSpent).ToString();
                    PanelPL1Replay.BackColor = movingPlayerColor;
                    break;
                case Participants.Client:
                    TimeLeftToMovePl2ReplayLbl.Text = (maxMoveDuration - move.TimeSpent).ToString();
                    PanelPL2Replay.BackColor = movingPlayerColor;
                    break;
                default:
                    PointsPl1ReplayLbl.Text = showingReplay.StartCreatorPoints.ToString();
                    PointsPl2ReplayLbl.Text = showingReplay.StartClientPoints.ToString();
                    TimeLeftToMovePl1ReplayLbl.Text = maxMoveDuration.ToString();
                    TimeLeftToMovePl2ReplayLbl.Text = maxMoveDuration.ToString();
                    return;
            }

            PointsPl1ReplayLbl.Text = move.CreatorPoints.ToString();
            PointsPl2ReplayLbl.Text = move.ClientPoints.ToString();
        }

        private void GoToEndingReplay(object sender, MouseEventArgs e)
        {
            replayDrawer.ToEnding();
            ReplayDrawerControl.Invalidate();
            UpdateReplayStateControls();
        }

        private void GoToBeginningReplay(object sender, MouseEventArgs e)
        {
            replayDrawer.ToBeginning();
            ReplayDrawerControl.Invalidate();
            UpdateReplayStateControls();
        }

        private void StartSelectingArea(object sender, MouseEventArgs e)
        {
            if (selectingArea)
                return;

            startSelectingAreaPoint = replayDrawer.GetCell(e.Location);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    replayDrawer.Select(startSelectingAreaPoint);
                    break;
                case MouseButtons.Right:
                    if (!replayDrawer.IsOnMap(startSelectingAreaPoint))
                        replayDrawer.DeselectAll();
                    else 
                        replayDrawer.Deselect(startSelectingAreaPoint);
                    break;
            }
            selectingArea = true;

            ReplayDrawerControl.Invalidate();
        }

        private void EndSelectingArea(object sender, MouseEventArgs e)
        {
            selectingArea = false;
            startSelectingAreaPoint = new Point(-1, -1);
            endSelectingAreaPoint = new Point(-1, -1);
        }

        private void UpdateSelectingArea(object sender, MouseEventArgs e)
        {
            if (!selectingArea)
                return;

            if (endSelectingAreaPoint != new Point(-1, -1))
                replayDrawer.DeselectArea(startSelectingAreaPoint, endSelectingAreaPoint);

            endSelectingAreaPoint = replayDrawer.GetCell(e.Location);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    replayDrawer.SelectArea(startSelectingAreaPoint, endSelectingAreaPoint);
                    break;
                case MouseButtons.Right:
                    replayDrawer.DeselectArea(startSelectingAreaPoint, endSelectingAreaPoint);
                    break;
            }

            ReplayDrawerControl.Invalidate();
        }

        private void ChangeCurrentMoveInList()
        {
            if (ReplayMovesList.Rows.Count == 0)
                return;

            ReplayMovesList.ClearSelection();

            Move currMove = showingReplay.GetMove();
            if (currMove.Index < 1)
                return;

            ReplayMovesList.Rows[currMove.Index - 1].Selected = true;
        }

        private void WhenMoveSelected(object sender, DataGridViewCellEventArgs e)
        {
            if (ReplayMovesList.SelectedCells.Count == 0)
                return;

            replayDrawer.ShowMove(ReplayMovesList.SelectedCells[0].RowIndex + 1);
            ReplayDrawerControl.Invalidate();
            UpdateReplayProgressState();
            UpdateReplayStateControls();
        }

        private void WhenFormSizeChanged(object sender, EventArgs e)
        {
            if (IsInMenu)
            {
                menuAutoSession.Session.MapDrawer.FitIn(MenuSessionDrawerControl.Size);
                menuAutoSession.Session.MapDrawer.CenterIn(MenuSessionDrawerControl.Size);
                return;
            }

            if (IsOnSessionScene)
            {
                session.MapDrawer.FitIn(SessionDrawerControl.Size);
                session.MapDrawer.CenterIn(SessionDrawerControl.Size);
                return;
            }

            if (IsOnSessionsSettingsScene)
            {
                SessionDrawerControl.Invalidate();
                return;
            }

            if (IsOnReplaysListScene)
            {
                replayDrawer.FitIn(ReplayPreviewDrawerControl.Size);
                replayDrawer.CenterIn(ReplayPreviewDrawerControl.Size);
            }

            else
            {
                replayDrawer.FitIn(ReplayDrawerControl.Size);
                replayDrawer.CenterIn(ReplayDrawerControl.Size);
            }
        }

        private void GoToUpdatesInfo(object sender, MouseEventArgs e)
        {
            SwitchScene(UpdatesInfoMainPanel);
        }

        private void ExportReplays(object sender, FormClosingEventArgs e)
        {
            ReplaysFileWriter.ExportReplays(replays);
        }

        private void DeleteReplay(object sender, DataGridViewRowCancelEventArgs e)
        {
            string id = (string)e.Row.Cells[1].Value;
            Replay deletedR = replays.Find(r => r.ID == id);
            if (deletedR != null) 
            {
                replays.Remove(deletedR);
            }
        }

        private void KeysPressed(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Left:
                    if (IsOnReplayScene)
                        replayDrawer.Next();
                    break;
                case Keys.Right:
                    if (IsOnReplayScene)
                        replayDrawer.Previous();
                    break;
                case Keys.Escape:
                    SwitchScene(MainMenuPanel);
                    return;
            }

            ReplayDrawerControl.Invalidate();
            UpdateReplayStateControls();
        }
    }
}
