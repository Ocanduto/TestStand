using KreativStend.Pages;
using KreativStend.Properties;

using NModbus;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Ports;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using WPFHelper.Commands;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KreativStend.ViewModels
{
	public class ProcessStepsVM : WPFHelper.ViewModel
	{
		public ProcessStepsVM()
		{
			_timer = new Timer(On_Tick_Callback, null, 0, 250);

			Messages = [];
			LogicItems = [];

			Variables = new double[10];

            App.ProcessKeys.IsTestRunning = true;
        }

		private Timer _timer;
		private int testId;
		private int SaveStep;


		public double[] Variables { get; set; }

		public int count { get; set; }


		public bool IsConnected { get => Get<bool>(); set { Set(value); } }

		public int StepIndex { get => Get<int>(); set => Set(value); }
		public string TestType { get => Get<string>(); set => Set(value); }
		public string SerialNumberBridge { get => Get<string>(); set => Set(value); }
		public string SerialNumberHydro { get => Get<string>(); set => Set(value); }
		public string SerialNumberValve { get => Get<string>(); set => Set(value); }
		public string BrandNumber { get => Get<string>(); set => Set(value); }

		public int SelectedTypeIndex { get => Get<int>(); set => Set(value); }

		public ObservableCollection<MessageVM> Messages { get; set; }

		public ObservableCollection<LogicItemVM> LogicItems { get; set; }
		public bool HasSuccess { get => Get<bool>(); set => Set(value); }
		public bool HasReplay { get => Get<bool>(); set => Set(value); }

		DateTime SaveDate1 = DateTime.Now;
		DateTime SaveDate2 = DateTime.Now;
		DateTime SaveDate3 = DateTime.Now;
		DateTime SaveDate4 = DateTime.Now;

		double Delta = 0;

		bool OpenDone_L = false;
        bool OpenDone_R = false;
		bool CloseDone_L = false;
        bool CloseDone_R = false;
		bool Message = false;
		int Back = 0;


        bool FirstCicle = true;
		double StartDavlenie = 0;

		private ViewModelCommandHandler Exit_Handler = new();
		private ViewModelCommandHandler Cancel_Handler = new();
		private ViewModelCommandHandler Success_Handler = new();
		private ViewModelCommandHandler Replay_Handler = new();

		public ICommand Exit_Command => Exit_Handler.Get(On_Exit_Command);
		public ICommand Cancel_Command => Cancel_Handler.Get(On_Cancel_Command);
		public ICommand Success_Command => Success_Handler.Get(On_Success_Command, () =>
			!string.IsNullOrEmpty(SerialNumberBridge) &&
			!string.IsNullOrEmpty(SerialNumberHydro) &&
			!string.IsNullOrEmpty(SerialNumberValve) &&
			!string.IsNullOrEmpty(BrandNumber)
		);
		public ICommand Replay_Command => Replay_Handler.Get(On_Replay_Command);

		private void On_Exit_Command()
		{
            App.ProcessKeys.IsTestRunning = false;
			ZeroParams();
            StepIndex = -1;
			App.NavigateHide();
		}
		private async void On_Cancel_Command()
		{


			if (testId > 0 && StepIndex > 0)
			{
				var itemEditor = Messages.OfType<EditorMessageVM>().FirstOrDefault();
				await ApiClient.Instance.Create(new Data.StepModel
				{
					CreateDate = DateTime.UtcNow,
					StepType = StepIndex,
					Result = false,
					TestId = testId,
					Info = itemEditor != null ? $"{itemEditor.Result}{itemEditor.Unit}" : string.Empty
				});
			}

            App.ProcessKeys.IsTestRunning = false;

            StepIndex = 0;
			ZeroParams();

			App.Navigate<NewTestPage>(q => q.DataContext = this);
		}

		private async void On_Error_Command()
		{
			ZeroParams();
			if (testId > 0 && SaveStep > 0)
			{
				var itemEditor = Messages.OfType<EditorMessageVM>().FirstOrDefault();
				await ApiClient.Instance.Create(new Data.StepModel
				{
					CreateDate = DateTime.UtcNow,
					StepType = SaveStep,
					Result = false,
					TestId = testId,
					Info = "Авария"
				});
			}

            App.ProcessKeys.IsTestRunning = false;

            StepIndex = 0;
			App.Navigate<NewTestPage>(q => q.DataContext = this);
		}

		private async void On_Success_Command()
		{

			//        if (StepIndex == 0)
			//        {
			//testId = await ApiClient.Instance.Create(new Data.TestModel
			//            {
			//                CreateDate = DateTime.UtcNow,
			//                SerialNumberBridge = SerialNumberBridge,
			//                SerialNumberHydro = SerialNumberHydro,
			//                SerialNumberValve = SerialNumberValve,
			//                BrandNumber = BrandNumber,
			//                TestType = TestType,
			//                Operator = " ",
			//            });
			//        }

			//        if (testId > 0 && StepIndex > 0)
			//        {
			//            var itemEditor = Messages.OfType<EditorMessageVM>().FirstOrDefault();
			//            await ApiClient.Instance.Create(new Data.StepModel
			//            {
			//                CreateDate = DateTime.UtcNow,
			//                StepType = StepIndex,
			//                Result = true,
			//                TestId = testId,
			//                Info = itemEditor != null ? $"{itemEditor.Result}{itemEditor.Unit}" : string.Empty
			//            });
			//        }


			//        if (StepIndex == 0)
			//        {
			//            LogicItems[0];
			//        }

			++StepIndex;
			Navigator();
		}
		private async void On_Replay_Command()
		{
			if (testId > 0 && StepIndex > 0)
			{
				var itemEditor = Messages.OfType<EditorMessageVM>().FirstOrDefault();
				await ApiClient.Instance.Create(new Data.StepModel
				{
					CreateDate = DateTime.UtcNow,
					StepType = StepIndex,
					Result = true,
					TestId = testId,
					Info = itemEditor != null ? $"{itemEditor.Result}{itemEditor.Unit}" : string.Empty
				});
			}

			Navigator();
		}

		private async void On_Tick_Callback(object? state)
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);

			//if(App.ProcessKeys.ERROR && StepIndex > 0)
			//{
			//    SaveStep = StepIndex;
			//    StepIndex = -3;

			//    App.Current.Dispatcher.Invoke(() =>
			//    {
			//        Navigator(true);
			//    });
			//}

			try
			{
                if (App.ProcessKeys.ErrorManager.ShouldStopTest)
                {
					On_Error_Command();
                }
                if (StepIndex > 0)
				{
					App.ProcessKeys.Pump1_Work = false;
                    App.ProcessKeys.Pump2_Work = false;
                    App.ProcessKeys.Pump3_Work = false;
                    App.ProcessKeys.Pump1_Clear = false;
                    App.ProcessKeys.Pump2_Clear = false;
                    App.ProcessKeys.Pump3_Clear = false;
                    App.ProcessKeys.M1 = LogicItems[StepIndex - 1].M1;
					App.ProcessKeys.M2 = LogicItems[StepIndex - 1].M2;
					App.ProcessKeys.M3 = LogicItems[StepIndex - 1].M3;

					App.ProcessKeys.K1_M1 = LogicItems[StepIndex - 1].K1_M1;
					App.ProcessKeys.K2_M1 = LogicItems[StepIndex - 1].K2_M1;
					App.ProcessKeys.K1_M2 = LogicItems[StepIndex - 1].K1_M2;
					App.ProcessKeys.K2_M2 = LogicItems[StepIndex - 1].K2_M2;
					App.ProcessKeys.K1_M3 = LogicItems[StepIndex - 1].K1_M3;
					App.ProcessKeys.K2_M3 = LogicItems[StepIndex - 1].K2_M3;
					App.ProcessKeys.Q_1_Work = LogicItems[StepIndex - 1].Q_1_Work;
					App.ProcessKeys.Q_1_Clear = LogicItems[StepIndex - 1].Q_1_Clear;
					App.ProcessKeys.Q_2_Work = LogicItems[StepIndex - 1].Q_2_Work;
					App.ProcessKeys.Q_2_Clear = LogicItems[StepIndex - 1].Q_2_Clear;
					App.ProcessKeys.Q_3_Work = LogicItems[StepIndex - 1].Q_3_Work;
					App.ProcessKeys.Q_3_Clear = LogicItems[StepIndex - 1].Q_3_Clear;
					App.ProcessKeys.Ger_On = LogicItems[StepIndex - 1].Ger_On;
					App.ProcessKeys.Ger_Off = LogicItems[StepIndex - 1].Ger_Off;
					App.ProcessKeys.Block_Dif_12 = LogicItems[StepIndex - 1].Block_Dif_12;
					App.ProcessKeys.Block_Dif_24 = LogicItems[StepIndex - 1].Block_Dif_24;
					App.ProcessKeys.Brake_L1 = LogicItems[StepIndex - 1].Brake_L1;
					App.ProcessKeys.Brake_L2 = LogicItems[StepIndex - 1].Brake_L2;
					App.ProcessKeys.Brake_R1 = LogicItems[StepIndex - 1].Brake_R1;
					App.ProcessKeys.Brake_R2 = LogicItems[StepIndex - 1].Brake_R2;
					if(LogicItems[StepIndex - 1].Function != "DD_Ger")
					{
						App.ProcessKeys.A3 = LogicItems[StepIndex - 1].A3;
						App.ProcessKeys.A5 = LogicItems[StepIndex - 1].A5;
					}
                    //App.ProcessKeys.Brake = LogicItems[StepIndex - 1].Brake;
                    if (LogicItems[StepIndex - 1].MotorSpeed != 0)
                    {
                        App.ProcessKeys.MotorSpeed = LogicItems[StepIndex - 1].MotorSpeed;
                        App.ProcessKeys.StartMotor = true;
                        App.ProcessKeys.StopMotor = false;
                    }
                    else
                    {
                        App.ProcessKeys.StartMotor = false;
                        App.ProcessKeys.StopMotor = true;
                    }
                    if (LogicItems[StepIndex - 1].Variable != "Kozuh close" && LogicItems[StepIndex - 1].Variable != "Kozuh open")
					{
						App.ProcessKeys.Motor_Case_L_Open = LogicItems[StepIndex - 1].Motor_Case_L_Open;
						App.ProcessKeys.Motor_Case_L_Close = LogicItems[StepIndex - 1].Motor_Case_L_Close;
						App.ProcessKeys.Motor_Case_R_Open = LogicItems[StepIndex - 1].Motor_Case_R_Open;
						App.ProcessKeys.Motor_Case_R_Close = LogicItems[StepIndex - 1].Motor_Case_R_Close;
						OpenDone_L = false;
						OpenDone_R = false;
						CloseDone_L = false;
						CloseDone_R = false;
					}
					else
					{
						if (LogicItems[StepIndex - 1].Variable == "Kozuh close")
						{
							if (!App.ProcessKeys.Case_L_Close && !CloseDone_L)
							{
								App.ProcessKeys.Motor_Case_L_Close = LogicItems[StepIndex - 1].Motor_Case_L_Close;
							}
							else
							{
								CloseDone_L = true;
								App.ProcessKeys.Motor_Case_L_Close = false;
							}
							if (!App.ProcessKeys.Case_R_Close && !CloseDone_R)
							{
								App.ProcessKeys.Motor_Case_R_Close = LogicItems[StepIndex - 1].Motor_Case_R_Close;
							}
							else
							{
								CloseDone_R = true;
								App.ProcessKeys.Motor_Case_R_Close = false;
							}
						}
						if (LogicItems[StepIndex - 1].Variable == "Kozuh open")
						{
							if (!App.ProcessKeys.Case_L_Open && !OpenDone_L)
							{
								App.ProcessKeys.Motor_Case_L_Open = LogicItems[StepIndex - 1].Motor_Case_L_Open;
							}
							else
							{
								OpenDone_L = true;
								App.ProcessKeys.Motor_Case_L_Open = false;
							}
							if (!App.ProcessKeys.Case_R_Open && !OpenDone_R)
							{
								App.ProcessKeys.Motor_Case_R_Open = LogicItems[StepIndex - 1].Motor_Case_R_Open;
							}
							else
							{
								OpenDone_R = true;
								App.ProcessKeys.Motor_Case_R_Open = false;
							}
						}
					}

					//if (FirstCicle)
					//{
					//	FirstCicle = false;
					//	SaveDate1 = DateTime.Now;
					//	using (StreamWriter sw = new StreamWriter("TM1.txt", true)) // true = append
					//	{
					//		sw.WriteLine("");
					//	}
					//	SaveDate2 = DateTime.Now;
					//	using (StreamWriter sw = new StreamWriter("TM2.txt", true)) // true = append
					//	{
					//		sw.WriteLine("");
					//	}
					//	SaveDate3 = DateTime.Now;
					//	using (StreamWriter sw = new StreamWriter("DD1_1.txt", true)) // true = append
					//	{
					//		sw.WriteLine("");
					//	}
					//	SaveDate4 = DateTime.Now;
					//	using (StreamWriter sw = new StreamWriter("DD1_2.txt", true)) // true = append
					//	{
					//		sw.WriteLine("");
					//	}
					//}

					//var DateNow = DateTime.Now;

					//if ((DateNow - SaveDate1).TotalSeconds > 0.5)
					//{
					//	using (StreamWriter sw = new StreamWriter("TM1.txt", true)) // true = append
					//	{
					//		sw.WriteLine("24 C " + DateNow.ToString("dd.MM.yyyy HH:mm:ss.fff"));
					//	}
					//	SaveDate1 = DateTime.Now;
					//}

					//if ((DateNow - SaveDate2).TotalSeconds > 1)
					//{
					//	using (StreamWriter sw = new StreamWriter("TM2.txt", true)) // true = append
					//	{
					//		sw.WriteLine("15 C " + DateNow.ToString("dd.MM.yyyy HH:mm:ss.fff"));
					//	}
					//	SaveDate2 = DateTime.Now;
					//}

					//if ((DateNow - SaveDate3).TotalSeconds > 1.5)
					//{
					//	using (StreamWriter sw = new StreamWriter("DD1_1.txt", true)) // true = append
					//	{
					//		sw.WriteLine("1.4 Pa " + DateNow.ToString("dd.MM.yyyy HH:mm:ss.fff"));
					//	}
					//	SaveDate3 = DateTime.Now;
					//}

					//if ((DateNow - SaveDate4).TotalSeconds > 2)
					//{
					//	using (StreamWriter sw = new StreamWriter("DD1_2.txt", true)) // true = append
					//	{
					//		sw.WriteLine("0.6 Pa " + DateNow.ToString("dd.MM.yyyy HH:mm:ss.fff"));
					//	}
					//	SaveDate4 = DateTime.Now;
					//}



					switch (LogicItems[StepIndex - 1].Function)
					{
						default:

							break;
						case "Delay":
							if(!Message)
							{

								var startTime = DateTime.Now;
								var endTime = DateTime.Now + TimeSpan.FromSeconds(Convert.ToDouble(LogicItems[StepIndex - 1].FunctionArgs));
								while (startTime < endTime && !App.ProcessKeys.ERROR && !App.ProcessKeys.ErrorManager.ShouldStopTest)
								{
									await Task.Delay(250);
									startTime = DateTime.Now;
								}

								StepIndex++;
								App.Current.Dispatcher.Invoke(() =>
								{
									Navigator(true);
								});
							}
							else
							{
								Message = false;
							}
								break;
						case "Message":
							Message = true;
							break;
                        case "CheckGer":
                            Message = true;
                            break;
                        case "Editor":

							break;
						case "End":

							break;
						case "Counter": //ПЕРЕДЕЛАТЬ
                            if (!Message)
                            {
                                var Coefs = GetCoefChank(Settings.Default.Coefs).ToList();
                                var Target = Math.Round(Coefs.FirstOrDefault(q => q.Index == Convert.ToInt32(LogicItems[StepIndex - 1].Variable)).Coef * Convert.ToDouble(LogicItems[StepIndex - 1].FunctionArgs));

                                if (App.ProcessKeys.Counters[Convert.ToInt32(LogicItems[StepIndex - 1].Variable)] >= Target)
                                {
                                    StepIndex++;
                                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        Navigator(true);
                                    });
                                }
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Navigator(true);
                                });
                            }
                            else
                            {
                                Message = false;
                            }
                            break;
                        case "Delta":
							if(!Message)
							{

								var startTime = DateTime.Now;
								var endTime = DateTime.Now + TimeSpan.FromSeconds(Convert.ToDouble(LogicItems[StepIndex - 1].FunctionArgs));
								StartDavlenie = App.ProcessKeys.DD_Ger;
								while (startTime < endTime && !App.ProcessKeys.ERROR && !App.ProcessKeys.ErrorManager.ShouldStopTest)
								{
									await Task.Delay(250);
									startTime = DateTime.Now;
									Delta = StartDavlenie - App.ProcessKeys.DD_Ger;
									App.Current.Dispatcher.Invoke(() =>
									{
										Navigator(true);
									});
								}
                                StepIndex++;
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Navigator(true);
                                });
                            }
							else
							{
								Message = false;
							}
								break;
						case "DD_Ger":
							//startTime = DateTime.Now;
							//endTime = DateTime.Now + TimeSpan.FromSeconds(Convert.ToDouble(LogicItems[StepIndex - 1].FunctionArgs));
							//var StartDavlenie = App.ProcessKeys.DD_Ger;
							await Task.Delay(500);
							if(!App.ProcessKeys.L1)
							{
								StepIndex++;
								App.Current.Dispatcher.Invoke(() =>
								{
									Navigator(true);
								});
							}
							//Delta = Convert.ToInt32(StartDavlenie) - Convert.ToInt32(App.ProcessKeys.DD_Ger);
							break;
						case "Go Back":
							if(Back < Convert.ToInt32(LogicItems[StepIndex - 1].Variable))
							{
								Back++;
								StepIndex = StepIndex - Convert.ToInt32(LogicItems[StepIndex - 1].FunctionArgs);
								App.Current.Dispatcher.Invoke(() =>
								{
									Navigator(true);
								});
							}
							else
							{
								StepIndex++;
								Back = 0;
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Navigator(true);
                                });
                            }
							break;
						case "Pump Out":
									if (!Message)
									{

										await Task.Delay(5000);
										while (App.ProcessKeys.DD2_1 < Convert.ToInt32(LogicItems[StepIndex - 1].FunctionArgs) && !App.ProcessKeys.ERROR && !App.ProcessKeys.ErrorManager.ShouldStopTest)
										{
											await Task.Delay(250);
											App.Current.Dispatcher.Invoke(() =>
											{
												Navigator(true);
											});
										}
										StepIndex++;
										App.Current.Dispatcher.Invoke(() =>
										{
											Navigator(true);
										});
									}
									else
									{
										Message = false;
									}
									break;
								case "Wait Signal":
									if (!Message)
									{
										//startTime = DateTime.Now;
										//endTime = DateTime.Now + TimeSpan.FromSeconds(Convert.ToDouble(LogicItems[StepIndex - 1].FunctionArgs));
										switch (LogicItems[StepIndex - 1].Variable)
										{
											case "L1":
												if (!App.ProcessKeys.L1)
												{
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "L2":
												if (!App.ProcessKeys.L2)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "R1":
												if (!App.ProcessKeys.R1)
												{
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "R2":
												if (!App.ProcessKeys.R2)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Kozuh close":
												if (CloseDone_R && CloseDone_L)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Kozuh open":
												if (OpenDone_L && OpenDone_R)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Tormoz All":
												if (App.ProcessKeys.Brake_L1_Off && App.ProcessKeys.Brake_L2_Off && App.ProcessKeys.Brake_R1_Off && App.ProcessKeys.Brake_R2_Off)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Tormoz L1":
												if (App.ProcessKeys.Brake_L1_Off)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Tormoz L2":
												if (App.ProcessKeys.Brake_L2_Off)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Tormoz R1":
												if (App.ProcessKeys.Brake_R1_Off)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Tormoz R2":
												if (App.ProcessKeys.Brake_R2_Off)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Tormoz ne L1":
												if (!App.ProcessKeys.Brake_L1_Off)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Tormoz ne L2":
												if (!App.ProcessKeys.Brake_L2_Off)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Tormoz ne R1":
												if (!App.ProcessKeys.Brake_R1_Off)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											case "Tormoz ne R2":
												if (!App.ProcessKeys.Brake_R2_Off)
												{
													//App.ProcessKeys.Brake = true;
													StepIndex++;
													App.Current.Dispatcher.Invoke(() =>
													{
														Navigator(true);
													});
												}
												break;
											default:
												break;
										}

									}
									else
									{
										Message = false;
									}
									break;
								}
				}
				
			}catch(Exception ex)
			{ }

			_timer.Change(0, 250);
		}

		private void ZeroParams()
		{
            App.ProcessKeys.M1 = false; ;
            App.ProcessKeys.M2 = false; ;
            App.ProcessKeys.M3 = false;

            App.ProcessKeys.K1_M1 = false;
            App.ProcessKeys.K2_M1 = false;
            App.ProcessKeys.K1_M2 = false;
            App.ProcessKeys.K2_M2 = false;
            App.ProcessKeys.K1_M3 = false;
            App.ProcessKeys.K2_M3 = false;
            App.ProcessKeys.Q_1_Work = false;
            App.ProcessKeys.Q_1_Clear = false;
            App.ProcessKeys.Q_2_Work = false;
            App.ProcessKeys.Q_2_Clear = false;
            App.ProcessKeys.Q_3_Work = false;
            App.ProcessKeys.Q_3_Clear = false;
            App.ProcessKeys.Ger_On = false;
            App.ProcessKeys.Ger_Off = false;
            App.ProcessKeys.Block_Dif_12 = false;
            App.ProcessKeys.Block_Dif_24 = false;
            App.ProcessKeys.Brake_L1 = false;
            App.ProcessKeys.Brake_L2 = false;
            App.ProcessKeys.Brake_R1 = false;
            App.ProcessKeys.Brake_R2 = false;
            App.ProcessKeys.MotorTest = false;
            App.ProcessKeys.MotorTest2 = false;
            App.ProcessKeys.Motor_Case_L_Close = false;
            App.ProcessKeys.Motor_Case_L_Open = false;
            App.ProcessKeys.Motor_Case_R_Close = false;
            App.ProcessKeys.Motor_Case_R_Open = false;
			App.ProcessKeys.StopMotor = true;
            App.ProcessKeys.StartMotor = true;
        }

		private void Navigator(bool noNav = false)
		{
			Messages.Clear();
			HasReplay = false;
			HasSuccess = false;

			if (StepIndex > 0)
			{
				Messages.Add(new MessageVM { Message = LogicItems[StepIndex - 1].Operation, Type = MessageTypeEnum.Header });
			}
			else
			{
				return;
			}

			switch (LogicItems[StepIndex - 1].Function)
			{
				default:
					HasSuccess = true;
					break;
				case "Delay":
					if(LogicItems[StepIndex - 1].Variable == "DD")
					{
                        Messages.Add(new EditorMessageVM { Message = $"Давление в системе откачки:{App.ProcessKeys.DD2_2})", Foreground = "Green", Type = MessageTypeEnum.Message });
                    }
                    break;
                case "Wait Signal":
                    break;
                case "DD_Ger":
                    break;
                case "CheckGer":
					if(Convert.ToInt32(LogicItems[StepIndex - 1].Variable) > Delta)
					{
                        Messages.Add(new EditorMessageVM { Message = $"Давление в системе упало на {Delta}. Было {StartDavlenie})", Foreground = "Green", Type = MessageTypeEnum.Message });
					}
					else
					{
                        Messages.Add(new EditorMessageVM { Message = $"Давление в системе упало на {Delta}. Было {StartDavlenie})", Foreground = "Red", Type = MessageTypeEnum.Message });
                    }
						HasSuccess = true;
					break;
				case "Editor":
					Messages.Add(new EditorMessageVM { Message = $"Показания шумомера: ", Unit = LogicItems[StepIndex - 1].FunctionArgs, Type = MessageTypeEnum.Editor });
					HasSuccess = true;
					break;
				case "End":
					On_Exit_Command();
					break;
                case "Delta":
                    string result = string.Format("{0:0.00}", Delta);
                    Messages.Add(new MessageVM { Message = $"Падение давления:{result}", Type = MessageTypeEnum.Message });
                    break;
                case "Counter":
					var Coefs = GetCoefChank(Settings.Default.Coefs).ToList();
					var Target = Convert.ToDouble(LogicItems[StepIndex - 1].FunctionArgs);
					var Current = App.ProcessKeys.Counters[Convert.ToInt32(LogicItems[StepIndex - 1].Variable)] 
						/ Coefs.FirstOrDefault(q => q.Index == Convert.ToInt32(LogicItems[StepIndex - 1].Variable)).Coef;
					Messages.Add(new MessageVM { Message = $"Залито {Current:f2} из {Target} литров" , Type = MessageTypeEnum.Message });
					break;
                case "Pump Out":
                    Messages.Add(new MessageVM { Message = $"Давление в системе {App.ProcessKeys.DD2_1}.		Ждем {LogicItems[StepIndex - 1].FunctionArgs}", Type = MessageTypeEnum.Message });
                    break;
            }

			//switch (StepIndex)
			//{
			//    default:
			//        On_Cancel_Command();
			//        return;
			//    case -3:
			//        On_Error_Command();
			//        return;
			//    case -1:
			//        On_Exit_Command();
			//        return;

			//    case 1:
			//        Messages.Add(new MessageVM { Message = $"УБЕДИТЕСЬ В ПОДКЛЮЧЕНИИ{Environment.NewLine} ВСЕХ НЕОБХОДИМЫХ{Environment.NewLine} КОММУНИКАЦИЙ", Type = MessageTypeEnum.Message });
			//        StepIndex = 11;
			//        HasSuccess = true;
			//        break;
			//    case 12:
			//        Messages.Add(new MessageVM { Message = $"ШАГ 1: Проверка движения Вперед/Назад", Type = MessageTypeEnum.Header });
			//        Messages.Add(new MessageVM { Message = $"Запустить гидромоторы на движение \"Вперед\" в течении 1 мин. при расходе жидкости 95-114 л/мин", Type = MessageTypeEnum.Informer });
			//        Messages.Add(new MessageVM { Message = $"Запустить гидромоторы на движение \"Назад\" в течении 1 мин. при расходе жидкости 95-114 л/мин", Type = MessageTypeEnum.Informer });
			//        Messages.Add(new EditorMessageVM { Message = $"Показания шумомера: ", Unit = "dB", Type = MessageTypeEnum.Editor });
			//        Messages.Add(new MessageVM { Message = $"ПРОВЕРИТЬ ПРАВИЛЬНОСТЬ НАПРАВЛЕНИЯ ВРАЩЕНИЯ ГИДРОМОТОРОВ", Type = MessageTypeEnum.Message });
			//        Messages.Add(new MessageVM { Message = $"ОСУЩЕСТВИТЬ ЗАМЕР ШУМА", Type = MessageTypeEnum.Message });
			//        break;

			//    case 3:
			//        Messages.Add(new MessageVM { Message = $"УСТАНОВИТЬ УПОРЫ", Type = MessageTypeEnum.Message });
			//        Messages.Add(new SelectorMessageVM { Commands = ["Без нагрузки", "С нагрузкой"], Type = MessageTypeEnum.Selector });
			//        StepIndex = 30;
			//        HasSuccess = true;
			//        break;
			//    case 31:
			//        Messages.Add(new MessageVM { Message = $"ШАГ 3: Проверка герметичности", Type = MessageTypeEnum.Header });
			//        Messages.Add(new MessageVM { Message = $"Происходит проверка", Type = MessageTypeEnum.Message });
			//        Messages.Add(new MessageVM { Message = $"ОСТОРОЖНО ВЫСОКОЕ ДАВЛЕНИЕ", Type = MessageTypeEnum.Warning });
			//        break;
			//    case 32:
			//        StepIndex = 3;
			//        Messages.Add(new MessageVM { Message = $"ШАГ 3: Проверка герметичности", Type = MessageTypeEnum.Header });
			//        Messages.Add(new MessageVM { Message = $"Проверить подтекание рабочей жидкости", Type = MessageTypeEnum.Message });
			//        HasSuccess = true;
			//        HasReplay = true;
			//        break;

			//    case 2:
			//        Messages.Add(new MessageVM { Message = $"УБРАТЬ УПОРЫ", Type = MessageTypeEnum.Message });
			//        StepIndex = 20;
			//        HasSuccess = true;
			//        break;
			//    case 21:
			//        Messages.Add(new MessageVM { Message = $"ШАГ 2: Проверка поворотных механизмов", Type = MessageTypeEnum.Header });
			//        Messages.Add(new MessageVM { Message = $"Подача поочередно 190бар на правую и левую часть гидроцилиндра, выдержка 10 сек", Type = MessageTypeEnum.Message });
			//        Messages.Add(new TwoColumnMessageVM { Left = $"Левый: 0/10", Right = $"Правый: 0/10" });
			//        StepIndex = 22;
			//        break;
			//    case 22:
			//        StepIndex = 2;
			//        Messages.Add(new MessageVM { Message = $"ШАГ 2: Проверка поворотных механизмов", Type = MessageTypeEnum.Header });
			//        Messages.Add(new MessageVM { Message = $"Подтвердить переход на следующий шаг", Type = MessageTypeEnum.Message });
			//        HasSuccess = true;
			//        HasReplay = true;
			//        break;

			//    case 4:
			//        Messages.Add(new MessageVM { Message = $"ШАГ 4: Откачка рабочей жидкости и продувка", Type = MessageTypeEnum.Header });
			//        Messages.Add(new MessageVM { Message = $"Откачка рабочей жидкости и продувка цилиндров", Type = MessageTypeEnum.Informer });
			//        StepIndex = 40;
			//        break;

			//    case 5:
			//        Messages.Add(new MessageVM { Message = $"ОТСОЕДИНИТЕ КОММУНИКАЦИИ{Environment.NewLine} ВЕРНИТЕ В ДОМАШНЮЮ ПОЗИЦИЮ", Type = MessageTypeEnum.Message });
			//        HasSuccess = true;
			//        break;

			//    case 6:
			//        Messages.Add(new MessageVM { Message = $"УСТАНОВИТЬ МАРКИРАТОР{Environment.NewLine} НА МЕСТО МАРКИРОВКИ{Environment.NewLine} ПРОИЗВЕСТИ МАРКИРАЦИЮ{Environment.NewLine} НАЖАТИЕМ КУРКА УСТРОЙСТВА", Type = MessageTypeEnum.Message });
			//        break;
			//    case 7:
			//        Messages.Add(new MessageVM { Message = $"УСТАНОВИТЬ МАРКИРАТОР{Environment.NewLine} НА МЕСТО МАРКИРОВКИ{Environment.NewLine} ПРОИЗВЕСТИ МАРКИРАЦИЮ{Environment.NewLine} НАЖАТИЕМ КУРКА УСТРОЙСТВА", Type = MessageTypeEnum.Message });
			//        break;
			//}
			if (!noNav)
				App.Navigate<ProcessPage>(q => q.DataContext = this);
		}

		private static IEnumerable<CoefChank> GetCoefChank(StringCollection strings)
		{
			if (strings == null) yield break;

			foreach (var CoefChankStr in strings)
			{
				var arr = CoefChankStr.Split(':');
				if (arr.Length == 2)
					yield return new CoefChank
					{
						Index = Convert.ToInt32(arr[0]),
						Coef = Convert.ToDouble(arr[1])
					};
			}
		}
	}

	public class MessageVM : WPFHelper.ViewModel
	{
		public string Message { get => Get<string>(); set => Set(value); }
		public StateMessageEnum State { get => Get<StateMessageEnum>(); set => Set(value); }
		public MessageTypeEnum Type { get => Get<MessageTypeEnum>(); set => Set(value); }
        public string Foreground { get => Get<string>(); set => Set(value); }
    }

	public class EditorMessageVM : MessageVM
	{
		public EditorMessageVM()
		{
			Type = MessageTypeEnum.Editor;
		}

		public string Unit { get => Get<string>(); set => Set(value); }
		public string Result { get => Get<string>(); set => Set(value); }
	}

	public class TwoColumnMessageVM : MessageVM
	{
		public TwoColumnMessageVM()
		{
			Type = MessageTypeEnum.TwoColumn;
		}
		public string Left { get => Get<string>(); set => Set(value); }
		public string Right { get => Get<string>(); set => Set(value); }
	}
	public class SelectorMessageVM : MessageVM
	{
		public SelectorMessageVM()
		{
			Type = MessageTypeEnum.Selector;
			Commands = [];
		}
		public ObservableCollection<string> Commands { get; set; }
	}

	public enum MessageTypeEnum
	{
		Header,
		Informer,
		Message,
		Warning,
		Editor,
		TwoColumn,
		Selector
	}

	public enum StateMessageEnum
	{
		Wait,
		Success,
		Error
	}

    public enum ForegroundEnum
    {
        Black,
		Red,
		Green
    }

    public static class ASCIIChars
	{
		public const char SOH = '\u0001';
		public const char STX = '\u0002';
		public const char ETX = '\u0003';
		public const char EOT = '\u0004';
		public const char ENQ = '\u0005';
		public const char ETB = '\u0023';
		public const char CR = '\u000D';
		public const char FF = '\u000C';
		private static char[] chars =
		[
			'\u002e',
		'\u0030',
		'\u0031',
		'\u0032',
		'\u0033',
		'\u0034',
		'\u0035',
		'\u0036',
		'\u0037',
		'\u0038',
		'\u0039',
		'\u0020',
		'\u0053'
		];
		public static string GetText(this string text)
		{
			var b = new StringBuilder();
			foreach (char c in text)
			{
				switch (c)
				{
					default: continue;
					case '.': b.Append(chars[0]); continue;
					case '0': b.Append(chars[1]); continue;
					case '1': b.Append(chars[2]); continue;
					case '2': b.Append(chars[3]); continue;
					case '3': b.Append(chars[4]); continue;
					case '4': b.Append(chars[5]); continue;
					case '5': b.Append(chars[6]); continue;
					case '6': b.Append(chars[7]); continue;
					case '7': b.Append(chars[8]); continue;
					case '8': b.Append(chars[9]); continue;
					case '9': b.Append(chars[10]); continue;
					case ' ': b.Append(chars[11]); continue;
					case 'S': b.Append(chars[12]); continue;
				}
			}
			return b.ToString();
		}
		public static string GetCommand(this string text)
		{
			return $"{STX}{text.GetText()}{ETX}";
		}
	}

	public class CoefChank
	{
		public int Index { get; set; }
		public double Coef { get; set; }
	}
}
