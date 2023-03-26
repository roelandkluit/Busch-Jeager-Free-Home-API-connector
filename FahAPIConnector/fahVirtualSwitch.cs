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

    public class fahVirtualSwitch : fahVirtualDeviceBase
    {
        public delegate void EventOnLightOnOff(fahVirtualDeviceBase caller, bool lightIsOn);
        public event EventOnLightOnOff OnLightOnOffEvent;
        public delegate void EventFahDataPoint(fahVirtualDeviceBase caller, fahApiConnector.DataPoint dataPoint);
        public event EventFahDataPoint OnFahDataPointEvent;
        public bool consoleOut = false;

        bool lightIsOn = false;

        public fahVirtualSwitch(fahApiConnector fahApiConnector, string DeviceSerialNumber) : base(fahApiConnector, DeviceSerialNumber, "SwitchingActuator")
        {
            OnDeviceRegisterdEvent += FahVirtualSwitch_OnDeviceRegisterdEvent;
        }

        private void FahVirtualSwitch_OnDeviceRegisterdEvent(string FahDeviceID, bool reRegister)
        {
            if(!reRegister)
                fahApi.RequestDataPoint(FahDeviceID, "ch0000", "idp0000");
        }

        public bool isLightOn()
        {
            return lightIsOn;
        }

        public bool lightOn
        {
            get
            {
                return lightIsOn;
            }
            set
            {
                SetLightOnOff(value);
            }
        }


        public void SetLightOnOff(bool isOn)
        {
            if (this.deviceFaHID != "")
            {
                fahApiConnector.DataPoint dataPoint = new fahApiConnector.DataPoint();
                dataPoint.channel = "ch0000";
                dataPoint.device = this.deviceFaHID;
                dataPoint.datapoint = "idp0000";
                dataPoint.value = isOn ? "1" : "0";
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
                    if(consoleOut) Console.Write("Device: " + dataPoint.device + " chan: " + dataPoint.channel + " dp: " + dataPoint.datapoint + " val: " + dataPoint.value);
                    //IDP000 on Channel 0000 is switch input 
                    if (dataPoint.datapoint == "idp0000")
                    {
                        if (consoleOut) Console.WriteLine(" ON/OFF - Processed");
                        //Confirm switch output on ODP0000
                        dataPoint.datapoint = "odp0000";
                        if (fahApi.SetDataPoint(dataPoint))
                        {
                            lightIsOn = (dataPoint.value == "1");
                            try
                            {
                                OnLightOnOffEvent?.Invoke(this, lightIsOn);
                            }
                            catch { }
                        }
                    }
                    else if (dataPoint.datapoint == "idp0001")
                    {
                        if (deviceData != null && deviceData.functionID == fahApiConnector.DeviceFunctionIDs.FID_TRIGGER)
                        {
                            if (dataPoint.value == "1")
                            {
                                if (consoleOut) Console.WriteLine(" Trigger - Processed");
                                try
                                {
                                    OnLightOnOffEvent?.Invoke(this, true);
                                }
                                catch { }
                            }
                        }
                    }
                    else if (dataPoint.datapoint == "odp0000")
                    {
                        if (dataPoint.dataPointType == fahApiConnector.DataPointType.SceneTriggerDataPoint)
                        {
                            if (deviceData != null && deviceData.functionID == fahApiConnector.DeviceFunctionIDs.FID_TRIGGER)
                            {
                                if (consoleOut) Console.WriteLine(" SceneControlTrigger - Processed");
                                try
                                {
                                    OnLightOnOffEvent?.Invoke(this, true);
                                }
                                catch { }
                            }
                            else
                            {
                                //Ensure to activate input!, otherwise on next refresh device will be reset to idp value
                                dataPoint.datapoint = "idp0000";
                                if (fahApi.SetDataPoint(dataPoint))
                                {
                                    if (consoleOut) Console.WriteLine(" SceneControl - Processed");
                                    //lightIsOn = (dataPoint.value == "1");
                                    //OnLightOnOffEvent?.Invoke(this, lightIsOn);
                                }
                            }
                        }
                        else
                        {
                            if (consoleOut) Console.WriteLine(" - Needed?");
                        }
                    }
                    else
                    {
                        if (consoleOut) Console.WriteLine("");
                    }
                }
            }
        }
    }
}
