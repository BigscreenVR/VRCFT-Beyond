using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Types;

namespace BeyondExtTrackingInterface
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SharedGazeData
    {
        public float LeftEyeX;
        public float LeftEyeY;
        public float LeftEyeZ;
        public float RightEyeX;
        public float RightEyeY;
        public float RightEyeZ;
        public float CombinedX;
        public float CombinedY;
        public float CombinedZ;
        public float Confidence;
        public long Timestamp;
        public int IsValid;
        public float LeftEyeClosedAmount;
        public float RightEyeClosedAmount;
    }

    public class BeyondExtTrackingModule : ExtTrackingModule
    {
        private MemoryMappedFile? _sharedMem;
        private MemoryMappedViewAccessor? _accessor;
        private const string SharedMemoryName = "VRCFTMemmapData";
        private bool _isInitialized;

        public BeyondExtTrackingModule()
        {
            _isInitialized = false;
        }

        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, false);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            try
            {
                _sharedMem = MemoryMappedFile.OpenExisting(SharedMemoryName);
                _accessor = _sharedMem.CreateViewAccessor(0, Marshal.SizeOf<SharedGazeData>());
                _isInitialized = true;
                ModuleInformation.Name = "Beyond VRCFT Module";
                return (true, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Failed to initialize: {ex.Message}");
                _isInitialized = false;
                return (false, false);
            }
        }

        private Vector2 Vector3ToGazeCoordinates(Vector3 vector)
        {
            // Normalize the vector
            float length = (float)Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
            if (length == 0) return new Vector2(0, 0);
            
            // Project onto XY plane and normalize to [-1, 1] range
            float x = vector.x / length;
            float y = vector.y / length;
            
            return new Vector2(x, y);
        }

        public override void Update()
        {
            if (!_isInitialized || _accessor == null)
            {
                return;
            }

            try
            {
                SharedGazeData data = new SharedGazeData();
                _accessor.Read(0, out data);

                if (data.IsValid == 1)
                {
                    UnifiedTracking.Data.Eye.Left.Openness = 1.0f - data.LeftEyeClosedAmount;
                    UnifiedTracking.Data.Eye.Right.Openness = 1.0f - data.RightEyeClosedAmount;

                    Vector3 leftEye = new Vector3(data.LeftEyeX, data.LeftEyeY, data.LeftEyeZ);
                    Vector3 rightEye = new Vector3(data.RightEyeX, data.RightEyeY, data.RightEyeZ);

                    Vector2 leftGaze = Vector3ToGazeCoordinates(leftEye);
                    Vector2 rightGaze = Vector3ToGazeCoordinates(rightEye);

                    UnifiedTracking.Data.Eye.Left.Gaze = leftGaze;
                    UnifiedTracking.Data.Eye.Right.Gaze = rightGaze;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from shared memory: {ex.Message}");
            }

            // Avoid thrashing CPU
            Thread.Sleep(10);
        }

        public override void Teardown()
        {
            if (_accessor != null)
            {
                _accessor.Dispose();
            }

            if (_sharedMem != null)
            {
                _sharedMem.Dispose();
            }
        }
    }
}
