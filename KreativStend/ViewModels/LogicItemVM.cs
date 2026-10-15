namespace KreativStend.ViewModels
{
    public class LogicItemVM : LogicItemBaseVM
    {
        public string Operation { get => Get<string>(); set => Set(value); }
        public string Function { get => Get<string>(); set => Set(value); }
        public string FunctionArgs { get => Get<string>(); set => Set(value); }
        public string Variable { get => Get<string>(); set => Set(value); }
	}

    public class LogicItemBaseVM : WPFHelper.ViewModel
    {
        public bool CM1 { get => Get<bool>(); set => Set(value); }
        public bool CM2 { get => Get<bool>(); set => Set(value); }
        public bool UM1 { get => Get<bool>(); set => Set(value); }
        public bool UM2 { get => Get<bool>(); set => Set(value); }

        public bool Ten { get => Get<bool>(); set => Set(value); }
        public bool M1 { get => Get<bool>(); set => Set(value); }
        public bool M2 { get => Get<bool>(); set => Set(value); }
        public bool M3 { get => Get<bool>(); set => Set(value); }
        public bool K1_M1 { get => Get<bool>(); set => Set(value); }
        public bool K2_M1 { get => Get<bool>(); set => Set(value); }
        public bool K1_M2 { get => Get<bool>(); set => Set(value); }
        public bool K2_M2 { get => Get<bool>(); set => Set(value); }
        public bool K1_M3 { get => Get<bool>(); set => Set(value); }
        public bool K2_M3 { get => Get<bool>(); set => Set(value); }
		public bool Q_1_Work { get => Get<bool>(); set => Set(value); }
		public bool Q_1_Clear { get => Get<bool>(); set => Set(value); }
		public bool Q_2_Work { get => Get<bool>(); set => Set(value); }
		public bool Q_2_Clear { get => Get<bool>(); set => Set(value); }
		public bool Q_3_Work { get => Get<bool>(); set => Set(value); }
		public bool Q_3_Clear { get => Get<bool>(); set => Set(value); }
		public bool Brake_L1 { get => Get<bool>(); set => Set(value); }
        public bool Brake_L2 { get => Get<bool>(); set => Set(value); }
        public bool Brake_R1 { get => Get<bool>(); set => Set(value); }
        public bool Brake_R2 { get => Get<bool>(); set => Set(value); }
        public int MotorSpeed { get => Get<int>(); set => Set(value); }
        public bool Motor_Case_L_Open { get => Get<bool>(); set => Set(value); }
        public bool Motor_Case_R_Open { get => Get<bool>(); set => Set(value); }
        public bool Motor_Case_L_Close { get => Get<bool>(); set => Set(value); }
        public bool Motor_Case_R_Close { get => Get<bool>(); set => Set(value); }
        public bool Ger_On { get => Get<bool>(); set => Set(value); }
        public bool Ger_Off { get => Get<bool>(); set => Set(value); }
        public bool Block_Dif_12 { get => Get<bool>(); set => Set(value); }
        public bool Block_Dif_24 { get => Get<bool>(); set => Set(value); }
        public bool A3 { get => Get<bool>(); set => Set(value); }
        public bool A5 { get => Get<bool>(); set => Set(value); }
		public string Preset { get => Get<string>(); set => Set(value); }
		public string StepMark { get => Get<string>(); set => Set(value); }
	}
}
