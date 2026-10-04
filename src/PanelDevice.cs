using System.IO.Ports;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;

namespace LovingCoolStudio;
public sealed class PanelDevice : IDisposable {
 SerialPort? _port; readonly object _gate=new(); CancellationTokenSource? _cts;
 public bool Connected => _port?.IsOpen==true; public string PortName=>_port?.PortName??"";
 public int Width{get;private set;}=480; public int Height{get;private set;}=480; public string Firmware{get;private set;}="?";
 static byte[] Frame(byte key, byte[]? payload=null){ payload??=[]; ushort len=(ushort)(payload.Length+7); var b=new List<byte>{0x55,0xAA,(byte)len,(byte)(len>>8),key}; b.AddRange(payload); ushort sum=(ushort)(b.Sum(x=>(int)x)&0xffff); b.Add((byte)sum); b.Add((byte)(sum>>8)); return b.ToArray(); }
 public static string? FindPort(){
  try { using var root=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB\VID_33C3&PID_7791"); if(root!=null) foreach(var id in root.GetSubKeyNames()){ using var dp=root.OpenSubKey(id+@"\Device Parameters"); var p=dp?.GetValue("PortName") as string; if(!string.IsNullOrWhiteSpace(p)) return p; } } catch{}
  return null;
 }
 public void Connect(){ Dispose(); var p=FindPort()??throw new InvalidOperationException("LOVINGCOOL 33C3:7791 не найден. Закройте штатный LOVINGCOOL Monitor и переподключите USB СЖО."); _port=new SerialPort(p,2_000_000){ReadTimeout=600,WriteTimeout=3000,DtrEnable=true}; _port.Open(); lock(_gate){_port.Write(new byte[]{0xFF,0xD9,0xFF,0xD9,0,0,0,0},0,8);} Thread.Sleep(120); ReadInfo(); _cts=new(); _=Task.Run(()=>Heartbeat(_cts.Token)); }
 byte[] Command(byte key, byte[]? payload=null,bool read=true){ if(!Connected)return []; var f=Frame(key,payload); lock(_gate){_port!.DiscardInBuffer(); _port.Write(f,0,f.Length); if(!read)return []; var sw=System.Diagnostics.Stopwatch.StartNew(); var buf=new List<byte>(); while(sw.ElapsedMilliseconds<800){ while(_port.BytesToRead>0) buf.Add((byte)_port.ReadByte()); if(buf.Count>=7&&buf[0]==0x55&&buf[1]==0xAA){int len=buf[2]|buf[3]<<8;if(buf.Count>=len)return buf.Take(len).ToArray();} Thread.Sleep(5);} return buf.ToArray(); } }
 void ReadInfo(){ try{var r=Command(0x06); if(r.Length>7){var json=Encoding.UTF8.GetString(r,5,r.Length-7); using var d=JsonDocument.Parse(json); var x=d.RootElement.TryGetProperty("data",out var data)?data:d.RootElement; if(x.TryGetProperty("width",out var w))Width=w.GetInt32(); if(x.TryGetProperty("height",out var h))Height=h.GetInt32(); if(x.TryGetProperty("version",out var v))Firmware=v.ToString(); }}catch{} }
 async Task Heartbeat(CancellationToken t){while(!t.IsCancellationRequested&&Connected){try{Command(0x11,null,false);}catch{} try{await Task.Delay(1500,t);}catch{}}}
 public void SetBrightness(int value)=>Command(0x03,[(byte)Math.Clamp(value,0,100)],false);
 public void Send(Bitmap bitmap){ if(!Connected)return; using var fitted=new Bitmap(Width,Height,PixelFormat.Format24bppRgb); using(var g=Graphics.FromImage(fitted)){g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;g.DrawImage(bitmap,new Rectangle(0,0,Width,Height));} byte[] jpg=[]; foreach(long q in new[]{88,80,72,64,56,48,40,32}){using var ms=new MemoryStream();var codec=ImageCodecInfo.GetImageEncoders().First(x=>x.FormatID==ImageFormat.Jpeg.Guid);using var ep=new EncoderParameters(1);ep.Param[0]=new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,q);fitted.Save(ms,codec,ep);jpg=ms.ToArray();if(jpg.Length<=32*1024)break;} bool modern=true;if(double.TryParse(Firmware.Replace(',','.'),System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out var ver))modern=ver>=2.8; using var packet=new MemoryStream(); if(modern)packet.Write(BitConverter.GetBytes(jpg.Length));packet.Write(jpg);if(modern){ushort sum=(ushort)(jpg.Sum(x=>(int)x)&0xffff);packet.Write(BitConverter.GetBytes(sum));}var b=packet.ToArray();lock(_gate){_port!.Write(b,0,b.Length);_port.BaseStream.Flush();} }
 public void Dispose(){try{_cts?.Cancel();}catch{} try{_port?.Close();_port?.Dispose();}catch{} _port=null;}
}