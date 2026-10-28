using HospitalQuality.Controls;

namespace HospitalQuality.Services
{
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class ToastService
    {
        private readonly ToastHost _toastHost;

        public ToastService(ToastHost toastHost)
        {
            _toastHost = toastHost;
        }

        public void ShowToast(string message, ToastType type = ToastType.Info)
        {
            _toastHost.Show(message, type);
        }
    }
}