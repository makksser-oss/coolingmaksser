using LibreHardwareMonitor.Hardware;
namespace LovingCool.Studio2.Sensors;
public sealed class LibreSensors:ISensorProvider{
 readonly Computer computer=new(){IsCpuEnabled=true,IsGpuEnabled=true,IsMemoryEnabled=true,IsMotherboardEnabled=true,IsStorageEnabled=true,IsNetworkEnabled=true,IsControllerEnabled=true};
 public LibreSensors(){computer.Open();}
 public IReadOnlyList<SensorValue> Snapshot(){var list=new List<SensorValue>();foreach(var h in computer.Hardware)Read(h,list);return list;}
 static void Read(IHardware h,List<SensorValue> list){h.Update();foreach(var s in h.Sensors)if(s.Value is float v){string unit=s.SensorType switch{SensorType.Temperature=>"°C",SensorType.Load=>"%",SensorType.Clock=>"MHz",SensorType.Power=>"W",SensorType.Fan=>"RPM",SensorType.Data=>"GB",SensorType.SmallData=>"MB",SensorType.Throughput=>"B/s",_=>""};list.Add(new($"{h.Identifier}/{s.Identifier}",h.Name,s.Name,s.SensorType.ToString(),v,unit));}foreach(var x in h.SubHardware)Read(x,list);}
 public void Dispose()=>computer.Close();
}