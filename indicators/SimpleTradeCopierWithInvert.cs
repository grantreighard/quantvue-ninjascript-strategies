#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Prop_Trader_Tools
{
	public class SimpleTradeCopierWithInvert : Indicator
	{
		
		private Chart chartWindow;
		
		private bool invertButtonClicked;
		private System.Windows.Controls.Button invertButton;
		private bool copyButtonClicked;
		private System.Windows.Controls.Button copyButton;
		private System.Windows.Controls.Grid myGrid;
		
		private bool IsCopyAllowed = false;
		private bool ShouldInvertTrades = false;
		private bool ShouldInvertTradesOnChop = false;
		
		//=== Accounts 10
		private Account MasterAccount = null;
		private Account Acc1= null;
		private Account Acc2= null;
		private Account Acc3= null;
		private Account Acc4= null;
		
		private Account Acc5= null;
		private Account Acc6= null;
		private Account Acc7= null;
		private Account Acc8= null;
		private Account Acc9= null;
		
		private Order	BuyOrder;
		private Order	SellOrder;
		
		private string lastOrderID = "";
		Position myPosition = null;
		
		OrderAction orderAction;
		OrderType orderType = OrderType.Market;
		private int ordQuantity;
		
		Execution masterExecution = null;
		private Position masterPosition = null;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Simple Trade Copier with Invert";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				
				ThisTool = "https://discord.gg/gB75nGrzZx";
				
			}
			else if (State == State.Configure)
			{
				IsCopyAllowed = false;
				ShouldInvertTrades = false;
				ShouldInvertTradesOnChop = false;
			}
			else if(State == State.DataLoaded)
			{
				#region Accounts
				// Account select
				lock (Account.All)
					if (MasterAccount_name != null  ) MasterAccount = Account.All.FirstOrDefault(a => a.Name == MasterAccount_name);
					if (Accountname_1 != null  ) Acc1 = Account.All.FirstOrDefault(a => a.Name == Accountname_1);
					if (Accountname_2 != null  ) Acc2 = Account.All.FirstOrDefault(a => a.Name == Accountname_2);
					if (Accountname_3 != null  ) Acc3 = Account.All.FirstOrDefault(a => a.Name == Accountname_3);
					if (Accountname_4 != null  ) Acc4 = Account.All.FirstOrDefault(a => a.Name == Accountname_4);
					if (Accountname_5 != null  ) Acc5 = Account.All.FirstOrDefault(a => a.Name == Accountname_5);
					if (Accountname_6 != null  ) Acc6 = Account.All.FirstOrDefault(a => a.Name == Accountname_6);
					if (Accountname_7 != null  ) Acc7 = Account.All.FirstOrDefault(a => a.Name == Accountname_7);
					if (Accountname_8 != null  ) Acc8 = Account.All.FirstOrDefault(a => a.Name == Accountname_8);
					if (Accountname_9 != null  ) Acc9 = Account.All.FirstOrDefault(a => a.Name == Accountname_9);
					
			
				
					if (MasterAccount != null) MasterAccount.OrderUpdate 	+= OnOrderUpdate;
					if (MasterAccount != null) MasterAccount.PositionUpdate 	+= OnPositionUpdate;
					if (MasterAccount != null) MasterAccount.ExecutionUpdate 	+= OnExecutionUpdate;
					
					if (Acc1 != null) 	{Acc1.PositionUpdate 	+= OnPositionUpdate; Acc1.OrderUpdate 	+= OnOrderUpdate; Acc1.ExecutionUpdate 	+= OnExecutionUpdate;}
					if (Acc2 != null) 	{Acc2.PositionUpdate 	+= OnPositionUpdate; Acc2.OrderUpdate 	+= OnOrderUpdate; Acc2.ExecutionUpdate 	+= OnExecutionUpdate;}
					if (Acc3 != null) 	{Acc3.PositionUpdate 	+= OnPositionUpdate; Acc3.OrderUpdate 	+= OnOrderUpdate; Acc3.ExecutionUpdate 	+= OnExecutionUpdate;}
					if (Acc4 != null) 	{Acc4.PositionUpdate 	+= OnPositionUpdate; Acc4.OrderUpdate 	+= OnOrderUpdate; Acc4.ExecutionUpdate 	+= OnExecutionUpdate;}
					if (Acc5 != null) 	{Acc5.PositionUpdate 	+= OnPositionUpdate; Acc5.OrderUpdate 	+= OnOrderUpdate; Acc5.ExecutionUpdate 	+= OnExecutionUpdate;}
					if (Acc6 != null) 	{Acc6.PositionUpdate 	+= OnPositionUpdate; Acc6.OrderUpdate 	+= OnOrderUpdate; Acc6.ExecutionUpdate 	+= OnExecutionUpdate;}
					if (Acc7 != null) 	{Acc7.PositionUpdate 	+= OnPositionUpdate; Acc7.OrderUpdate 	+= OnOrderUpdate; Acc7.ExecutionUpdate 	+= OnExecutionUpdate;}
					if (Acc8 != null) 	{Acc8.PositionUpdate 	+= OnPositionUpdate; Acc8.OrderUpdate 	+= OnOrderUpdate; Acc8.ExecutionUpdate 	+= OnExecutionUpdate;}
					if (Acc9 != null) 	{Acc9.PositionUpdate 	+= OnPositionUpdate; Acc9.OrderUpdate 	+= OnOrderUpdate; Acc9.ExecutionUpdate 	+= OnExecutionUpdate;}
	
					
					if (MasterAccount != null ) 
						foreach (Position pos in MasterAccount.Positions)
						{
								if ( pos.Instrument == Instrument && pos.Account == MasterAccount )
								masterPosition = pos;
						}
				#endregion
				
				
			}
			else if (State == State.Historical)
			{
				if (UserControlCollection.Contains(myGrid))
					return;
				
				Dispatcher.InvokeAsync((() =>
				{
					myGrid = new System.Windows.Controls.Grid
					{
						Name = "MyCustomGrid", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom
					};
					System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
					myGrid.ColumnDefinitions.Add(column1);
					System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
					myGrid.ColumnDefinitions.Add(column2);
					copyButton = new System.Windows.Controls.Button
					{
						Name = "Copy", Content = "Copy OFF", Foreground = Brushes.White, Background = Brushes.Maroon
					};					
					copyButton.Click += OnButtonClick;
					invertButton = new System.Windows.Controls.Button
					{
						Name = "Invert", Content = "Invert OFF", Foreground = Brushes.White, Background = Brushes.Maroon
					};					
					invertButton.Click += OnButtonClick1;
					System.Windows.Controls.Grid.SetColumn(invertButton, 0);
					System.Windows.Controls.Grid.SetColumn(copyButton, 1);
					myGrid.Children.Add(copyButton);
					myGrid.Children.Add(invertButton);
					UserControlCollection.Add(myGrid);
				}));
				//=========================
				
			}
			else if (State == State.Terminated)
			{	
				IsCopyAllowed = false;
				Dispatcher.InvokeAsync((() =>
				{
					if (myGrid != null)
					{
						if (copyButton != null)
						{
							myGrid.Children.Remove(copyButton);
							copyButton.Click -= OnButtonClick;
							copyButton = null;
						}
						if (invertButton != null)
						{
							myGrid.Children.Remove(invertButton);
							invertButton.Click -= OnButtonClick1;
							invertButton = null;
						}
					}
				}));
				//---
				if (MasterAccount != null) MasterAccount.OrderUpdate 	-= OnOrderUpdate;
				if (MasterAccount != null) MasterAccount.PositionUpdate 	-= OnPositionUpdate;
				if (MasterAccount != null) MasterAccount.ExecutionUpdate 	-= OnExecutionUpdate;
			
					if (Acc1 != null) 	{Acc1.PositionUpdate 	-= OnPositionUpdate; Acc1.OrderUpdate 	-= OnOrderUpdate; Acc1.ExecutionUpdate 	-= OnExecutionUpdate;}
					if (Acc2 != null) 	{Acc2.PositionUpdate 	-= OnPositionUpdate; Acc2.OrderUpdate 	-= OnOrderUpdate; Acc2.ExecutionUpdate 	-= OnExecutionUpdate;}
					if (Acc3 != null) 	{Acc3.PositionUpdate 	-= OnPositionUpdate; Acc3.OrderUpdate 	-= OnOrderUpdate; Acc3.ExecutionUpdate 	-= OnExecutionUpdate;}
					if (Acc4 != null) 	{Acc4.PositionUpdate 	-= OnPositionUpdate; Acc4.OrderUpdate 	-= OnOrderUpdate; Acc4.ExecutionUpdate 	-= OnExecutionUpdate;}
					if (Acc5 != null) 	{Acc5.PositionUpdate 	-= OnPositionUpdate; Acc5.OrderUpdate 	-= OnOrderUpdate; Acc5.ExecutionUpdate 	-= OnExecutionUpdate;}
					if (Acc6 != null) 	{Acc6.PositionUpdate 	-= OnPositionUpdate; Acc6.OrderUpdate 	-= OnOrderUpdate; Acc6.ExecutionUpdate 	-= OnExecutionUpdate;}
					if (Acc7 != null) 	{Acc7.PositionUpdate 	-= OnPositionUpdate; Acc7.OrderUpdate 	-= OnOrderUpdate; Acc7.ExecutionUpdate 	-= OnExecutionUpdate;}
					if (Acc8 != null) 	{Acc8.PositionUpdate 	-= OnPositionUpdate; Acc8.OrderUpdate 	-= OnOrderUpdate; Acc8.ExecutionUpdate 	-= OnExecutionUpdate;}
					if (Acc9 != null) 	{Acc9.PositionUpdate 	-= OnPositionUpdate; Acc9.OrderUpdate 	-= OnOrderUpdate; Acc9.ExecutionUpdate 	-= OnExecutionUpdate;}
			}
			else if (State == State.Realtime)
			{
			// this will disable copy if you change current window/chart 
//				if (ChartControl != null )
//				{
//					Dispatcher.InvokeAsync((Action)(() => 
//					    {
//							chartWindow = Window.GetWindow(this.ChartControl.Parent) as Chart;
//						}));
					
//					if (!chartWindow.IsFocused) IsCopyAllowed = false;
//				}
			}
		}
		//---
		public override string DisplayName
		{
		  get { return "Simple Trade Copier with Invert";}

		}
		//---
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button == copyButton && button.Name == "Copy" && button.Content == "Copy OFF")
			{
				button.Content = "Copying...ON";
				button.Background = Brushes.DarkGreen;
				copyButtonClicked = true;
				IsCopyAllowed = true;
				return;
			}
			
			if (button == copyButton && button.Name == "Copy" && button.Content == "Copying...ON")
			{
				button.Content = "Copy OFF";
				button.Background = Brushes.Maroon;
				copyButtonClicked = false;
				IsCopyAllowed = false;
				return;
			}			
		}
		
		private void OnButtonClick1(object sender, RoutedEventArgs rea)
		{
			
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button == invertButton && button.Name == "Invert" && button.Content == "Invert OFF")
			{
				button.Content = "Inverting...ON";
				button.Background = Brushes.DarkGreen;
				invertButtonClicked = true;
				ShouldInvertTrades = true;
				ShouldInvertTradesOnChop = false;
				return;
			}
			
			if (button == invertButton && button.Name == "Invert" && button.Content == "Inverting...ON")
			{
				button.Content = "Invert on Chop... ON";
				button.Background = Brushes.DarkGreen;
				invertButtonClicked = true;
				ShouldInvertTrades = false;
				ShouldInvertTradesOnChop = true;
				return;
			}
			
			if (button == invertButton && button.Name == "Invert" && button.Content == "Invert on Chop... ON")
			{
				button.Content = "Invert OFF";
				button.Background = Brushes.Maroon;
				invertButtonClicked = false;
				ShouldInvertTrades = false;
				ShouldInvertTradesOnChop = false;
				return;
			}
		}
		
		private void OnAccountItemUpdate(object sender, AccountItemEventArgs e){}	
		private void OnExecutionUpdate(object sender, ExecutionEventArgs e){}			
	    private void OnPositionUpdate(object sender, PositionEventArgs e)
		    {
				masterPosition = null;
				if ( e.Position.Instrument == Instrument && e.Position.Account == MasterAccount )
					masterPosition = e.Position;	
			}
			//---		
		private void OnOrderUpdate(object sender, OrderEventArgs e)
	    	{
				if (!IsCopyAllowed) return;
				
				// ----------   ENRTY LONG/SHORT   -----------------------
				if (e.Order.Account == MasterAccount && e.Order.Instrument == Instrument)
					{
						if (e.OrderState == OrderState.Submitted && e.Order.IsMarket && e.OrderState != OrderState.CancelSubmitted )
							{
								if (Acc1 != null ) sendOrder(Acc1, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
								if (Acc2 != null ) sendOrder(Acc2, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
								if (Acc3 != null ) sendOrder(Acc3, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
								if (Acc4 != null ) sendOrder(Acc4, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
								if (Acc5 != null ) sendOrder(Acc5, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
								if (Acc6 != null ) sendOrder(Acc6, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
								if (Acc7 != null ) sendOrder(Acc7, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
								if (Acc8 != null ) sendOrder(Acc8, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
								if (Acc9 != null ) sendOrder(Acc9, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
							}
					}
					//---
				if (e.Order.Account == MasterAccount && e.Order.Instrument == Instrument)
					{
						if ( e.OrderState == OrderState.Filled && ( e.Order.IsLimit || e.Order.IsStopMarket ) )
							{
								if (Acc1 != null ) sendOrder(Acc1, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
								if (Acc2 != null ) sendOrder(Acc2, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
								if (Acc3 != null ) sendOrder(Acc3, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
								if (Acc4 != null ) sendOrder(Acc4, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
								if (Acc5 != null ) sendOrder(Acc5, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
								if (Acc6 != null ) sendOrder(Acc6, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
								if (Acc7 != null ) sendOrder(Acc7, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
								if (Acc8 != null ) sendOrder(Acc8, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
								if (Acc9 != null ) sendOrder(Acc9, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
							}
					}
	    }
		//---
		private void sendOrder(Account acc, OrderAction ordAction, OrderType ordType, int ordQuantity, string ordName)
			{
				if (!IsCopyAllowed) return;
				Order ordToSend = null;
				OrderAction newOrderAction = ordAction;
				if (ShouldInvertTrades) {
					if (ordAction == OrderAction.Buy)
					{
						newOrderAction = OrderAction.Sell;
					} 
					else
					{
						newOrderAction = OrderAction.Buy;
					}
				}
				ordToSend = acc.CreateOrder(Instrument, newOrderAction, ordType, OrderEntry.Manual, TimeInForce.Day, ordQuantity ,0, 0, "", ordName, DateTime.MaxValue, null);
				acc.Submit(new[] { ordToSend });
			}
			//---
		
		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (ShouldInvertTradesOnChop)
			{
				ShouldInvertTrades = ChoppinessIndex(14)[0] > 38.2;	
			}
			
		}
		
		#region properties
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Support/HowToUse/SetUp -> :",  		       					Order=0, GroupName="0. Support")]
		public string ThisTool
		{ get; set; }
		
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Copy from this Account ", 								Order=1, GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string MasterAccount_name { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="To Account 1", 											Order=2, GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Accountname_1 { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="To Account 2", 											Order=3,  GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Accountname_2 { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="To Account 3", 											Order=4,  GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Accountname_3 { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="To Account 4", 											Order=5,  GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Accountname_4 { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="To Account 5", 											Order=6,  GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Accountname_5 { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="To Account 6", 											Order=7,  GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Accountname_6 { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="To Account 7", 											Order=8,  GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Accountname_7 { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="To Account 8", 											Order=9,  GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Accountname_8 { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="To Account 9", 											Order=10,  GroupName="1. Replicate ChartTrader")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Accountname_9 { get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Prop_Trader_Tools.SimpleTradeCopierWithInvert[] cacheSimpleTradeCopierWithInvert;
		public Prop_Trader_Tools.SimpleTradeCopierWithInvert SimpleTradeCopierWithInvert(string thisTool, string masterAccount_name, string accountname_1, string accountname_2, string accountname_3, string accountname_4, string accountname_5, string accountname_6, string accountname_7, string accountname_8, string accountname_9)
		{
			return SimpleTradeCopierWithInvert(Input, thisTool, masterAccount_name, accountname_1, accountname_2, accountname_3, accountname_4, accountname_5, accountname_6, accountname_7, accountname_8, accountname_9);
		}

		public Prop_Trader_Tools.SimpleTradeCopierWithInvert SimpleTradeCopierWithInvert(ISeries<double> input, string thisTool, string masterAccount_name, string accountname_1, string accountname_2, string accountname_3, string accountname_4, string accountname_5, string accountname_6, string accountname_7, string accountname_8, string accountname_9)
		{
			if (cacheSimpleTradeCopierWithInvert != null)
				for (int idx = 0; idx < cacheSimpleTradeCopierWithInvert.Length; idx++)
					if (cacheSimpleTradeCopierWithInvert[idx] != null && cacheSimpleTradeCopierWithInvert[idx].ThisTool == thisTool && cacheSimpleTradeCopierWithInvert[idx].MasterAccount_name == masterAccount_name && cacheSimpleTradeCopierWithInvert[idx].Accountname_1 == accountname_1 && cacheSimpleTradeCopierWithInvert[idx].Accountname_2 == accountname_2 && cacheSimpleTradeCopierWithInvert[idx].Accountname_3 == accountname_3 && cacheSimpleTradeCopierWithInvert[idx].Accountname_4 == accountname_4 && cacheSimpleTradeCopierWithInvert[idx].Accountname_5 == accountname_5 && cacheSimpleTradeCopierWithInvert[idx].Accountname_6 == accountname_6 && cacheSimpleTradeCopierWithInvert[idx].Accountname_7 == accountname_7 && cacheSimpleTradeCopierWithInvert[idx].Accountname_8 == accountname_8 && cacheSimpleTradeCopierWithInvert[idx].Accountname_9 == accountname_9 && cacheSimpleTradeCopierWithInvert[idx].EqualsInput(input))
						return cacheSimpleTradeCopierWithInvert[idx];
			return CacheIndicator<Prop_Trader_Tools.SimpleTradeCopierWithInvert>(new Prop_Trader_Tools.SimpleTradeCopierWithInvert(){ ThisTool = thisTool, MasterAccount_name = masterAccount_name, Accountname_1 = accountname_1, Accountname_2 = accountname_2, Accountname_3 = accountname_3, Accountname_4 = accountname_4, Accountname_5 = accountname_5, Accountname_6 = accountname_6, Accountname_7 = accountname_7, Accountname_8 = accountname_8, Accountname_9 = accountname_9 }, input, ref cacheSimpleTradeCopierWithInvert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Prop_Trader_Tools.SimpleTradeCopierWithInvert SimpleTradeCopierWithInvert(string thisTool, string masterAccount_name, string accountname_1, string accountname_2, string accountname_3, string accountname_4, string accountname_5, string accountname_6, string accountname_7, string accountname_8, string accountname_9)
		{
			return indicator.SimpleTradeCopierWithInvert(Input, thisTool, masterAccount_name, accountname_1, accountname_2, accountname_3, accountname_4, accountname_5, accountname_6, accountname_7, accountname_8, accountname_9);
		}

		public Indicators.Prop_Trader_Tools.SimpleTradeCopierWithInvert SimpleTradeCopierWithInvert(ISeries<double> input , string thisTool, string masterAccount_name, string accountname_1, string accountname_2, string accountname_3, string accountname_4, string accountname_5, string accountname_6, string accountname_7, string accountname_8, string accountname_9)
		{
			return indicator.SimpleTradeCopierWithInvert(input, thisTool, masterAccount_name, accountname_1, accountname_2, accountname_3, accountname_4, accountname_5, accountname_6, accountname_7, accountname_8, accountname_9);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Prop_Trader_Tools.SimpleTradeCopierWithInvert SimpleTradeCopierWithInvert(string thisTool, string masterAccount_name, string accountname_1, string accountname_2, string accountname_3, string accountname_4, string accountname_5, string accountname_6, string accountname_7, string accountname_8, string accountname_9)
		{
			return indicator.SimpleTradeCopierWithInvert(Input, thisTool, masterAccount_name, accountname_1, accountname_2, accountname_3, accountname_4, accountname_5, accountname_6, accountname_7, accountname_8, accountname_9);
		}

		public Indicators.Prop_Trader_Tools.SimpleTradeCopierWithInvert SimpleTradeCopierWithInvert(ISeries<double> input , string thisTool, string masterAccount_name, string accountname_1, string accountname_2, string accountname_3, string accountname_4, string accountname_5, string accountname_6, string accountname_7, string accountname_8, string accountname_9)
		{
			return indicator.SimpleTradeCopierWithInvert(input, thisTool, masterAccount_name, accountname_1, accountname_2, accountname_3, accountname_4, accountname_5, accountname_6, accountname_7, accountname_8, accountname_9);
		}
	}
}

#endregion
