using System.Runtime.InteropServices.WindowsRuntime;

var sourceBuffer = new Windows.Storage.Streams.Buffer(128000);
sourceBuffer.Length = 20000;
var destBuffer = new Windows.Storage.Streams.Buffer(20000);
destBuffer.Length = 0;

// This will work using .34 but not .38
// Please see the reference in the Project file
sourceBuffer.CopyTo(0, destBuffer, 0, 1000);

