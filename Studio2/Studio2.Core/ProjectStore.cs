using System.Text.Json;
namespace LovingCool.Studio2.Core;
public static class ProjectStore{
 static readonly JsonSerializerOptions Opt=new(){WriteIndented=true};public static string Root=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"LOVINGCOOL Studio 2");
 public static Project Load(){try{Directory.CreateDirectory(Root);var f=Path.Combine(Root,"project.json");return File.Exists(f)?JsonSerializer.Deserialize<Project>(File.ReadAllText(f),Opt)??Default():Default();}catch{return Default();}}
 public static void Save(Project p){Directory.CreateDirectory(Root);File.WriteAllText(Path.Combine(Root,"project.json"),JsonSerializer.Serialize(p,Opt));}
 public static Project Default(){var p=new Project();p.Scene.Nodes.AddRange([new(){Kind=NodeKind.Ring,Name="CPU",Text="CPU",X=30,Y=35,Width=180,Height=180,SensorId="CPU Temperature"},new(){Kind=NodeKind.Ring,Name="GPU",Text="GPU",X=270,Y=35,Width=180,Height=180,SensorId="GPU Temperature",Accent="#FFE34DFF"},new(){Kind=NodeKind.Clock,Name="Clock",Text="{time}",X=250,Y=270,Width=200,Height=80,FontSize=62},new(){Kind=NodeKind.SensorText,Name="RAM",Text="RAM  {value}",X=35,Y=285,Width=190,Height=45,FontSize=27,SensorId="Memory Load"}]);return p;}
}