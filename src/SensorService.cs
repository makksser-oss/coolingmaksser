using LibreHardwareMonitor.Hardware;
namespace LovingCoolStudio;
public sealed class SensorService:IDisposable {
 readonly Computer _pc=new(){IsCpuEnabled=true,IsGpuEnabled=true,IsMemoryEnabled=true,IsMotherboardEnabled=true,IsStorageEnabled=true,IsNetworkEnabled=true,IsControllerEnabled=true};
 readonly UpdateVisitor _visitor=new(); public SensorService(){_pc.Open();}
 public Dictionary<string,float> Snapshot(){var d=new Dictionary<string,float>();_pc.Accept(_visitor);foreach(var h in _pc.Hardware)Collect(h,d);return d;}
 void Collect(IHardware h,Dictionary<string,float>d){foreach(var s in h.Sensors)if(s.Value.HasValue)d[$"{h.Name} • {s.SensorType} • {s.Name}"]=s.Value.Value;foreach(var sh in h.SubHardware)Collect(sh,d);}
 public void Dispose()=>_pc.Close();
 sealed class UpdateVisitor:IVisitor{public void VisitComputer(IComputer c)=>c.Traverse(this);public void VisitHardware(IHardware h){h.Update();foreach(var s in h.SubHardware)s.Accept(this);}public void VisitSensor(ISensor s){}public void VisitParameter(IParameter p){}}
}