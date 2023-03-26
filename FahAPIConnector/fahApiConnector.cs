using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KluitNET.FreeAtHome.LocalAPIConnector
{
    public class fahApiConnector : IDisposable
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

        public enum DeviceFunctionIDs: long
        {
            FID_SWITCH_SENSOR = 0x0000, // Control element
            FID_DIMMING_SENSOR = 0x0001, // Dimming sensor
            FID_BLIND_SENSOR = 0x0003, // Blind sensor
            FID_STAIRCASE_LIGHT_SENSOR = 0x0004, // Stairwell light sensor
            FID_FORCE_ON_OFF_SENSOR = 0x0005, // Force On/Off sensor
            FID_SCENE_SENSOR = 0x0006, // Scene sensor
            FID_SWITCH_ACTUATOR = 0x0007, // Switch actuator
            FID_SHUTTER_ACTUATOR = 0x0009, // Blind actuator
            FID_ROOM_TEMPERATURE_CONTROLLER_MASTER_WITH_FAN = 0x000A, // Room temperature controller with fan speed level
            FID_ROOM_TEMPERATURE_CONTROLLER_SLAVE = 0x000B, // Room temperature controller extension unit
            FID_WIND_ALARM_SENSOR = 0x000C, // Wind Alarm
            FID_FROST_ALARM_SENSOR = 0x000D, // Frost Alarm
            FID_RAIN_ALARM_SENSOR = 0x000E, // Rain Alarm
            FID_WINDOW_DOOR_SENSOR = 0x000F, // Window sensor
            FID_MOVEMENT_DETECTOR = 0x0011, // Movement Detector
            FID_DIMMING_ACTUATOR = 0x0012, // Dim actuator
            FID_RADIATOR_ACTUATOR = 0x0014, // Radiator
            FID_UNDERFLOOR_HEATING = 0x0015, // Underfloor heating
            FID_FAN_COIL = 0x0016, // Fan Coil
            FID_TWO_LEVEL_CONTROLLER = 0x0017, // Two-level controller
            FID_DES_DOOR_OPENER_ACTUATOR = 0x001A, // Door opener
            FID_PROXY = 0x001B, // Proxy
            FID_DES_LEVEL_CALL_ACTUATOR = 0x001D, // Door Entry System Call Level Actuator
            FID_DES_LEVEL_CALL_SENSOR = 0x001E, // Door Entry System Call Level Sensor
            FID_DES_DOOR_RINGING_SENSOR = 0x001F, // Door call
            FID_DES_AUTOMATIC_DOOR_OPENER_ACTUATOR = 0x0020, // Automatic door opener
            FID_DES_LIGHT_SWITCH_ACTUATOR = 0x0021, // Corridor light
            FID_ROOM_TEMPERATURE_CONTROLLER_MASTER_WITHOUT_FAN = 0x0023, // Room temperature controller
            FID_COOLING_ACTUATOR = 0x0024, // Cooling mode
            FID_HEATING_ACTUATOR = 0x0027, // Heating mode
            FID_FORCE_UP_DOWN_SENSOR = 0x0028, // Force-position blind
            FID_HEATING_COOLING_ACTUATOR = 0x0029, // Auto. heating/cooling mode
            FID_HEATING_COOLING_SENSOR = 0x002A, // Switchover heating/cooling
            FID_DES_DEVICE_SETTINGS = 0x002B, // Device settings
            FID_RGB_W_ACTUATOR = 0x002E, // Dim actuator
            FID_RGB_ACTUATOR = 0x002F, // Dim actuator
            FID_PANEL_SWITCH_SENSOR = 0x0030, // Control element
            FID_PANEL_DIMMING_SENSOR = 0x0031, // Dimming sensor
            FID_PANEL_BLIND_SENSOR = 0x0033, // Blind sensor
            FID_PANEL_STAIRCASE_LIGHT_SENSOR = 0x0034, // Stairwell light sensor
            FID_PANEL_FORCE_ON_OFF_SENSOR = 0x0035, // Force On/Off sensor
            FID_PANEL_FORCE_UP_DOWN_SENSOR = 0x0036, // Force-position blind
            FID_PANEL_SCENE_SENSOR = 0x0037, // Scene sensor
            FID_PANEL_ROOM_TEMPERATURE_CONTROLLER_SLAVE = 0x0038, // Room temperature controller extension unit
            FID_PANEL_FAN_COIL_SENSOR = 0x0039, // Fan coil sensor
            FID_PANEL_RGB_CT_SENSOR = 0x003A, // RGB + warm white/cold white sensor
            FID_PANEL_RGB_SENSOR = 0x003B, // RGB sensor
            FID_PANEL_CT_SENSOR = 0x003C, // Warm white/cold white sensor
            FID_ADDITIONAL_HEATING_ACTUATOR = 0x003D, // Add. stage for heating mode
            FID_RADIATOR_ACTUATOR_MASTER = 0x003E, // Radiator thermostate
            FID_RADIATOR_ACTUATOR_SLAVE = 0x003F, // Room temperature controller extension unit
            FID_BRIGHTNESS_SENSOR = 0x0041, // Brightness sensor
            FID_RAIN_SENSOR = 0x0042, // Rain sensor
            FID_TEMPERATURE_SENSOR = 0x0043, // Temperature sensor
            FID_WIND_SENSOR = 0x0044, // Wind sensor
            FID_TRIGGER = 0x0045, // Trigger
            FID_FCA_2_PIPE_HEATING = 0x0047, // Heating mode
            FID_FCA_2_PIPE_COOLING = 0x0048, // Cooling mode
            FID_FCA_2_PIPE_HEATING_COOLING = 0x0049, // Auto. heating/cooling mode
            FID_FCA_4_PIPE_HEATING_AND_COOLING = 0x004A, // Two valves for heating and cooling
            FID_WINDOW_DOOR_ACTUATOR = 0x004B, // Window/Door
            FID_INVERTER_INFO = 0x004E, // ABC
            FID_METER_INFO = 0x004F, // ABD
            FID_BATTERY_INFO = 0x0050, // ACD
            FID_PANEL_TIMER_PROGRAM_SWITCH_SENSOR = 0x0051, // Timer program switch sensor
            FID_DOMUSTECH_ZONE = 0x0055, // Zone
            FID_CENTRAL_HEATING_ACTUATOR = 0x0056, // Central heating actuator
            FID_CENTRAL_COOLING_ACTUATOR = 0x0057, // Central cooling actuator
            FID_HOUSE_KEEPING = 0x0059, // Housekeeping
            FID_MEDIA_PLAYER = 0x005A, // Media Player
            FID_PANEL_ROOM_TEMPERATURE_CONTROLLER_SLAVE_FOR_BATTERY_DEVICE = 0x005B, // Panel Room Temperature Controller Slave For Battery Device
            FID_PANEL_MEDIA_PLAYER_SENSOR = 0x0060, // Media Player Sensor
            FID_BLIND_ACTUATOR = 0x0061, // Roller blind actuator
            FID_ATTIC_WINDOW_ACTUATOR = 0x0062, // Attic window actuator
            FID_AWNING_ACTUATOR = 0x0063, // Awning actuator
            FID_WINDOW_DOOR_POSITION_SENSOR = 0x0064, // WindowDoor Position Sensor
            FID_WINDOW_DOOR_POSITION_ACTUATOR = 0x0065, // Window/Door position
            FID_MEDIA_PLAYBACK_CONTROL_SENSOR = 0x0066, // Media playback control sensor
            FID_MEDIA_VOLUME_SENSOR = 0x0067, // Media volume sensor
            FID_DISHWASHER = 0x0068, // Dishwasher
            FID_LAUNDRY = 0x0069, // Laundry
            FID_DRYER = 0x006A, // Dryer
            FID_OVEN = 0x006B, // Oven
            FID_FRIDGE = 0x006C, // Fridge
            FID_FREEZER = 0x006D, // Freezer
            FID_HOOD = 0x006E, // Hood
            FID_COFFEE_MACHINE = 0x006F, // Coffee machine
            FID_FRIDGE_FREEZER = 0x0070, // Fridge/Freezer
            FID_TIMER_PROGRAM_OR_ALERT_SWITCH_SENSOR = 0x0071, // Timer program switch sensor
            FID_CEILING_FAN_ACTUATOR = 0x0073, // Ceiling fan actuator
            FID_CEILING_FAN_SENSOR = 0x0074, // Ceiling fan sensor
            FID_SPLIT_UNIT_GATEWAY = 0x0075, // Room temperature controller with fan speed level
            FID_ZONE = 0x0076, // Zone
            FID_24H_ZONE = 0x0077, // Safety
            FID_EXTERNAL_IR_SENSOR_BX80 = 0x0078, // External IR Sensor BX80
            FID_EXTERNAL_IR_SENSOR_VXI = 0x0079, // External IR Sensor VXI
            FID_EXTERNAL_IR_SENSOR_MINI = 0x007A, // External IR Sensor Mini
            FID_EXTERNAL_IR_SENSOR_HIGH_ALTITUDE = 0x007B, // External IR Sensor High Altitude
            FID_EXTERNAL_IR_SENSOR_CURTAIN = 0x007C, // External IR Sensor Curtain
            FID_SMOKE_DETECTOR = 0x007D, // Smoke Detector
            FID_CARBON_MONOXIDE_SENSOR = 0x007E, // Carbon Monoxide Sensor
            FID_METHANE_DETECTOR = 0x007F, // Methane Detector
            FID_GAS_SENSOR_LPG = 0x0080, // Gas Sensor LPG
            FID_FLOOD_DETECTION = 0x0081, // Flood Detection
            FID_DOMUS_CENTRAL_UNIT_NEXTGEN = 0x0082, // secure@home Central Unit
            FID_THERMOSTAT = 0x0083, // Thermostat
            FID_PANEL_DOMUS_ZONE_SENSOR = 0x0084, // secure@home Zone Sensor
            FID_THERMOSTAT_SLAVE = 0x0085, // Slave thermostat
            FID_DOMUS_SECURE_INTEGRATION = 0x0086, // secure@home Integration Logic
            FID_ADDITIONAL_COOLING_ACTUATOR = 0x0087, // Add. stage for cooling mode
            FID_TWO_LEVEL_HEATING_ACTUATOR = 0x0088, // Two Level Heating Actuator
            FID_TWO_LEVEL_COOLING_ACTUATOR = 0x0089, // Two Level Cooling Actuator
            FID_GLOBAL_ZONE = 0x008E, // Zone
            FID_VOLUME_UP_SENSOR = 0x008F, // Volume up
            FID_VOLUME_DOWN_SENSOR = 0x0090, // Volume down
            FID_PLAY_PAUSE_SENSOR = 0x0091, // Play/pause
            FID_NEXT_FAVORITE_SENSOR = 0x0092, // Next favorite
            FID_NEXT_SONG_SENSOR = 0x0093, // Next song
            FID_PREVIOUS_SONG_SENSOR = 0x0094, // Previous song
            FID_HOME_APPLIANCE_SENSOR = 0x0095, // Home appliance sensor
            FID_HEAT_SENSOR = 0x0096, // Heat sensor
            FID_ZONE_SWITCHING = 0x0097, // Zone switching
            FID_SECURE_AT_HOME_FUNCTION = 0x0098, // Button function
            FID_COMPLEX_CONFIGURATION = 0x0099, // Advanced configuration
            FID_DOMUS_CENTRAL_UNIT_BASIC = 0x009A, // secure@home Central Unit Basic
            FID_DOMUS_REPEATER = 0x009B, // Repeater
            FID_DOMUS_SCENE_TRIGGER = 0x009C, // Remote scene control
            FID_DOMUSWINDOWCONTACT = 0x009D, // Window sensor
            FID_DOMUSMOVEMENTDETECTOR = 0x009E, // Movement Detector
            FID_DOMUSCURTAINDETECTOR = 0x009F, // External IR Sensor Curtain
            FID_DOMUSSMOKEDETECTOR = 0x00A0, // Smoke Detector
            FID_DOMUSFLOODDETECTOR = 0x00A1, // Flood Detection
            FID_PANEL_SUG_SENSOR = 0x00A3, // Sensor for air-conditioning unit
            FID_TWO_LEVEL_HEATING_COOLING_ACTUATOR = 0x00A4, // Two-point controller for heating or cooling
            FID_PANEL_THERMOSTAT_CONTROLLER_SLAVE = 0x00A5, // Slave thermostat
            FID_WALLBOX = 0x00A6, // Wallbox
            FID_PANEL_WALLBOX = 0x00A7, // Wallbox
            FID_DOOR_LOCK_CONTROL = 0x00A8, // Door lock control
            FID_VRV_GATEWAY = 0x00AA, // Room temperature controller with fan speed level
        }

        public enum DataPointType
        {
            NormalDataPoint,
            SceneTriggerDataPoint
        }

        public class DeviceData
        {
            public string device;
            public string DisplayName;
            public DeviceFunctionIDs functionID;

            internal void setFunctionID(string hexFunctionID)
            {
                functionID = (DeviceFunctionIDs)long.Parse(hexFunctionID, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public class DataPoint
        {
            public DataPointType dataPointType = DataPointType.NormalDataPoint;
            public string device;
            public string channel;
            public string datapoint;
            public string value;
            public string pairingID;
            public string JSON;

            public void SetValue(int value)
            {
                this.value = value.ToString();
            }

            public void SetValue(string value)
            {
                this.value = value;
            }

            public void SetValue(double value)
            {
                this.value = value.ToString("F", CultureInfo.InvariantCulture);
            }

            public override string ToString()
            {            
               return device + ": " + channel + "-" + datapoint + " => " + value;

            }
        }

        public delegate void EventOnDataPoint(fahApiConnector caller, DataPoint dataPoint);
        public event EventOnDataPoint OnDataPointEvent;
        public delegate void EventOnDeviceData(fahApiConnector caller, DeviceData deviceData);
        public event EventOnDeviceData onDeviceData;

        public delegate void EventOnOnlineStatusChanged(fahApiConnector caller, bool isOnline);
        public event EventOnOnlineStatusChanged OnOnlineStatusChangedEvent;

        private Queue<DataPoint> dpsToQuery = new Queue<DataPoint>();
        private bool isTerminating = false;
        private bool isApiOnline = false;
        private ClientWebSocket ws = null;
        private string EndPointURL = "sysap";
        private Thread wsThread = null;
        private string UsernamePassword = "YWRtaW46YWRtaW4="; //admin:admin
        public bool logAllDataPointsToConsole = false;

        public bool isApiWSConnectionActive
        {
            get
            {
                return isApiOnline;
            }
            private set
            {
                isApiOnline = value;
                try
                {
                    OnOnlineStatusChangedEvent?.Invoke(this, value);
                }
                catch { }
            }
        }

        public void RequestDataPoint(string fahDevice, string Channel, string DataPoint)
        {
            DataPoint d = new DataPoint();
            d.device = fahDevice;
            d.channel = Channel;
            d.datapoint = DataPoint;
            dpsToQuery.Enqueue(d);
        }

        public fahApiConnector(string EndPointName)
        {
            EndPointURL = EndPointName;
        }

        public void SetCredentials(string Username, string Password)
        {
            UsernamePassword = Convert.ToBase64String(Encoding.Default.GetBytes(Username + ":" + Password));
        }

        private static string GetDeviceFromCreationResponse(string deviceCreation)
        {
            try
            {
                JToken jTok = JObject.Parse(deviceCreation)["00000000-0000-0000-0000-000000000000"]["devices"];
                if (jTok.Count<object>() == 1)
                {
                    foreach (var x in (JObject)jTok)
                    {
                        return x.Key;
                    }
                }
                return "";
            }
            catch(Exception e)
            {
                Console.WriteLine("Unable to get device response:" + e);
                return "";
            }
        }

        private WebClient ConstructWebClient()
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateServerCertificate);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "FaHApiConsumer");
            client.Headers.Add("Authorization", "Basic " + UsernamePassword);
            return client;
        }

        internal bool RegisterDevice(string serial, string deviceType, out string deviceFaHID)
        {
            try
            {
                using (WebClient client = ConstructWebClient())
                {
                    string ret = client.UploadString("https://" + EndPointURL + "/fhapi/v1/api/rest/virtualdevice/00000000-0000-0000-0000-000000000000/" + serial, WebRequestMethods.Http.Put, "{\"type\": \"" + deviceType + "\", \"properties\": {\"ttl\": \"300\"}}");
                    //Console.WriteLine(ret);
                    deviceFaHID = GetDeviceFromCreationResponse(ret);
                    if (deviceFaHID != "")
                    {
                        if (logAllDataPointsToConsole)  Console.WriteLine("Device registration: " + deviceFaHID + " ApiName:" + serial);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("unable to register: " + ret);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to register device: " + e);
            }
            deviceFaHID = "";
            return false;
        }

        internal bool SetDataPoint(DataPoint dataPoint)
        {
            return SetDataPoint(dataPoint.device, dataPoint.channel, dataPoint.datapoint, dataPoint.value);
        }

        private static int GetStatusCode(WebClient client, out string statusDescription)
        {
            FieldInfo responseField = client.GetType().GetField("m_WebResponse", BindingFlags.Instance | BindingFlags.NonPublic);

            if (responseField != null)
            {
                using (HttpWebResponse response = responseField.GetValue(client) as HttpWebResponse)
                {
                    if (response != null)
                    {
                        statusDescription = response.StatusDescription;
                        int status = (int)response.StatusCode;
                        responseField = null;
                        return status;
                    }
                }
            }

            statusDescription = null;
            return 0;
        }

        /*static public void testc()
        {
            string w = "{\"00000000-0000-0000-0000-000000000000\": {\"datapoints\": {},\"devices\": {\"60005CD9523A\": {\"floor\": \"03\",\"room\": \"02\",\"interface\": \"vdev:6128fafd-e467-4812-821c-10cd6ccdd28b@busch-jaeger.de\",\"displayName\": \"Switch actuator\",\"unresponsive\": false,\"channels\": {\"ch0000\": {\"displayName\": \"Switch actuator\",\"functionID\": \"7\",\"inputs\": {\"idp0000\": {\"pairingID\": 1,\"value\": \"0\"},\"idp0001\": {\"pairingID\": 2,\"value\": \"0\"},\"idp0002\": {\"pairingID\": 3,\"value\": \"0\"},\"idp0003\": {\"pairingID\": 4,\"value\": \"0\"},\"idp0004\": {\"pairingID\": 6,\"value\": \"0\"},\"idp0005\": {\"pairingID\": 48,\"value\": \"0\"},\"idp0006\": {\"pairingID\": 323,\"value\": \"0\"},\"idp0007\": {\"pairingID\": 333,\"value\": \"0\"},\"idp0008\": {\"pairingID\": 334,\"value\": \"0\"}},\"outputs\": {\"odp0000\": {\"pairingID\": 256,\"value\": \"0\"},\"odp0001\": {\"pairingID\": 257,\"value\": \"0\"},\"odp0002\": {\"pairingID\": 273,\"value\": \"0\"},\"odp0003\": {\"pairingID\": 305,\"value\": \"0\"},\"odp0004\": {\"pairingID\": 321,\"value\": \"0\"},\"odp0005\": {\"pairingID\": 335,\"value\": \"0\"},\"odp0006\": {\"pairingID\": 336,\"value\": \"0\"}}}}}},\"devicesAdded\": [\"60005CD9523A\"],\"devicesRemoved\": [],\"scenesTriggered\": {}}}";
            JObject jData = GetJsonObject(w);
            ProcessDevices(jData, w);
        }*/

        internal bool SetDataPoint(string deviceFaHID, string channel, string datapoint, string value)
        {
            string dp = deviceFaHID + "." + channel + "." + datapoint;
            if (string.IsNullOrEmpty(deviceFaHID))
            {
                Console.WriteLine("Unable to set datapoint, FAHDeviceID empty (" + dp + "-->" + value + ")");
                return false;
            }
            
            try
            {
                using (WebClient client = ConstructWebClient())
                {
                    string ret = client.UploadString("https://" + EndPointURL + "/fhapi/v1/api/rest/datapoint/00000000-0000-0000-0000-000000000000/" + dp, WebRequestMethods.Http.Put, value);
                    if (ret.Contains("\"result\": \"OK\""))
                    {
                        return true;
                    }
                    string status;
                    int iret = GetStatusCode(client, out status);
                    Console.WriteLine(iret + "-->" + status + "-->" + ret);
                    return false;
                }
            }
            catch (Exception e)
            {               
                Console.WriteLine("Unable to set datapoint (" + dp + "-->" + value + "):" + e);
            }
            return false;
        }

        public void Connect(bool wait = false)
        {          
            if (wsThread != null)
            {
                throw new Exception("Invalid State");
            }
            wsThread = new Thread(new ThreadStart(ThreadProc));
            wsThread.Start();

            if(wait)
            {
                while(!isApiWSConnectionActive)
                {
                    Thread.Sleep(250);
                }
            }
        }

        public void Disconnect()
        {
            Dispose();
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
            /*
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if(certificate.Subject == "E=info@abb.com, CN=free@home System Access Point, O=ABB, L=Luedenscheid, S=Germany, C=DE")
            {
                if(certificate.Issuer== certificate.Subject)
                {
                    return true;
                }
            }

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;*/
        }

        internal bool GetDeviceConfigFromAP(DeviceData deviceData)
        {
            try
            {
                using (WebClient client = ConstructWebClient())
                {
                    string dataPointValue = client.DownloadString("https://" + EndPointURL + "/fhapi/v1/api/rest/device/00000000-0000-0000-0000-000000000000/" + deviceData.device);
                    JObject jsonArraydataPointValue = (JObject)(JObject.Parse(dataPointValue)["00000000-0000-0000-0000-000000000000"]);
                    ProcessDevices(jsonArraydataPointValue, dataPointValue);
                    //ProcessInitialDataPoints(jsonArraydataPointValue, dataPointValue, deviceData.device);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to get device Config: " + e);
            }
            return false;
        }

        public JObject GetDeviceJSON(string device)
        {
            try
            {
                using (WebClient client = ConstructWebClient())
                {
                    string dataPointValue = client.DownloadString("https://" + EndPointURL + "/fhapi/v1/api/rest/device/00000000-0000-0000-0000-000000000000/" + device);
                    return (JObject)JObject.Parse(dataPointValue)["00000000-0000-0000-0000-000000000000"]["devices"];
                }
            }
            catch
            {
                return null;
            }
        }

        private bool GetDataPointValueFromAP(DataPoint dataPoint)
        {
            if (dataPoint == null)
                return true;
            try
            {
                using (WebClient client = ConstructWebClient())
                {
                    if (client == null)
                        throw new Exception("Unable to construct webclient");                    
                    string dataPointValue = client.DownloadString("https://" + EndPointURL + "/fhapi/v1/api/rest/datapoint/00000000-0000-0000-0000-000000000000/" + dataPoint.device + "." + dataPoint.channel + "." + dataPoint.datapoint);
                    JArray jsonArraydataPointValue = (JArray)JObject.Parse(dataPointValue)["00000000-0000-0000-0000-000000000000"]["values"];
                    dataPoint.value = jsonArraydataPointValue.FirstOrDefault().ToString();
                    try
                    {
                        OnDataPointEvent?.Invoke(this, dataPoint);
                    }
                    catch { }
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to get datapoint: " + e + " " + e.InnerException);
            }
            return false;
        }

        public class MyCertificateValidation : ICertificatePolicy
        {

            public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest request, int problem)
            {
                return true;
            }
        }

        public void ThreadProc()
        {
            while (!isTerminating)
            {
                HttpClientHandler handler = new HttpClientHandler();
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                //Needed for MONO where ServerCertificateValidationCallback is not implemented
                #pragma warning disable CS0618 // Type or member is obsolete
                ServicePointManager.CertificatePolicy = new MyCertificateValidation();
                #pragma warning restore CS0618 // Type or member is obsolete
                Console.WriteLine("Connecting WebSocket");
                ws = new ClientWebSocket();
                dpsToQuery.Clear();
                Task wsTask = SysAPWebsocketConnection();

                while (wsTask.Status != TaskStatus.Canceled && wsTask.Status != TaskStatus.Faulted && wsTask.Status != TaskStatus.RanToCompletion)
                {
                    if(dpsToQuery.Count > 0)
                    {                     
                        DataPoint dpToQuery = dpsToQuery.Dequeue();
                        if (!GetDataPointValueFromAP(dpToQuery))
                        {
                            //Hold off a couple of seconds when failed.
                            Thread.Sleep(5000);
                            dpsToQuery.Enqueue(dpToQuery);
                        }
                    }
                    if (!isTerminating)
                    {
                        if (dpsToQuery.Count == 0)
                        {
                            Thread.Sleep(250);
                        }
                        else
                        {
                            Thread.Sleep(25);
                        }
                    }
                }
                if (!isTerminating)
                {
                    Thread.Sleep(10000);
                }
            }
        }

        private void ProcessDevices(JObject jFahMsgData, string sJSON)
        {
            //Get Devices
            try
            {
                DeviceData dpDeviceDataReport = new DeviceData();

                JObject devices = (JObject)jFahMsgData["devices"];
                foreach (var device in (JObject)devices)
                {
                    dpDeviceDataReport.device = device.Key.ToString();
                    dpDeviceDataReport.DisplayName = device.Value["displayName"].ToString();
                    dpDeviceDataReport.setFunctionID(device.Value["channels"]["ch0000"]["functionID"].ToString());

                    try
                    {
                        onDeviceData?.Invoke(this, dpDeviceDataReport);
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to process device event: " + e);
                Console.WriteLine("Recieved JSON: " + sJSON);
            }
        }

        /*
        private void ProcessInitialDataPoints(JObject jFahMsgData, string sJSON, string FahID)
        {
            //Get DataPoints
            try
            {
                DataPoint dpDataPointReport = new DataPoint();
                dpDataPointReport.dataPointType = DataPointType.NormalDataPoint;
                dpDataPointReport.JSON = sJSON;

                JObject datapoints = (JObject)jFahMsgData["devices"][FahID]["channels"];
                foreach (var dataPoint in (JObject)datapoints)
                {
                    dpDataPointReport.value = dataPoint.Value.ToString();
                    string[] items = dataPoint.Key.Split('/');
                    dpDataPointReport.device = items[0];
                    dpDataPointReport.channel = items[1];
                    dpDataPointReport.datapoint = items[2];
                    OnDataPointEvent?.Invoke(this, dpDataPointReport);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to process datapoint event: " + e);
                Console.WriteLine("Recieved JSON: " + e);
            }
        }*/

        private void ProcessDataPoints(JObject jFahMsgData, string sJSON)
        {
            //Get DataPoints
            try
            {
                DataPoint dpDataPointReport = new DataPoint();
                dpDataPointReport.dataPointType = DataPointType.NormalDataPoint;
                dpDataPointReport.JSON = sJSON;

                JObject datapoints = (JObject)jFahMsgData["datapoints"];
                foreach (var dataPoint in (JObject)datapoints)
                {
                    dpDataPointReport.value = dataPoint.Value.ToString();
                    string[] items = dataPoint.Key.Split('/');
                    dpDataPointReport.device = items[0];
                    dpDataPointReport.channel = items[1];
                    dpDataPointReport.datapoint = items[2];
                    try
                    {
                        OnDataPointEvent?.Invoke(this, dpDataPointReport);
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to process datapoint event: " + e);
                Console.WriteLine("Recieved JSON: " + sJSON);
            }
        }

        private void ProcessSceneTriggers(JObject jFahMsgData, string sJSON)
        {
            //Get Scene trigggers
            try
            {
                DataPoint dpSceneTrigger = new DataPoint();
                dpSceneTrigger.dataPointType = DataPointType.SceneTriggerDataPoint;
                dpSceneTrigger.JSON = sJSON;

                JObject scenesTriggered = (JObject)jFahMsgData["scenesTriggered"];

                foreach (var scenes in (JObject)scenesTriggered)
                {
                    dpSceneTrigger.device = scenes.Key;
                    JObject channels = (JObject)scenes.Value["channels"];

                    foreach (var channel in (JObject)channels)
                    {
                        dpSceneTrigger.channel = channel.Key;
                        JObject outputs = (JObject)channel.Value["outputs"];
                        foreach (var output in (JObject)outputs)
                        {
                            dpSceneTrigger.datapoint = output.Key;
                            dpSceneTrigger.value = output.Value["value"].ToString();
                            dpSceneTrigger.pairingID = output.Value["pairingID"].ToString();
                            try
                            {
                                OnDataPointEvent?.Invoke(this, dpSceneTrigger);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to process scene event: " + e);
                Console.WriteLine("Recieved JSON: " + sJSON);
            }
        }

        static private JObject GetJsonObject(string JSON)
        {
            if (JSON == "")
                return null;

            try
            {
                JObject jObject = (JObject)JObject.Parse(JSON)["00000000-0000-0000-0000-000000000000"];
                return jObject;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to process JSON: " + e);
                Console.WriteLine("Recieved JSON: " + JSON);
                return null;
            }
        }

        private async Task SysAPWebsocketConnection()
        {
            const int BUFFSIZE = 4096;
            ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[BUFFSIZE]);
            try
            {
                ws.Options.SetRequestHeader("Authorization", "Basic " + UsernamePassword);
                Uri serverUri = new Uri("wss://" + EndPointURL + "/fhapi/v1/api/ws");
                await ws.ConnectAsync(serverUri, CancellationToken.None);
                while (ws.State == WebSocketState.Open)
                {
                    string eventdata = "";
                    isApiWSConnectionActive = true;

                    while (true)
                    {
                        WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived, CancellationToken.None);
                        if (result.Count < BUFFSIZE - 1)
                        {
                            bytesReceived.Array[result.Count + 1] = 0;
                        }
                        else
                        {
                            Console.WriteLine("Possible buffer overflow!");
                        }

                        //Get event record
                        eventdata += Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count);
                        if (result.EndOfMessage) //Is this the end of the message or is it chunked?
                            break;
                        //else
                          //  Console.WriteLine("Chunked");
                    }

                    if (logAllDataPointsToConsole)
                    {
                        Console.WriteLine(eventdata);
                    }

                    //Process Json Data
                    JObject jData = GetJsonObject(eventdata);
                    if (jData != null)
                    {
                        //Datapoints
                        ProcessDataPoints(jData, eventdata);
                        //Scene triggers
                        ProcessSceneTriggers(jData, eventdata);
                        //Devices
                        ProcessDevices(jData, eventdata);
                    }
                }
            }
            catch(Exception e)
            {
                isApiWSConnectionActive = false;
                Console.WriteLine(e);
            }
            isApiWSConnectionActive = false;
        }

        public void Dispose()
        {
            isTerminating = true;
            if (wsThread.IsAlive)
            {
                if(ws.State == WebSocketState.Open)
                {
                    ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "ProgrammTerminating", CancellationToken.None);
                }
                else
                {
                    ws = null;
                }
            }
        }
    }
}
