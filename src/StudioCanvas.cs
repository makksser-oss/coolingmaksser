using System.Drawing.Drawing2D;
using System.Drawing.Text;
namespace LovingCoolStudio;
public sealed class StudioCanvas:Control {
 public StudioProject Project {get;set;}=new(); public Dictionary<string,float> Sensors {get;set;}=[]; public StudioWidget? Selected {get;private set;}
 public event Action<StudioWidget?>? SelectionChanged; StudioWidget? drag; PointF grab; const float Pad=20;
 public StudioCanvas(){DoubleBuffered=true;BackColor=Color.FromArgb(6,10,17);SetStyle(ControlStyles.ResizeRedraw,true);}
 public Bitmap RenderLogical(){var b=new Bitmap(480,480);using var g=Graphics.FromImage(b);g.SmoothingMode=SmoothingMode.AntiAlias;g.TextRenderingHint=TextRenderingHint.AntiAliasGridFit;
  using(var grad=new LinearGradientBrush(new Rectangle(0,0,480,480),Color.FromArgb(5,14,32),Color.FromArgb(28,7,47),35))g.FillRectangle(grad,0,0,480,480);
  foreach(var w in Project.Widgets.Where(x=>x.Visible))Draw(g,w,false);return b;}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);float z=Math.Min((Width-Pad*2)/480f,(Height-Pad*2)/480f);z=Math.Max(.2f,z);float ox=(Width-480*z)/2,oy=(Height-480*z)/2;
  using var b=RenderLogical();e.Graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;e.Graphics.DrawImage(b,ox,oy,480*z,480*z);
  if(Selected!=null){var r=Selected;using var p=new Pen(Color.FromArgb(40,220,255),2);e.Graphics.DrawRectangle(p,ox+r.X*z,oy+r.Y*z,r.W*z,r.H*z);foreach(var q in Handles(r,ox,oy,z))e.Graphics.FillRectangle(Brushes.White,q);}
 }
 void Draw(Graphics g,StudioWidget w,bool selected){var fg=ColorTranslator.FromHtml(w.Foreground);var ac=ColorTranslator.FromHtml(w.Accent);string value=Value(w);string text=w.Text.Replace("{time}",DateTime.Now.ToString("HH:mm")).Replace("{date}",DateTime.Now.ToString("dd.MM.yyyy")).Replace("{value}",value);
  if(w.Type==WidgetType.Ring){float v=Numeric(w),range=Math.Max(1,w.Max-w.Min),pct=Math.Clamp((v-w.Min)/range,0,1);float d=Math.Min(w.W,w.H);var r=new RectangleF(w.X,w.Y,d,d);using var basePen=new Pen(Color.FromArgb(45,255,255,255),12);using var pen=new Pen(ac,12){StartCap=LineCap.Round,EndCap=LineCap.Round};g.DrawArc(basePen,r,-90,360);g.DrawArc(pen,r,-90,360*pct);using var f1=new Font("Segoe UI",w.FontSize,FontStyle.Bold,GraphicsUnit.Pixel);using var f2=new Font("Segoe UI",15,FontStyle.Bold,GraphicsUnit.Pixel);using var br=new SolidBrush(fg);var vs=g.MeasureString(value,f1);g.DrawString(value,f1,br,w.X+(d-vs.Width)/2,w.Y+d*.39f);var ls=g.MeasureString(w.Text,f2);g.DrawString(w.Text,f2,br,w.X+(d-ls.Width)/2,w.Y+d*.67f);return;}
  if(w.Type==WidgetType.Bar){float pct=Math.Clamp((Numeric(w)-w.Min)/Math.Max(1,w.Max-w.Min),0,1);using var baseBr=new SolidBrush(Color.FromArgb(40,255,255,255));using var acBr=new SolidBrush(ac);g.FillRectangle(baseBr,w.X,w.Y,w.W,w.H);g.FillRectangle(acBr,w.X,w.Y,w.W*pct,w.H);return;}
  using var f=new Font("Segoe UI",w.FontSize,FontStyle.Bold,GraphicsUnit.Pixel);using var br2=new SolidBrush(fg);g.DrawString(text,f,br2,w.X,w.Y);
 }
 float Numeric(StudioWidget w){var k=Resolve(w);return k!=null&&Sensors.TryGetValue(k,out var v)?v:0;} string Value(StudioWidget w){var k=Resolve(w);if(k==null||!Sensors.TryGetValue(k,out var v))return "--";if(k.Contains("Temperature"))return $"{v:0}°C";if(k.Contains("Load"))return $"{v:0}%";if(k.Contains("Clock"))return $"{v:0} MHz";if(k.Contains("Power"))return $"{v:0} W";if(k.Contains("Fan"))return $"{v:0} RPM";return $"{v:0.0}";}
 string? Resolve(StudioWidget w){if(w.SensorKey!=null&&Sensors.ContainsKey(w.SensorKey))return w.SensorKey;if(w.SensorHint==null)return null;var parts=w.SensorHint.Split(' ',2);return Sensors.Keys.FirstOrDefault(k=>parts.All(p=>k.Contains(p,StringComparison.OrdinalIgnoreCase))&&!k.Contains("Thread",StringComparison.OrdinalIgnoreCase))??Sensors.Keys.FirstOrDefault(k=>k.Contains(parts[0],StringComparison.OrdinalIgnoreCase));}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);var p=ToLogical(e.Location);Selected=Project.Widgets.AsEnumerable().Reverse().FirstOrDefault(w=>w.Visible&&!w.Locked&&new RectangleF(w.X,w.Y,w.W,w.H).Contains(p));drag=Selected;if(drag!=null)grab=new(p.X-drag.X,p.Y-drag.Y);SelectionChanged?.Invoke(Selected);Invalidate();}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(e.Button!=MouseButtons.Left||drag==null)return;var p=ToLogical(e.Location);drag.X=Math.Clamp(p.X-grab.X,0,480-drag.W);drag.Y=Math.Clamp(p.Y-grab.Y,0,480-drag.H);Invalidate();}
 protected override void OnMouseUp(MouseEventArgs e){drag=null;base.OnMouseUp(e);}
 PointF ToLogical(Point p){float z=Math.Min((Width-Pad*2)/480f,(Height-Pad*2)/480f);float ox=(Width-480*z)/2,oy=(Height-480*z)/2;return new((p.X-ox)/z,(p.Y-oy)/z);}
 static IEnumerable<RectangleF> Handles(StudioWidget w,float ox,float oy,float z){float s=8;yield return new(ox+w.X*z-s/2,oy+w.Y*z-s/2,s,s);yield return new(ox+(w.X+w.W)*z-s/2,oy+w.Y*z-s/2,s,s);yield return new(ox+w.X*z-s/2,oy+(w.Y+w.H)*z-s/2,s,s);yield return new(ox+(w.X+w.W)*z-s/2,oy+(w.Y+w.H)*z-s/2,s,s);}
}