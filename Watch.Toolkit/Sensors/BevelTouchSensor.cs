using Watch.Toolkit.Input.Touch;

namespace Watch.Toolkit.Sensors
{
    public class BevelTouchSensor:TouchSensor
    {
        private BevelState _touchStates;

        public BevelTouchSensor()
        {
            _touchStates= new BevelState();
        }
        public BevelState TouchStates
        {
            get { return _touchStates; }
            set
            {
                _touchStates = value;
                Value = -1; 
            }
        }

        protected override void ProcessSensorData()
        {
            
        }
    }
}
