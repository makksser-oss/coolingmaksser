using System.Drawing;
namespace LovingCool.Studio2.Device;
public interface ILcdDevice:IDisposable { bool Connected{get;} string? Port{get;} int Width{get;} int Height{get;} void Connect(); void SetBrightness(int value); void Send(Bitmap uprightFrame,int physicalRotation); }
public static class LovingCoolIds { public const int Vid=0x33C3,Pid=0x7791; public const int Baud=2_000_000; }