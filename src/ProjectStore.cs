using System.Text.Json;
namespace LovingCoolStudio;
public static class ProjectStore {
 static readonly string Dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"LOVINGCOOL Studio");
 static readonly JsonSerializerOptions Json=new(){WriteIndented=true};
 public static StudioProject Load(){try{Directory.CreateDirectory(Dir);var p=Path.Combine(Dir,"project.json");return File.Exists(p)?JsonSerializer.Deserialize<StudioProject>(File.ReadAllText(p),Json)??New():New();}catch{return New();}}
 public static void Save(StudioProject p){Directory.CreateDirectory(Dir);File.WriteAllText(Path.Combine(Dir,"project.json"),JsonSerializer.Serialize(p,Json));}
 public static StudioProject New()=>new(){Widgets=[
  new(){Type=WidgetType.Ring,Name="CPU Temperature",SensorHint="CPU Temperature",Text="CPU",X=34,Y=35,W=175,H=175,Accent="#FF20D8FF"},
  new(){Type=WidgetType.Ring,Name="GPU Temperature",SensorHint="GPU Temperature",Text="GPU",X=270,Y=35,W=175,H=175,Accent="#FFE643FF"},
  new(){Type=WidgetType.Sensor,Name="RAM",SensorHint="Memory Load",Text="RAM  {value}",X=38,Y=255,W=190,H=45,FontSize=26},
  new(){Type=WidgetType.Clock,Name="Clock",Text="{time}",X=245,Y=255,W=200,H=72,FontSize=58},
  new(){Type=WidgetType.Text,Name="Date",Text="{date}",X=282,Y=325,W=160,H=38,FontSize=20}
 ]};
}