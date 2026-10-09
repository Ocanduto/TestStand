using Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace KreativStend.ViewModels
{

    public class ErrorManager
    {
        public ObservableCollection<ErrorItem> Errors { get; }
            = new ObservableCollection<ErrorItem>();

        public ErrorManager()
        {
            RegisterErrors();
        }

        private void RegisterErrors()
        {
            // Тир 1 — оборудование и подключения
            AddError(101, 1, "Датчик DD1_1: давление вне диапазона");
            AddError(102, 1, "Датчик DD1_2: давление вне диапазона");
            AddError(103, 1, "Датчик DD2_1: давление вне диапазона");
            AddError(104, 1, "Датчик DD2_2: давление вне диапазона");
            AddError(105, 1, "Датчик DD_Ger: давление вне диапазона");

            AddError(201, 1, "Нет связи с PLC");
            AddError(202, 1, "Нет связи с мотором по RS485");
            AddError(203, 1, "Нет связи с частотником по RS485");

            AddError(301, 1, "Нет связи с вибродатчиком 1");
            AddError(302, 1, "Нет связи с вибродатчиком 2");

            AddError(401, 1, "Низкий уровень масла №1");
            AddError(402, 1, "Низкий уровень масла №2");

            // Тир 2 — условия запуска
            AddError(501, 2, "Температура масла в баке №1 вне диапазона");
            AddError(502, 2, "Температура масла в баке №2 вне диапазона");

            // Тир 3 — ошибки во время обкатки
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

         public void RaiseError(int code)
        {
            var error = Errors.FirstOrDefault(e => e.Code == code);

            if (error == null || error.Ignore)
                return;

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