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
    public class fahDeviceRTCMonitor
    {
        private const string CH0 = "ch0000";
        private class IDPODP
        {
            public const string IDP_AL_FAN_STAGE_REQUEST = "idp0010";
            public const string IDP_AL_ECO_ON_OFF = "idp0011";
            public const string IDP_AL_CONTROLLER_ON_OFF_REQUEST = "idp0012";
            public const string IDP_AL_INFO_ABSOLUTE_SET_POINT_REQUEST = "idp0016";
            public const string ODP_AL_SET_POINT_TEMPERATURE = "odp0006";
            public const string ODP_AL_CONTROLLER_ON_OFF = "odp0008";
            public const string ODP_AL_FAN_MANUAL_ON_OFF = "odp000b";
            public const string ODP_AL_MEASURED_TEMPERATURE = "odp0010";
        }

        protected fahApiConnector fahApi;
        protected string FahDeviceID = "";
        private double _SetPoint;
        private double _MessuredTemperature;
        private bool _isOn;
        private bool _isEco;

        public double SetPoint
        {
            get
            {
                return _SetPoint;
            }
            private set
            {
                _SetPoint = value;
            }
        }

        public bool isOn
        {
            get
            {
                if(_isEco)
                {
                    return false;
                }
                return _isOn;
            }
            private set
            {
                _isOn = value;
            }
        }

        public bool isEco
        {
            get
            {
                return _isEco;
            }
            private set
            {
                _isEco = value;
            }
        }


        public double MessuredTemperature
        {
            get
            {
                return _MessuredTemperature;
            }
            private set
            {
                _MessuredTemperature = value;
            }
        }

        public delegate void EventOnDeviceStatusChanged(fahDeviceRTCMonitor caller, bool isActivatedState, bool isEco);
        public delegate void EventOnDeviceTermperatureChanged(fahDeviceRTCMonitor caller, double temperature);
        public event EventOnDeviceStatusChanged OnDeviceStatusChanged;
        public event EventOnDeviceTermperatureChanged OnDeviceSetPointChanged;
        public event EventOnDeviceTermperatureChanged OnDeviceMessuredTemperatureChanged;

        public string DeviceFaHID { get => FahDeviceID; private set => FahDeviceID = value; }

        public fahDeviceRTCMonitor(fahApiConnector fahApiConnector, string DeviceFahID)
        {
            FahDeviceID = DeviceFahID;
            fahApi = fahApiConnector;
            fahApi.OnDataPointEvent += FahApi_OnDataPointEvent;
            fahApi.RequestDataPoint(FahDeviceID, CH0, IDPODP.IDP_AL_ECO_ON_OFF);
            fahApi.RequestDataPoint(FahDeviceID, CH0, IDPODP.IDP_AL_INFO_ABSOLUTE_SET_POINT_REQUEST);
            fahApi.RequestDataPoint(FahDeviceID, CH0, IDPODP.IDP_AL_CONTROLLER_ON_OFF_REQUEST);
            fahApi.RequestDataPoint(FahDeviceID, CH0, IDPODP.ODP_AL_MEASURED_TEMPERATURE);
        }

        protected virtual void FahApi_OnDataPointEvent(fahApiConnector caller, fahApiConnector.DataPoint dataPoint)
        {
            if(dataPoint.channel == CH0 && dataPoint.device == DeviceFaHID)
            {
                switch(dataPoint.datapoint)
                {
                    case IDPODP.IDP_AL_INFO_ABSOLUTE_SET_POINT_REQUEST:
                        SetPoint = double.Parse(dataPoint.value, System.Globalization.CultureInfo.InvariantCulture);
                        OnDeviceSetPointChanged?.Invoke(this, SetPoint);
                        break;

                    case IDPODP.IDP_AL_CONTROLLER_ON_OFF_REQUEST:
                        isOn = dataPoint.value == "1";
                        OnDeviceStatusChanged?.Invoke(this, isOn, isEco);
                        break;

                    case IDPODP.IDP_AL_ECO_ON_OFF:
                        _isEco = dataPoint.value == "1";
                        OnDeviceStatusChanged?.Invoke(this, isOn, isEco);
                        break;

                    case IDPODP.ODP_AL_MEASURED_TEMPERATURE:
                        MessuredTemperature = double.Parse(dataPoint.value, System.Globalization.CultureInfo.InvariantCulture);
                        OnDeviceMessuredTemperatureChanged?.Invoke(this, MessuredTemperature);
                        break;
                }                
            }
        }
    }
}
