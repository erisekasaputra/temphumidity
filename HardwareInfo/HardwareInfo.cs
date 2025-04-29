
using System;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;


public static class HardwareInfo
{
  public static string GetProcessorId()
  {
    using (var searcher = new ManagementObjectSearcher("select ProcessorId from Win32_Processor"))
    {
      foreach (var item in searcher.Get())
      {
        return item["ProcessorId"]?.ToString();
      }
    }
    return string.Empty;
  }


  public static string GetMotherboardId()
  {
    using (var searcher = new ManagementObjectSearcher("select SerialNumber from Win32_BaseBoard"))
    {
      foreach (var item in searcher.Get())
      {
        return item["SerialNumber"]?.ToString();
      }
    }
    return string.Empty;
  }


  public static string GetMacAddress()
  {
    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
    {
      if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
      {
        return nic.GetPhysicalAddress().ToString();
      }
    }
    return string.Empty;
  }


  public static string GetHardwareId()
  {
    string raw = GetProcessorId() + GetMotherboardId() + GetMacAddress();
    using (SHA256 sha = SHA256.Create())
    {
      byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
      return BitConverter.ToString(hash).Replace("-", "");
    }
  }
}
