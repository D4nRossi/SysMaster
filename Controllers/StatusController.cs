using AForge.Video.DirectShow;
using Microsoft.AspNetCore.Mvc;
using SysMaster.Models;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Device;
using System.Linq;
using System;

namespace SysMaster.Controllers {
    public class StatusController : Controller {
        public IActionResult Index() {
            var viewModel = new StatusViewModel();

            // Medição de hardware e software
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var obj in searcher.Get()) {
                var cpu = (ManagementObject)obj;
                viewModel.CpuName = cpu["Name"].ToString();
                viewModel.CpuCores = int.Parse(cpu["NumberOfCores"].ToString());
            }

            //OS e Processador
            var os = Environment.OSVersion;
            viewModel.OsName = os.VersionString;
            viewModel.FrameworkVersion = RuntimeInformation.FrameworkDescription;

            //RAM
            var ramCounter = new PerformanceCounter("Memory", "Available Bytes");
            var ramValue = ramCounter.NextValue() / (1024 * 1024 * 1024); // em megabytes
            viewModel.RamMemory = ramValue.ToString("F2");


            //Download e Upload
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            long bytesSent = 0;
            long bytesReceived = 0;

            foreach (NetworkInterface ni in interfaces) {
                bytesSent += ni.GetIPv4Statistics().BytesSent;
                bytesReceived += ni.GetIPv4Statistics().BytesReceived;
            }

            double downloadSpeed = (bytesReceived / 1024d) / 1024d / 1024d;
            double uploadSpeed = (bytesSent / 1024d) / 1024d / 1024d;

            viewModel.DownloadSpeed = downloadSpeed.ToString("0.00");
            viewModel.UploadSpeed = uploadSpeed.ToString("0.00");

            //Webcam
            // Obter todas as câmeras disponíveis
            var cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (cameras.Count == 0) {
                viewModel.Webcam = ("Não foi encontrada nenhuma câmera.");
            } else {
                Console.WriteLine("As seguintes câmeras foram encontradas:");
                foreach (FilterInfo camera in cameras) {
                    viewModel.Webcam = ("- " + camera.Name);
                }
            }

            //Nome da máquina
            var machineName = Environment.MachineName;
            viewModel.MaquinaNome = (machineName).ToString();

            //Endereço fisico da maquina
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in networkInterfaces) {
                // check if network interface is Ethernet and it's operational
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                    && networkInterface.OperationalStatus == OperationalStatus.Up) {
                    var physicalAddress = networkInterface.GetPhysicalAddress();
                    viewModel.CaminhoFisico = (physicalAddress.ToString());
                }
            }


            //View
            return View(viewModel);
        }

    }

}
