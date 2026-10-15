using Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace KreativStend.ViewModels
{
    public class ErrorManager : INotifyPropertyChanged
    {
        public ObservableCollection<ErrorItem> Errors { get; }
            = new ObservableCollection<ErrorItem>();

        public event PropertyChangedEventHandler? PropertyChanged;

        // Запуск разрешён, если нет активных ошибок тиров 1 и 2,
        // которые оператор не игнорирует.
        public bool CanStartTest =>
            !Errors.Any(e =>
                e.IsActive &&
                e.Tier <= 2 &&
                !e.Ignore);

        // Нужно остановить обкатку, если появилась
        // активная и не игнорируемая ошибка тира 1 или 2.
        public bool ShouldStopTest => !CanStartTest;

        public Brush TopLineColor
        {
            get
            {
                var activeErrors = Errors.Where(e => e.IsActive).ToList();

                if (activeErrors.Any(e => !e.Ignore))
                    return Brushes.Red;

                if (activeErrors.Any())
                    return Brushes.Orange;

                return new SolidColorBrush(Color.FromRgb(0x33, 0x99, 0xFF));
            }
        }

        public void NotifyTopLineColorChanged()
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(TopLineColor)));
        }

        public ErrorManager()
        {
            RegisterErrors();

            Errors.CollectionChanged += (_, _) =>
            {
                SubscribeToErrors();
                NotifyBlockingStateChanged();
            };

            SubscribeToErrors();
        }

        private void RegisterErrors()
        {
            // Уровень 1 — оборудование и подключения
            AddError(101, 1, "Датчик DD1_1: нет связи");
            AddError(102, 1, "Датчик DD1_2: нет связи");
            AddError(103, 1, "Датчик DD2_1: нет связи");
            AddError(104, 1, "Датчик DD2_2: нет связи");
            AddError(105, 1, "Датчик DD_Ger: нет связи");
            AddError(106, 1, "Датчик TM_Work: нет связи");
            AddError(107, 1, "Датчик TM_Clear: нет связи");

            AddError(201, 1, "Нет связи с PLC");
            AddError(202, 1, "Нет связи с мотором по RS485");
            AddError(203, 1, "Нет связи с частотником по RS232");

            AddError(301, 1, "Нет связи с вибродатчиком 1");
            AddError(302, 1, "Нет связи с вибродатчиком 2");

            AddError(401, 1, "Низкий уровень масла №1");
            AddError(402, 1, "Низкий уровень масла №2");

            // Уровень 2 — условия запуска
            AddError(501, 2, "Температура масла в баке №1 вне диапазона");
            AddError(502, 2, "Температура масла в баке №2 вне диапазона");

            // Уровень 3 — ошибки во время обкатки
            AddError(601, 3, "Забит фильтр DD1_1 — DD1_2");
            AddError(602, 3, "Забит фильтр DD2_1 — DD2_2");

            AddError(701, 3, "Ошибка расходомера №1");
            AddError(702, 3, "Ошибка расходомера №2");
        }

        private void AddError(int code, int tier, string message)
        {
            Errors.Add(new ErrorItem
            {
                Code = code,
                Tier = tier,
                Message = message,
                IsActive = false,
                Ignore = false
            });
        }

        private void SubscribeToErrors()
        {
            foreach (var error in Errors)
            {
                error.PropertyChanged -= Error_PropertyChanged;
                error.PropertyChanged += Error_PropertyChanged;
            }
        }

        private void Error_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ErrorItem.IsActive) ||
                e.PropertyName == nameof(ErrorItem.Ignore) ||
                e.PropertyName == nameof(ErrorItem.Tier) ||
                string.IsNullOrEmpty(e.PropertyName))
            {
                NotifyTopLineColorChanged();
                NotifyBlockingStateChanged();
            }
        }

        private void NotifyBlockingStateChanged()
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(CanStartTest)));

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(ShouldStopTest)));
        }

        public void RaiseError(int code)
        {
            var error = Errors.FirstOrDefault(e => e.Code == code);

            if (error == null)
                return;

            // Ошибка остаётся активной, даже если оператор её игнорирует.
            error.IsActive = true;
        }

        public void ClearError(int code)
        {
            var error = Errors.FirstOrDefault(e => e.Code == code);

            if (error != null)
                error.IsActive = false;
        }
    }
}
