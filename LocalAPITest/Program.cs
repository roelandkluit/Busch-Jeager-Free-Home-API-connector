using KluitNET.FreeAtHome.LocalAPIConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalAPITest
{
    class Program
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
        public static void Main(string[] args)
        {
            string hostname = "sysap";
            string userguid = "6128fbfd-e467-4812-741d-10cd6cddd29b";
            string password = "Password";
            string DeviceToMontor = "ABB701D1B7B4";
            string Channel = "ch0007";

            fahApiConnector fahApiConnection = new fahApiConnector(hostname);
            fahApiConnection.SetCredentials(userguid, password);
            //Enable to collect information on all events recieved by SysAP
            //fahApiConnection.OnDataPointEvent += FahApiConnection_OnDataPointEvent;
            fahApiConnection.Connect(true);
            fahApiConnection.logAllDataPointsToConsole = false;

            //Weather station example
            //fahVirtualWeatherStation virtualWeather = new fahVirtualWeatherStation(fahApiConnection, "WeatherStation");
            //virtualWeather.StartDevice();
            //virtualWeather.SetRainInformation(5);
            //virtualWeather.SetBrightnessLevelLux("1000");
            //virtualWeather.SetTemperatureLevel("18.3");

            //Device monitor example
            fahDeviceSwitchMonitor fahDeviceLamp1Monitor = new fahDeviceSwitchMonitor(fahApiConnection, DeviceToMontor, Channel);
            fahDeviceLamp1Monitor.OnDeviceStatusChangedEvent += FahDeviceMonitor_OnDeviceStatusChangedEvent;

            //Binary input example
            //fahVirtualBinarySensor fahBinSensor = new fahVirtualBinarySensor(fahApiConnection, "BinOutput");
            //fahBinSensor.ActAsImpulsButton = true;
            //fahBinSensor.StartDevice();          

            //Virtual light example
            //fahVirtualSwitch fahVirtualDevice = new fahVirtualSwitch(fahApiConnection, "VirtLight");
            //fahVirtualDevice.OnLightOnOffEvent += FahVirtualDevice_OnLightOnOffEvent;
            //fahVirtualDevice.StartDevice();

            Console.ReadLine();
        }


        private static void FahApiConnection_OnDataPointEvent(fahApiConnector caller, fahApiConnector.DataPoint dataPoint)
        {
            Console.WriteLine("********************** Got new Datapoint *************************");
            Console.WriteLine(dataPoint.JSON);
            Console.WriteLine("********************** End new Datapoint *************************");
        }

        private static void FahDeviceMonitor_OnDeviceStatusChangedEvent(fahDeviceSwitchMonitor caller, bool isActivatedState)
        {
            Console.WriteLine("StateChanged: " + caller.DeviceFaHID + "--> IsActivated: " + isActivatedState);
        }

        private static void FahVirtualDevice_OnLightOnOffEvent(fahVirtualDeviceBase caller, bool lightIsOn)
        {
            Console.WriteLine("LightStatus: " + lightIsOn);
        }
    }
}
