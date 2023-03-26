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

    public class fahVirtualDeviceBase
    {
        private static byte FAH_DEVICE_REFRESH_COUNTER = 20;
        protected fahApiConnector fahApi;
        private string serial = "";
        private string deviceType = "";
        protected string deviceFaHID = "";
        private System.Timers.Timer TimerUpdateRegistration;
        private uint counter = 0;
        private bool isStarted = false;
        protected fahApiConnector.DeviceData deviceData;

        public delegate void EventOnDeviceRegisterd(string FahDeviceID, bool Refresh);
        public event EventOnDeviceRegisterd OnDeviceRegisterdEvent;

        public fahVirtualDeviceBase(fahApiConnector fahApiConnector, string DeviceSerialNumber, string DeviceType)
        {
            serial = DeviceSerialNumber;
            deviceType = DeviceType;
            fahApi = fahApiConnector;
            fahApi.OnOnlineStatusChangedEvent += FahApi_OnOnlineStatusChangedEvent;
            fahApi.onDeviceData += FahApi_onDeviceData;
            TimerUpdateRegistration = new System.Timers.Timer(10000);
            TimerUpdateRegistration.Elapsed += TimerUpdateRegistration_Elapsed;
            TimerUpdateRegistration.AutoReset = true;
        }

        private void FahApi_onDeviceData(fahApiConnector caller, fahApiConnector.DeviceData deviceData)
        {
            if (deviceData.device == deviceFaHID)
            {
                if(fahApi.logAllDataPointsToConsole) Console.WriteLine("GotDeviceData for this device;" + deviceData.DisplayName + "-->" + deviceData.functionID);
                this.deviceData = deviceData;
            }
        }

        private void TimerUpdateRegistration_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Console.WriteLine("TMR-" + deviceFaHID + "->" + counter);
            if (isStarted)
            {
                counter++;
                if (counter >= FAH_DEVICE_REFRESH_COUNTER)
                {
                    counter = 0;
                    registerDevice(true);
                }
            }
        }

        private void FahApi_OnOnlineStatusChangedEvent(fahApiConnector caller, bool isOnline)
        {
            if (isOnline && isStarted)
            {
                if(deviceFaHID == "")
                {
                    registerDevice(false);
                }
            }
        }

        private void registerDevice(bool bRefreshRegistration)
        {
            if (isStarted)
            {
                if (fahApi.RegisterDevice(serial, deviceType, out deviceFaHID))
                {
                    if(deviceFaHID !="" && deviceData == null)
                    {
                        fahApiConnector.DeviceData d = new fahApiConnector.DeviceData();
                        d.device = deviceFaHID;
                        fahApi.GetDeviceConfigFromAP(d);                        
                    }
                    if (!bRefreshRegistration)
                    {
                        fahApi.OnDataPointEvent += FahApi_OnDataPointEvent;
                        TimerUpdateRegistration.Enabled = true;
                        TimerUpdateRegistration.Start();
                    }
                    try
                    {
                        OnDeviceRegisterdEvent?.Invoke(deviceFaHID, bRefreshRegistration);
                    }
                    catch { }
                }
                else
                {
                    throw new Exception("Unable to register device");
                }
            }
        }

        public void StopDevice()
        {
            if (isStarted)
            {
                isStarted = false;
                fahApi.OnDataPointEvent -= FahApi_OnDataPointEvent;
                TimerUpdateRegistration.Stop();
                deviceFaHID = "";
            }
        }

        public bool isRunning
        {
            get
            {
                return isStarted;
            }
        }

        public void StartDevice()
        {
            isStarted = true;
            if (deviceFaHID != "")
            {
                throw new Exception("Device already started");
            }
            try
            {
                if(fahApi.isApiWSConnectionActive)
                {
                    registerDevice(false);
                }
                else
                {
                    //Console.WriteLine("Cannot register device at this time, AP not connected, try later");
                }
            }
            catch(Exception e)
            {
                throw new Exception("Unable to start device", e);
            }
        }

        protected virtual void FahApi_OnDataPointEvent(fahApiConnector caller, fahApiConnector.DataPoint dataPoint)
        {
            if (dataPoint.device == deviceFaHID)
            {
                if (fahApi.logAllDataPointsToConsole) Console.WriteLine(">> " + dataPoint.ToString());
            }
        }

        public string DeviceFaHID { get => deviceFaHID; private set => deviceFaHID = value; }
    }
}
