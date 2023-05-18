using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Xml.Linq;
using FanControl.Plugins;
using HidSharp;
using static FanControl.NzxtKraken.NzxtKrakenDevice;


namespace FanControl.NzxtKraken
{
    internal class KrakenControlV2 : KrakenControl
    {
        HidDevice _hidDevice;
        byte _channel;
        int _minValue;
        public KrakenControlV2(string id, string name, float resetValue, int minValue, HidDevice hidDevice, byte channel) : base(id, name, resetValue)
        {
            _hidDevice = hidDevice;
            _channel = channel;
            _minValue = minValue;
        }

        public override void Set(float val)
        {
            _hidDevice.TryOpen(out HidStream stream);
            var speed = Math.Min(Math.Max((int)val, _minValue), 100);
            var channel = _channel & 0x70;
            var packet = new byte[] { 0x2, 0x4d, (byte) channel, 0x0, (byte) speed };
  
            stream.Write(packet);
            stream.Close();
        }
    }

    internal class NzxtKrakenX2 : NzxtKrakenDevice
    {
        internal override string Name => "Kraken X2";
        public KrakenSensor liquidTemperature;
        public KrakenSensor pumpSpeed;
        public KrakenControlV2 pumpControl;
        public KrakenSensor fanSpeed;
        public KrakenControlV2 fanControl;
        public NzxtKrakenX2(HidDevice hidDevice, IPluginLogger pluginLogger, IPluginSensorsContainer container) : base(hidDevice, pluginLogger, container)
        {
            liquidTemperature = new KrakenSensor($"liquidtemp-{_serial}", $"Liquid - {Name}");
            _container.TempSensors.Add(liquidTemperature);

            pumpSpeed = new KrakenSensor($"pumprpm-{_serial}", $"Pump - {Name}");
            _container.FanSensors.Add(pumpSpeed);

            pumpControl = new KrakenControlV2($"pumpcontrol-{_serial}", $"Pump - {Name}", 60, 20, hidDevice, 0xc0);
            _container.ControlSensors.Add(pumpControl);

            fanSpeed = new KrakenSensor($"fanrpm-{_serial}", $"Fan - {Name}");
            _container.FanSensors.Add(fanSpeed);

            fanControl = new KrakenControlV2($"fancontrol-{_serial}", $"Fan - {Name}", 30, 0, hidDevice, 0x80);
            _container.ControlSensors.Add(fanControl);
        }

        public override void Update()
        {
            _hidDevice.TryOpen(out HidStream stream);
            var packet = stream.Read();
            SetValues(packet);
            stream.Close();
        }
        static readonly Dictionary<int, int> PUMP_RPM_LOOKUP = new Dictionary<int, int>
            { // We can only estimate, as it is not provided in any output. Hence I applied this ugly hack
                {1200, 40}, {1206, 41}, {1212, 42}, {1218, 43}, {1224, 44}, {1230, 45}, {1236, 46}, {1242, 47}, {1248, 48}, {1254, 49},
                {1260, 50}, {1313, 51}, {1366, 52}, {1419, 53}, {1472, 54}, {1525, 55}, {1578, 56}, {1631, 57}, {1684, 58}, {1737, 59},
                {1790, 60}, {1841, 61}, {1892, 62}, {1943, 63}, {1994, 64}, {2045, 65}, {2096, 66}, {2147, 67}, {2198, 68}, {2249, 69},
                {2300, 70}, {2330, 71}, {2360, 72}, {2390, 73}, {2420, 74}, {2450, 75}, {2480, 76}, {2510, 77}, {2540, 78}, {2570, 79},
                {2600, 80}, {2618, 81}, {2636, 82}, {2654, 83}, {2672, 84}, {2690, 85}, {2708, 86}, {2726, 87}, {2744, 88}, {2762, 89},
                {2780, 90}, {2789, 91}, {2798, 92}, {2807, 93}, {2816, 94}, {2825, 95}, {2834, 96}, {2843, 97}, {2852, 98}, {2861, 99},
                {2870, 100}
            };
        static readonly Dictionary<int, int> FAN_RPM_LOOKUP = new Dictionary<int, int>
            { // We can only estimate, as it is not provided in any output. Hence I applied this ugly hack
                {520, 20}, {521, 21}, {522, 22}, {523, 23}, {524, 24}, {525, 25}, {526, 26}, {527, 27}, {528, 28}, {529, 29},
                {530, 30}, {532, 31}, {534, 32}, {536, 33}, {538, 34}, {540, 35}, {542, 36}, {544, 37}, {546, 38}, {548, 39},
                {550, 40}, {571, 41}, {592, 42}, {613, 43}, {634, 44}, {655, 45}, {676, 46}, {697, 47}, {718, 48}, {739, 49},
                {760, 50}, {781, 51}, {802, 52}, {823, 53}, {844, 54}, {865, 55}, {886, 56}, {907, 57}, {928, 58}, {949, 59},
                {970, 60}, {989, 61}, {1008, 62}, {1027, 63}, {1046, 64}, {1065, 65}, {1084, 66}, {1103, 67}, {1122, 68}, {1141, 69},
                {1160, 70}, {1180, 71}, {1200, 72}, {1220, 73}, {1240, 74}, {1260, 75}, {1280, 76}, {1300, 77}, {1320, 78}, {1340, 79},
                {1360, 80}, {1377, 81}, {1394, 82}, {1411, 83}, {1428, 84}, {1445, 85}, {1462, 86}, {1479, 87}, {1496, 88}, {1513, 89},
                {1530, 90}, {1550, 91}, {1570, 92}, {1590, 93}, {1610, 94}, {1630, 95}, {1650, 96}, {1670, 97}, {1690, 98}, {1720, 99},
                {1980, 100}
            };
        internal virtual void SetValues(byte[] packet)
        {
            liquidTemperature.SetValue(packet[1] + (float)packet[2] / 10);
            var pumpRPM = packet[3] << 8 | packet[4];
            pumpSpeed.SetValue(pumpRPM);
            pumpControl.SetValue(PUMP_RPM_LOOKUP.OrderBy(e => Math.Abs(e.Key - pumpRPM)).FirstOrDefault().Value);

            var fanRPM = packet[5] << 8 | packet[6];
            fanSpeed.SetValue(fanRPM);
            fanControl.SetValue(FAN_RPM_LOOKUP.OrderBy(e => Math.Abs(e.Key - fanRPM)).FirstOrDefault().Value);
        }

        public static new bool SupportsDevice(HidDevice hidDevice)
        {
            return Array.Exists(new int[] { 0x170e }, i => i == hidDevice.ProductID);
        }
    }
}
