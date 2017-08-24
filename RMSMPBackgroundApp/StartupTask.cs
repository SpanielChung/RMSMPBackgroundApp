using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace RMSMPBackgroundApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        // local variables
        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        private string arduinoSerialName = "USB";
        private CancellationTokenSource ReadCancellationTokenSource;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Create the deferral by requesting it from the task instance.
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            //
            // Call asynchronous method(s) using the await keyword.
            Task.WaitAll(ListenAsync());

            // Once the asynchronous method(s) are done, close the deferral.
            deferral.Complete();
        }

        public async Task ListenAsync()
        {
            List<DeviceInformation> listOfDevices = new List<DeviceInformation>();

            try
            {
                string deviceSelector = SerialDevice.GetDeviceSelector();
                var devices = await DeviceInformation.FindAllAsync(deviceSelector);
                for (int i = 0; i < devices.Count; i++)
                {
                    listOfDevices.Add(devices[i]);
                }
                // update device id
                string deviceID = listOfDevices.Where(x => x.Name.Contains(arduinoSerialName)).Select(x => x.Id).FirstOrDefault();

                // get serial port and confirm on screen
                serialPort = await SerialDevice.FromIdAsync(deviceID);


                if (serialPort == null) return;

                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;

                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();


                return;
            }

            catch (Exception ex)
            {
                string error = ex.Message;
                return;
            }

        }

    }


}
