using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace KluitNET.FreeAtHome.LocalAPIConnector
{
    /*
     *  (C)2023 Roeland Kluit
     *  
     *  This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
     *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
     *  You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
     * 
     *  Communication between FreeAtHome and other demotica
     *  
     *  Requires Free@Home System Access Point version 2 or later
     *  - Firmware version 3.0 or later
     *  - Local API to be enabled
     * 
     */

    public class fahVirtualRainSensor : fahVirtualDeviceBase
    {
        public delegate void EventFahDataPoint(fahVirtualDeviceBase caller, fahApiConnector.DataPoint dataPoint);
        public event EventFahDataPoint OnFahDataPointEvent;
        public bool ActAsImpulsButton = true;
        private Timer CheckMaintananceTimer = new Timer();

        //Chan0
        //0x0042	FID_RAIN_SENSOR
        //  0 >> 0x0027	    AL_RAIN_ALARM	    Rain Alarm	State of the rain sensor (sent cyclically and on COV)
        //  1 >> 0x0004	    AL_SCENE_CONTROL	Scene Control	Recall or learn the set value related to encoded scene number
        //  2 >> 0x0405	    AL_RAIN_SENSOR_ACTIVATION_PERCENTAGE Rain detection
        //  3 >> 0x0406	    AL_RAIN_SENSOR_FREQUENCY	Rain sensor frequency

        public void SetRainInformation(double amount_of_rain)
        {
            fahApiConnector.DataPoint dataPoint = new fahApiConnector.DataPoint();
            dataPoint.channel = "ch0000";
            dataPoint.device = this.deviceFaHID;
            dataPoint.datapoint = "odp0000";
            dataPoint.SetValue(amount_of_rain > 0 ? 1: 0);
            fahApi.SetDataPoint(dataPoint);

            dataPoint.datapoint = "odp0002";
            dataPoint.SetValue(amount_of_rain);
            fahApi.SetDataPoint(dataPoint);
        }

        bool isOutputActivated = false;        

        public fahVirtualRainSensor(fahApiConnector fahApiConnector, string DeviceSerialNumber) : base(fahApiConnector, DeviceSerialNumber, "Weather-RainSensor")
        {
            OnDeviceRegisterdEvent += fahVirtualWeatherTemperatureSensor_OnDeviceRegisterdEvent;
            CheckMaintananceTimer.Elapsed += CheckMaintananceTimer_Elapsed; ;
            CheckMaintananceTimer.Enabled = false;
            CheckMaintananceTimer.Interval = 5000;
            CheckMaintananceTimer.AutoReset = false;
        }

        private void CheckMaintananceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckMaintananceTimer.Enabled = false;
            //SetOutputOnOff(false);
        }

        private void fahVirtualWeatherTemperatureSensor_OnDeviceRegisterdEvent(string FahDeviceID, bool reRegister)
        {
            if(!reRegister)
                fahApi.RequestDataPoint(FahDeviceID, "ch0000", "odp0000");
        }

        public bool isOutputActive()
        {
            return isOutputActivated;
        }
        
        protected override void FahApi_OnDataPointEvent(fahApiConnector caller, fahApiConnector.DataPoint dataPoint)
        {
            if (dataPoint.device == deviceFaHID)
            {
                try
                {
                    OnFahDataPointEvent?.Invoke(this, dataPoint);
                }
                catch { }
            }
        }
    }
}
