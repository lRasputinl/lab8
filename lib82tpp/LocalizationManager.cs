using System;
using System.Globalization;
using System.Windows;

namespace lib82tpp
{
    public class LocalizationManager
    {
        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LocalizationManager();
                return _instance;
            }
        }

        public event EventHandler CultureChanged;

        private CultureInfo _currentCulture;
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    CultureInfo.CurrentUICulture = value;
                    CultureChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private LocalizationManager()
        {
            _currentCulture = CultureInfo.CurrentUICulture;
        }

        public void ChangeCulture(string cultureName)
        {
            try
            {
                CurrentCulture = new CultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
                MessageBox.Show($"Culture {cultureName} not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 