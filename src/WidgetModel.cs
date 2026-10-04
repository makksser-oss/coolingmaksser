namespace LovingCoolStudio;
public enum WidgetType { Text, Clock, Sensor, Ring, Bar, Graph, Image }
public sealed class StudioWidget {
 public Guid Id {get;set;}=Guid.NewGuid(); public WidgetType Type {get;set;}=WidgetType.Text; public string Name {get;set;}="Элемент"; public string Text {get;set;}="";
 public float X {get;set;}=40; public float Y {get;set;}=40; public float W {get;set;}=160; public float H {get;set;}=50; public float FontSize {get;set;}=26;
 public string Foreground {get;set;}="#FFFFFFFF"; public string Accent {get;set;}="#FF20D8FF"; public string? SensorKey {get;set;} public string? SensorHint {get;set;}
 public float Min {get;set;}=0; public float Max {get;set;}=100; public bool Visible {get;set;}=true; public bool Locked {get;set;}=false; public string? AssetPath {get;set;}
}
public sealed class StudioProject {
 public string Name {get;set;}="Мой экран"; public int Rotation {get;set;}=90; public int Brightness {get;set;}=80; public string? BackgroundPath {get;set;}
 public List<StudioWidget> Widgets {get;set;}=[];
}