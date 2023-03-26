using System;
using System.Collections.Generic;
using System.Globalization;
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

    public class fahVirtualRTC : fahVirtualDeviceBase
    {
        private System.Timers.Timer tmrUpdateTemperature = null;

        public const string CH0 = "ch0000";
        public class IDPODP
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

        public enum TempertureController_STATE_INDICATION : byte
        {
            STATE_IDLE_ONorOFF = 65,
            STATE_ECO = 68,
            STATE_HEATING = 33,
            STATE_COOLING = 1
        }

        private bool _IsInital = true;
        private bool _IsHeating = false;
        private bool _IsCooling = false;
        private bool _IsEcoMode = false;
        private bool _IsOn = false;

        public delegate void EventSetTemperatureChanged(fahVirtualRTC caller, double setTemperature);
        public event EventSetTemperatureChanged OnEventSetTemperatureChanged;

        public delegate void EventSetDeviceModeEvent(fahVirtualRTC caller);
        public event EventSetDeviceModeEvent OnEventDeviceOn;
        public event EventSetDeviceModeEvent OnEventDeviceEco;
        public event EventSetDeviceModeEvent OnEventDeviceOff;

        private double _SetPointTemperature = 0;
        private double _newDelayedSetPointTemperature = 0;
        private double _MessuredTemperature = 0;

        public bool IsHeating
        {
            get => _IsHeating;
            set
            {
                if (_IsHeating != value)
                {
                    _IsHeating = value;
                    if (_IsCooling)
                        _IsCooling = false;
                    UpdateDisplayIndication();
                }
            }
        }

        public bool IsEcoMode
        {
            get => _IsEcoMode;
            set
            {
                if (_IsEcoMode != value)
                {
                    _IsEcoMode = value;
                    fahApi.SetDataPoint(this.DeviceFaHID, CH0, IDPODP.IDP_AL_ECO_ON_OFF, value ? "1" : "0");
                }
            }
        }

        public bool IsOn
        {
            get => _IsOn;
            set
            {
                if (_IsOn != value)
                {
                    fahApi.SetDataPoint(this.DeviceFaHID, CH0, IDPODP.IDP_AL_CONTROLLER_ON_OFF_REQUEST, value ? "1" : "0");
                }
            }
        }

        public bool IsCooling
        {
            get => IsCooling;
            set
            {
                if (_IsCooling != value)
                {
                    _IsCooling = value;
                    if (_IsHeating)
                        _IsHeating = false;
                    UpdateDisplayIndication();
                }
            }
        }

        public double MessuredTemperature
        {
            get => _MessuredTemperature;
            set
            {
                if (_MessuredTemperature != value)
                {
                    _MessuredTemperature = value;
                    if (_MessuredTemperature >= 0)
                        fahApi.SetDataPoint(this.DeviceFaHID, CH0, IDPODP.ODP_AL_MEASURED_TEMPERATURE, _MessuredTemperature.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }


        public fahVirtualRTC(fahApiConnector fahApiConnector, string DeviceSerialNumber) : base(fahApiConnector, DeviceSerialNumber, "RTC")
        {
            OnDeviceRegisterdEvent += fahVirtualRoomTempController_OnDeviceRegisterdEvent;
        }

        private void fahVirtualRoomTempController_OnDeviceRegisterdEvent(string FahDeviceID, bool reRegister)
        {
            if (!reRegister)
            {
                fahApi.RequestDataPoint(FahDeviceID, CH0, IDPODP.IDP_AL_INFO_ABSOLUTE_SET_POINT_REQUEST);
                fahApi.RequestDataPoint(FahDeviceID, CH0, IDPODP.IDP_AL_ECO_ON_OFF);
                fahApi.RequestDataPoint(FahDeviceID, CH0, IDPODP.IDP_AL_CONTROLLER_ON_OFF_REQUEST);
                //fahApi.RequestDataPoint(FahDeviceID, CH0, IDPODP.IDP_AL_FAN_STAGE_REQUEST);
            }
        }

        protected override void FahApi_OnDataPointEvent(fahApiConnector caller, fahApiConnector.DataPoint dataPoint)
        {
            if (dataPoint.device == deviceFaHID && dataPoint.channel == CH0)
            {
                //Console.WriteLine(DateTime.Now.ToString("s") + " Device: " + dataPoint.device + " chan: " + dataPoint.channel + " dp: " + dataPoint.datapoint + " val: " + dataPoint.value);
                OnDataPointEvent(caller, dataPoint);                    
            }
        }

        private void UpdateDisplayIndication()
        {
            if (_IsEcoMode && _IsOn)
            {
                SetThermostatDisplayStatus(TempertureController_STATE_INDICATION.STATE_ECO);
            }
            else if (_IsHeating)
            {
                SetThermostatDisplayStatus(TempertureController_STATE_INDICATION.STATE_HEATING);
            }
            else if (_IsCooling)
            {
                SetThermostatDisplayStatus(TempertureController_STATE_INDICATION.STATE_COOLING);
            }
            else
            {
                SetThermostatDisplayStatus(TempertureController_STATE_INDICATION.STATE_IDLE_ONorOFF);
            }
        }

        private void SetThermostatDisplayStatus(TempertureController_STATE_INDICATION _INDICATION)
        {
            if (_IsInital)
                return;
            Console.WriteLine("SI changed: " + _INDICATION.ToString());// + "_" + (_IsOn ? "ON" : "OFF")) ;
            fahApi.SetDataPoint(this.deviceFaHID, "ch0000", "odp0009", ((byte)_INDICATION).ToString());
        }

        private void OnDataPointEvent(fahApiConnector caller, fahApiConnector.DataPoint dataPoint)
        {
            switch (dataPoint.datapoint)
            {
                case IDPODP.IDP_AL_INFO_ABSOLUTE_SET_POINT_REQUEST:
                    float setPoint = float.Parse(dataPoint.value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    if (_IsInital)
                    {
                        _SetPointTemperature = setPoint;
                        _IsInital = false;
                    }
                    else
                    {
                        if (setPoint >= 5 && setPoint < 35)
                        {
                            OnNewSetTemperature(setPoint);
                        }
                    }                    
                    break;

                case IDPODP.IDP_AL_CONTROLLER_ON_OFF_REQUEST:
                    if (dataPoint.value == "0")
                    {
                        if (_IsOn)
                        {
                            _IsOn = false;
                            fahApi.SetDataPoint(this.DeviceFaHID, CH0, IDPODP.ODP_AL_CONTROLLER_ON_OFF, "0");
                            OnEventDeviceOff?.Invoke(this);
                        }
                        UpdateDisplayIndication();
                    }
                    else
                    {
                        if (!_IsOn)
                        {
                            _IsOn = true;
                            fahApi.SetDataPoint(this.DeviceFaHID, CH0, IDPODP.ODP_AL_CONTROLLER_ON_OFF, "1");
                            if (_IsEcoMode)
                                OnEventDeviceEco?.Invoke(this);
                            else
                                OnEventDeviceOn?.Invoke(this);
                        }
                        UpdateDisplayIndication();
                    }
                    break;

                /*case IDPODP.IDP_AL_FAN_STAGE_REQUEST:
                    //odp000b   AL_FAN_MANUAL_ON_OFF    0/1
                    string fanModeOut = (dataPoint.value == "1" ? "1" : "0");
                    fahApi.SetDataPoint(this.deviceFaHID, CH0, IDPODP.ODP_AL_FAN_MANUAL_ON_OFF, fanModeOut);
                    break;*/

                case IDPODP.IDP_AL_ECO_ON_OFF:
                    if (dataPoint.value == "0")
                    {
                        if (_IsEcoMode)
                        {
                            _IsEcoMode = false;
                            OnEventDeviceOn?.Invoke(this);
                        }
                        UpdateDisplayIndication();
                    }
                    else
                    {
                        if (!_IsEcoMode)
                        {
                            if (_IsOn)
                            {
                                _IsEcoMode = true;
                                OnEventDeviceEco?.Invoke(this);
                            }
                            else
                            {
                                OnEventDeviceOff?.Invoke(this);
                            }
                        }
                        UpdateDisplayIndication();
                    }
                    break;

                default:
                    break;
            }
        }

        public double SetPoint
        {
            get => _SetPointTemperature;
            set
            {
                if (_SetPointTemperature != value)
                {
                    fahApi.SetDataPoint(this.DeviceFaHID, CH0, IDPODP.IDP_AL_INFO_ABSOLUTE_SET_POINT_REQUEST, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }

        private void TmrUpdateTemperature_Elapsed(object sender, ElapsedEventArgs e)
        {            
            if (_newDelayedSetPointTemperature > 3)
            {
                _SetPointTemperature = _newDelayedSetPointTemperature;
                OnEventSetTemperatureChanged?.Invoke(this, _SetPointTemperature);
            }
        }

        private void OnNewSetTemperature(double newTemperature)
        {
            if (newTemperature >= 5 && newTemperature < 35)
            {
                //Add delay of 2.5 seconds
                if ((_SetPointTemperature != newTemperature) || (_newDelayedSetPointTemperature != newTemperature))
                {
                    if (_newDelayedSetPointTemperature != newTemperature)
                    {
                        _newDelayedSetPointTemperature = newTemperature;
                        fahApi.SetDataPoint(this.DeviceFaHID, CH0, IDPODP.ODP_AL_SET_POINT_TEMPERATURE, _newDelayedSetPointTemperature.ToString(System.Globalization.CultureInfo.InvariantCulture));

                        //Update temperature takes 5 sec to prevent overfloading of events
                        if (tmrUpdateTemperature == null)
                        {
                            tmrUpdateTemperature = new Timer(5000);
                            tmrUpdateTemperature.Elapsed += TmrUpdateTemperature_Elapsed;
                            tmrUpdateTemperature.AutoReset = false;

                        }
                        tmrUpdateTemperature.Stop();
                        tmrUpdateTemperature.Start(); 
                    }
                }
            }
        }
    }
}
