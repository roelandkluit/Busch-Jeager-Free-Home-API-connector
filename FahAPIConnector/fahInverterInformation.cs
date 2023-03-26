using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KluitNET.FreeAtHome.LocalAPIConnector
{
    //Virtual device does not function correctly
    //implementation incomplete

    public class fahInverterInformation : fahVirtualDeviceBase
    {
        public delegate void EventFahDataPoint(fahVirtualDeviceBase caller, fahApiConnector.DataPoint dataPoint);
        //public event EventFahDataPoint OnFahDataPointEvent;

        public fahInverterInformation(fahApiConnector fahApiConnector, string DeviceSerialNumber) : base(fahApiConnector, DeviceSerialNumber, "EnergyInverterMeterBattery")
        {
            //throw new Exception("Not Implemented");
            OnDeviceRegisterdEvent += fahInverterInformation_OnDeviceRegisterdEvent;
        }

        private void fahInverterInformation_OnDeviceRegisterdEvent(string FahDeviceID, bool reRegister)
        {
            //fahApi.RequestDataPoint(FahDeviceID, "ch0000", "idp0000");
        }

/*
        0x04A6	AL_SELF_CONSUMPTION Self-consumption production PV/ Total consumption
        0x04A7	AL_SELF_SUFFICIENCY Self-sufficiency Consumption from PV/ Total consumption
        0x04A8	AL_HOME_POWER_CONSUMPTION Home power consumption  Power in home(PV and grid)
        0x04A9	AL_POWER_TO_GRID Power to grid   Power from and to the grid: Purchased(less than 0), Injection(more than 0)
*/

        public void SetCurrentProduction()
        {
            if (this.deviceFaHID != "")
            {
                //TODO
            }
            else
            {
                Console.WriteLine("Not Online");
            }
        }

        protected override void FahApi_OnDataPointEvent(fahApiConnector caller, fahApiConnector.DataPoint dataPoint)
        {
            if (dataPoint.device == deviceFaHID)
            {
            }
        }
    }
}
