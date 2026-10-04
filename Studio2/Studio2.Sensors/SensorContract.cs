namespace LovingCool.Studio2.Sensors;
public sealed record SensorValue(string Id,string Hardware,string Name,string Type,float Value,string Unit);
public interface ISensorProvider:IDisposable { IReadOnlyList<SensorValue> Snapshot(); }