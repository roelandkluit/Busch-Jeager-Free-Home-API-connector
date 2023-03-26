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

    public class fahVirtualWeatherStation : fahVirtualDeviceBase
    {
        public delegate void EventFahDataPoint(fahVirtualDeviceBase caller, fahApiConnector.DataPoint dataPoint);
        public event EventFahDataPoint OnFahDataPointEvent;
        private Timer CheckMaintananceTimer = new Timer();
        private string lvBrightness = "";
        private double lvRain = -1;
        private string lvTemperature = "";
        private string lvWindSpeed = "";
        private string lvWindBeaufort = "";

        //Chan0
        //0x0041	FID_BRIGHTNESS_SENSOR
        //  0 >> 0x0402	    AL_BRIGHTNESS_ALARM	Brightness alarm	Brightness alarm
        //  1 >> 0x0403     AL_BRIGHTNESS_LEVEL Lux value Weatherstation brightness level
        //  2 >> 0x0004	    AL_SCENE_CONTROL	Scene Control	Recall or learn the set value related to encoded scene number

        public void SetBrightnessLevelWM2(string level)
        {
            int ilevel = (Int32.Parse(level) * 63);

            if(ilevel.ToString() == lvBrightness)
                return;

            fahApiConnector.DataPoint dataPoint = new fahApiConnector.DataPoint();
            dataPoint.channel = "ch0000";
            dataPoint.device = this.deviceFaHID;
            dataPoint.datapoint = "odp0001";
            dataPoint.SetValue(ilevel);
            fahApi.SetDataPoint(dataPoint);
            lvBrightness = ilevel.ToString();
        }

        public void SetBrightnessLevelLux(string level)
        {
            if (level == lvBrightness)
                return;

            fahApiConnector.DataPoint dataPoint = new fahApiConnector.DataPoint();
            dataPoint.channel = "ch0000";
            dataPoint.device = this.deviceFaHID;
            dataPoint.datapoint = "odp0001";
            dataPoint.SetValue(level);
            fahApi.SetDataPoint(dataPoint);
            lvBrightness = level;
        }

        //Chan1
        //0x0042	FID_RAIN_SENSOR
        //  0 >> 0x0027	    AL_RAIN_ALARM	    Rain Alarm	State of the rain sensor (sent cyclically and on COV)
        //  1 >> 0x0004	    AL_SCENE_CONTROL	Scene Control	Recall or learn the set value related to encoded scene number
        //  2 >> 0x0405	    AL_RAIN_SENSOR_ACTIVATION_PERCENTAGE Rain detection
        //  3 >> 0x0406	    AL_RAIN_SENSOR_FREQUENCY	Rain sensor frequency

        public void SetRainInformation(double amount_of_rain)
        {
            if (lvRain == amount_of_rain)
                return;

            fahApiConnector.DataPoint dataPoint = new fahApiConnector.DataPoint();
            dataPoint.channel = "ch0001";
            dataPoint.device = this.deviceFaHID;
            dataPoint.datapoint = "odp0000";
            dataPoint.SetValue(amount_of_rain > 0 ? 1: 0);
            fahApi.SetDataPoint(dataPoint);

            dataPoint.datapoint = "odp0002";
            dataPoint.SetValue(amount_of_rain);
            fahApi.SetDataPoint(dataPoint);
            lvRain = amount_of_rain;
        }

        //chan2
        //0x0043	FID_TEMPERATURE_SENSOR
        //  0 >> 0x0026	    AL_FROST_ALARM	    Frost Alarm	State of the frost sensor (sent cyclically and on COV) Moves the sunblind to a secure position and to block it for any further control
        //  1 >> 0x0400	    AL_OUTDOOR_TEMPERATURE	Outside temperature	Outdoor Temperature
        //  2 >> 0x0004	    AL_SCENE_CONTROL	Scene Control	Recall or learn the set value related to encoded scene number

        public void SetTemperatureLevel(string level)
        {
            if (lvTemperature == level)
                return;

            double ilevel = double.Parse(level);
            fahApiConnector.DataPoint dataPoint = new fahApiConnector.DataPoint();
            dataPoint.channel = "ch0002";
            dataPoint.device = this.deviceFaHID;
            dataPoint.datapoint = "odp0001";
            dataPoint.SetValue(level);
            fahApi.SetDataPoint(dataPoint);

            dataPoint.datapoint = "odp0000";
            dataPoint.SetValue(ilevel < 2 ? 1 : 0);
            fahApi.SetDataPoint(dataPoint);
            lvTemperature = level;
        }

        //chan3
        //0x0044	FID_WIND_SENSOR
        //  0 >> 0x0025	    AL_WIND_ALARM	    Wind Alarm	State of the wind sensor (sent cyclically and on COV) Moves the sunblind to a secure position and to block it for any further control
        //  1 >> 0x0401	    AL_WIND_FORCE	    Wind force	Wind force
        //  2 >> 0x0004	    AL_SCENE_CONTROL	Scene Control	Recall or learn the set value related to encoded scene number
        //  3 >> 0x0404	    AL_WIND_SPEED	    Wind speed	Wind speed

        public void SetWindSpeed(string speedBaufort, string speedMS)
        {
            if (lvWindBeaufort != speedBaufort)
            {
                double ispeed = double.Parse(speedBaufort);
                fahApiConnector.DataPoint dataPoint = new fahApiConnector.DataPoint();
                dataPoint.channel = "ch0003";
                dataPoint.device = this.deviceFaHID;
                dataPoint.datapoint = "odp0001";
                dataPoint.SetValue(speedBaufort);
                fahApi.SetDataPoint(dataPoint);

                dataPoint.datapoint = "odp0000";
                dataPoint.SetValue(ispeed > 5 ? 1 : 0);
                fahApi.SetDataPoint(dataPoint);
                lvWindBeaufort = speedBaufort;
            }

            if (lvWindSpeed != speedMS)
            {
                fahApiConnector.DataPoint dataPoint = new fahApiConnector.DataPoint();
                dataPoint.channel = "ch0003";
                dataPoint.device = this.deviceFaHID;
                dataPoint.datapoint = "odp0003";
                dataPoint.SetValue(speedMS);
                fahApi.SetDataPoint(dataPoint);
                lvWindSpeed = speedMS;
            }
        }

        public fahVirtualWeatherStation(fahApiConnector fahApiConnector, string DeviceSerialNumber) : base(fahApiConnector, DeviceSerialNumber, "WeatherStation")
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
