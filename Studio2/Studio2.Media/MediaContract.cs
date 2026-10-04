using System.Drawing;
namespace LovingCool.Studio2.Media;
public interface IFrameSource:IDisposable { Size NativeSize{get;} double DurationSeconds{get;} Bitmap GetFrame(double seconds); }
public interface IScreenCapture { IReadOnlyList<string> Displays(); Bitmap Capture(string displayId,Rectangle crop); }