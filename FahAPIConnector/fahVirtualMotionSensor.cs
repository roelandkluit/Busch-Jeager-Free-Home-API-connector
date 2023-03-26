using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class fahVirtualMotionSensor : fahVirtualDeviceBase
    {
        public delegate void EventFahDataPoint(fahVirtualDeviceBase caller, fahApiConnector.DataPoint dataPoint);
        public event EventFahDataPoint OnFahDataPointEvent;
        bool isOutputActivated = false;

        public fahVirtualMotionSensor(fahApiConnector fahApiConnector, string DeviceSerialNumber) : base(fahApiConnector, DeviceSerialNumber, "MotionSensor")
        {
            OnDeviceRegisterdEvent += fahVirtualBinaryInput_OnDeviceRegisterdEvent;
        }

        private void fahVirtualBinaryInput_OnDeviceRegisterdEvent(string FahDeviceID, bool reRegister)
        {
            if(!reRegister)
                fahApi.RequestDataPoint(FahDeviceID, "ch0000", "odp0000");
        }

        public bool isMotionSensed
        {
            get
            {
                return isOutputActivated;
            }
        }

        public void SetMotion(bool isMotionDetected)
        {
            if (this.deviceFaHID != "")
            {
                isOutputActivated = isMotionDetected;
                fahApiConnector.DataPoint dataPoint = new fahApiConnector.DataPoint();
                dataPoint.channel = "ch0000";
                dataPoint.device = this.deviceFaHID;
                dataPoint.datapoint = "odp0000";
                dataPoint.value = isMotionDetected ? "1" : "0";
                fahApi.SetDataPoint(dataPoint);
            }
            else
            {
                throw new Exception("Device not online");
            }
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
                if (dataPoint.channel == "ch0000")
                {
                    Console.Write("Device: " + dataPoint.device + " chan: " + dataPoint.channel + " dp: " + dataPoint.datapoint + " val: " + dataPoint.value);
                    //ODP000 on Channel 0000 is motion sensor output
                    if (dataPoint.datapoint == "odp0000")
                    {
                        if (dataPoint.value == "1")
                            isOutputActivated = true;
                        else if (dataPoint.value == "0")
                            isOutputActivated = false;

                        Console.WriteLine(" - Done");
                    }
                    else
                    {
                        Console.WriteLine("");
                    }
                }
            }
        }
    }
}