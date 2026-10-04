using System.Collections.ObjectModel;using System.ComponentModel;using System.Runtime.CompilerServices;using LovingCool.Studio2.Core;using LovingCool.Studio2.Sensors;
namespace LovingCool.Studio2.App;
public sealed class EditorViewModel:INotifyPropertyChanged,IDisposable{
 public Project Project{get;}=ProjectStore.Load();public ObservableCollection<Node> Nodes{get;}=new();public ObservableCollection<SensorValue> Sensors{get;}=new();readonly LibreSensors provider=new();readonly Stack<string> undo=new(),redo=new();Node? selected;
 public Node? Selected{get=>selected;set{selected=value;Changed();}}
 public EditorViewModel(){foreach(var n in Project.Scene.Nodes)Nodes.Add(n);RefreshSensors();}
 public void Snapshot(){undo.Push(System.Text.Json.JsonSerializer.Serialize(Project.Scene));while(undo.Count>50)undo.TrimBottom();redo.Clear();}
 public void Undo(){if(undo.Count==0)return;redo.Push(System.Text.Json.JsonSerializer.Serialize(Project.Scene));Restore(undo.Pop());}
 public void Redo(){if(redo.Count==0)return;undo.Push(System.Text.Json.JsonSerializer.Serialize(Project.Scene));Restore(redo.Pop());}
 void Restore(string json){var s=System.Text.Json.JsonSerializer.Deserialize<Scene>(json);if(s==null)return;Project.Scene=s;Nodes.Clear();foreach(var n in s.Nodes)Nodes.Add(n);Selected=null;Changed(nameof(Project));}
 public void Add(NodeKind kind){Snapshot();var n=new Node{Kind=kind,Name=kind.ToString(),X=70,Y=70,Width=kind is NodeKind.Ring or NodeKind.Gauge?170:210,Height=kind is NodeKind.Ring or NodeKind.Gauge?170:60,Text=kind switch{NodeKind.Clock=>"{time}",NodeKind.Ring=>"CPU",NodeKind.Gauge=>"GPU",NodeKind.SensorText=>"CPU  {value}",_=>"LOVINGCOOL"}};Project.Scene.Nodes.Add(n);Nodes.Add(n);Selected=n;}
 public void Delete(){if(Selected==null)return;Snapshot();Project.Scene.Nodes.Remove(Selected);Nodes.Remove(Selected);Selected=null;}
 public void Save()=>ProjectStore.Save(Project);
 public void RefreshSensors(){Sensors.Clear();foreach(var s in provider.Snapshot().Where(s=>!s.Name.Contains("Thread",StringComparison.OrdinalIgnoreCase)).OrderBy(s=>s.Hardware).ThenBy(s=>s.Type).ThenBy(s=>s.Name))Sensors.Add(s);}
 public Dictionary<string,(float,string)> Values()=>Sensors.ToDictionary(s=>$"{s.Hardware} {s.Type} {s.Name}",s=>(s.Value,s.Unit),StringComparer.OrdinalIgnoreCase);
 public event PropertyChangedEventHandler? PropertyChanged;void Changed([CallerMemberName]string? n=null)=>PropertyChanged?.Invoke(this,new(n));public void Dispose()=>provider.Dispose();
}
static class StackExt{public static void TrimBottom<T>(this Stack<T> s){var a=s.Reverse().Skip(1).ToArray();s.Clear();foreach(var x in a)s.Push(x);}}