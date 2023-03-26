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
    public class fahDeviceController:fahDeviceSwitchMonitor
    {
        private bool bDatapointDirty = false;
        private DateTime lastDirtyTime = DateTime.UtcNow;
        string FahSetDataPoint = "";

        public fahDeviceController(fahApiConnector fahApiConnector, string DeviceFahID, string Channel, string GetDataPoint = "odp0000", string SetDataPoint = "odp0000") : base(fahApiConnector, DeviceFahID, Channel, GetDataPoint)
        {
            this.FahSetDataPoint = SetDataPoint;
            fahApi.OnDataPointEvent += FahApi_OnControllerDataPointEvent;
        }

        new public string dataPointValue
        {
            get
            {
                if (!dataPointGetAfterUpdateCompleted)
                    dataPointGetAfterUpdateCompleted = true;
                return sDataPointValue;
            }
            set
            {
                bDatapointDirty = true;
                lastDirtyTime = DateTime.UtcNow;
                fahApi.SetDataPoint(DeviceFaHID, base.DeviceChannel, FahSetDataPoint, value);
            }
        }


        new public bool ActivatedState
        {
            get
            {
                return base.ActivatedState;
            }
            set
            {
                bDatapointDirty = true;
                lastDirtyTime = DateTime.UtcNow;
                fahApi.SetDataPoint(DeviceFaHID, base.DeviceChannel, FahSetDataPoint, (value ? "1" : "0"));
                //System.Diagnostics.Debug.WriteLine("Controller dirty Self: " + bDatapointDirty);
            }
        }

        public bool isDataPointDirty
        {
            get
            {
                int result = DateTime.Compare(lastDirtyTime.AddSeconds(3), DateTime.UtcNow);
                if ( result > 1)
                {
                    //System.Diagnostics.Debug.WriteLine("Dirty expired: " + lastDirtyTime + "-->" + result);
                    bDatapointDirty = false;
                }
                return bDatapointDirty;
            }
        }

        protected virtual void FahApi_OnControllerDataPointEvent(fahApiConnector caller, fahApiConnector.DataPoint dataPoint)
        {
            if (dataPoint.device == FahDeviceID && dataPoint.channel == DeviceChannel && dataPoint.datapoint == FahDataPoint)
            {
                //System.Diagnostics.Debug.WriteLine("Dirty reset by dataevent");
                bDatapointDirty = false;
            }
        }
    }
}
