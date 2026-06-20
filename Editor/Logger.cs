#if UNITY_EDITOR

using UnityEngine;

namespace VRCBPIDDuplicationDetector
{
  public class Logger
  {
    const string LOG_PREFIX = "[VRCBPIDDuplicationDetector] ";

    public static void Log(string message)
    {
      Debug.Log(LOG_PREFIX + message);
    }
  }
}


#endif
