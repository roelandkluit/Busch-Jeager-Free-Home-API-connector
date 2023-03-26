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

    public class fahDeviceSwitchMonitor
    {
        protected fahApiConnector fahApi;
        private string deviceChannel = "";
        protected string FahDeviceID = "";
        protected string FahDataPoint = "";
        protected bool ChannelIsActive = false;
        protected bool dataPointRetrievedFromSysAP = false;
        protected string sDataPointValue = "";
        protected bool dataPointGetAfterUpdateCompleted = true;

        public delegate void EventOnDeviceStatusChanged(fahDeviceSwitchMonitor caller, bool isActivatedState);
        public event EventOnDeviceStatusChanged OnDeviceStatusChangedEvent;

        public string DeviceFaHID { get => FahDeviceID; private set => FahDeviceID = value; }
        public string DeviceChannel { get => deviceChannel; private set => deviceChannel = value; }

        public bool hasDeviceStatusBeenRetrieved
        {
            get
            {
                return dataPointRetrievedFromSysAP;
            }
        }

        public bool ActivatedState
        {
            get
            {
                if (dataPointRetrievedFromSysAP)
                {
                    if (!dataPointGetAfterUpdateCompleted)
                        dataPointGetAfterUpdateCompleted = true;
                    return ChannelIsActive;
                }
                else
                    throw new Exception("Datapoint value has not been retrieved at this moment");
            }
        }

        public bool dataPointValueHasChanged
        {
            get
            {
                return !dataPointGetAfterUpdateCompleted;
            }
        }

        public string dataPointValue
        {
            get
            {
                if (!dataPointGetAfterUpdateCompleted)
                    dataPointGetAfterUpdateCompleted = true;
                return sDataPointValue;
            }
        }

        public fahDeviceSwitchMonitor(fahApiConnector fahApiConnector, string DeviceFahID, string Channel, string DataPoint = "odp0000")
        {
            FahDeviceID = DeviceFahID;
            DeviceChannel = Channel;
            FahDataPoint = DataPoint;
            fahApi = fahApiConnector;
            fahApi.OnDataPointEvent += FahApi_OnDataPointEvent;
            fahApi.RequestDataPoint(FahDeviceID, Channel, FahDataPoint);
        }

        protected virtual void FahApi_OnDataPointEvent(fahApiConnector caller, fahApiConnector.DataPoint dataPoint)
        {
            if (dataPoint.device == FahDeviceID && dataPoint.channel == DeviceChannel && dataPoint.datapoint == FahDataPoint)
            {
                dataPointGetAfterUpdateCompleted = false;
                dataPointRetrievedFromSysAP = true;
                sDataPointValue = dataPoint.value;
                ChannelIsActive = dataPoint.value == "1";
                try
                {
                    OnDeviceStatusChangedEvent?.Invoke(this, ChannelIsActive);
                }
                catch { }
            }    
        }
    }
}
