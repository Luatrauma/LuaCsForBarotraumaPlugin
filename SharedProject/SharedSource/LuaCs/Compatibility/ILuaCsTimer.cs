using System.Collections.Generic;

namespace Barotrauma.LuaCs.Compatibility;

public partial interface ILuaCsTimer : IReusableService, ILuaCsShim
{
    public static double Time => Timing.TotalTime;
    public static double GetTime() => Time;
    public static double AccumulatorMax
    {
        get => Timing.AccumulatorMax;
        set => Timing.AccumulatorMax = value;
    }

    public void Clear();
    public void Wait(LuaCsAction action, int millisecondDelay);
    public void NextFrame(LuaCsAction action);
}
