namespace Wpf.Libaries.ServerService.Base
{
    public interface ITcpTaskTimer
    {
        void DisposeTimer();
        bool GetTimerEnable();
        double GetTimerInterval();
        void InitTimer();
        void SetTimerEnable(bool value);
        void SetTimerInterval(int time = 1000);
        void SetTimerStart();
        void SetTimerStop();
    }
}