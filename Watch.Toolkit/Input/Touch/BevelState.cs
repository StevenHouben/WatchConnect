using System.ComponentModel;

namespace Watch.Toolkit.Input.Touch
{
    public class BevelState
    {
        public bool BevelTop { get; set; }
        public bool BevelRight { get; set; }
        public bool BevelLeft { get; set; }
        public bool BevelBottom { get; set; }

        public void UpdateState(BevelSide side, bool state)
        {
            switch (side)
            {
                case BevelSide.TopSide:
                    BevelTop = state;
                    break;
                case BevelSide.LeftSide:
                    BevelLeft = state;
                    break;
                case BevelSide.RightSide:
                    BevelRight = state;
                    break;
                case BevelSide.BottomSide:
                    BevelBottom = state;
                    break;

            }
        }
    }
}
