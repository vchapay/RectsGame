using System.Windows.Forms;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    public class Rule
    {
        public Rule(Session session) 
        {
            MoveDuration = session.MoveDuration;
            GameMode = session.GameMode;
            IsPossibleToRotate = session.IsPossibleToRotate;
            SkippedMovesLimit = session.SkippedMovesLimit;
        }

        public Rule(GameModes gameMode, int skippedMovesLimit, int moveDuration, bool isPossibleToRotate)
        {
            GameMode = gameMode;
            SkippedMovesLimit = skippedMovesLimit;
            MoveDuration = moveDuration;
            IsPossibleToRotate = isPossibleToRotate;
        }

        public int MoveDuration { get; }

        public GameModes GameMode { get; }

        public bool IsPossibleToRotate { get; }

        public int SkippedMovesLimit { get; }
    }
}