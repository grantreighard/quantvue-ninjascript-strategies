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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class GridMoney : Strategy
	{
		private Moneyball Moneyball1;
		private iGRID_EVO iGRID_EVO1;
		private	CustomEnumNamespaceGridMoney.TimeMode	TimeModeSelect		= CustomEnumNamespaceGridMoney.TimeMode.Restricted;
		private DateTime 								startTime 			= DateTime.Parse("9:00:00", System.Globalization.CultureInfo.InvariantCulture);
		private DateTime		 						endTime 			= DateTime.Parse("13:00:00", System.Globalization.CultureInfo.InvariantCulture);
		private double currentPnL;
		private int										grid1Flip;
		private int										bullbearHA2;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "GridMoney";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				DQ = 1;
				mb_Nb_bars = 15;
				mb_period = 10;
				mb_zero = true;
				mb_uThreshold = 0.35;
				mb_lThreshold = -0.35;
				mb_Sensitivity = 0.1;
				grid1Period1 = 55;
				grid1omaL = 19;
				grid1omaS = 2.9;
				grid1omaA = true;
				grid1Sensitivity = 2;
				grid1StepSize = 50;
				grid1Period2 = 8;
				maxDailyProfit = false;
				maxDailyProfitAmount = 500;
				maxDailyLoss = false;
				maxDailyLossAmount = 500;
			}
			else if (State == State.Configure)
			{
				DefaultQuantity = DQ;
			}
			else if (State == State.DataLoaded)
			{				
				Moneyball1 = Moneyball(Close, Brushes.RoyalBlue, Brushes.Blue, mb_Nb_bars, mb_period, mb_zero, mb_uThreshold, mb_lThreshold, mb_Sensitivity, MoneyballMode.M, false);
				AddChartIndicator(Moneyball(Close, Brushes.RoyalBlue, Brushes.Blue, mb_Nb_bars, mb_period, true, mb_uThreshold, mb_lThreshold, mb_Sensitivity, MoneyballMode.M, false));
				iGRID_EVO1 = iGRID_EVO(Close, grid1Period1, grid1omaL, grid1omaS, grid1omaA, grid1Sensitivity, grid1StepSize, grid1Period2);
                AddChartIndicator(iGRID_EVO(Close, grid1Period1, grid1omaL, grid1omaS, grid1omaA, grid1Sensitivity, grid1StepSize, grid1Period2));
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 5)
				return;
			
			if (iGRID_EVO1.FlipSignal[0] == 1)
			{
				grid1Flip = 1;
			}
			else if (iGRID_EVO1.FlipSignal[0] == -1)
			{
				grid1Flip = 2;
			}
			
			if (iGRID_EVO1.HA2Close[1] < iGRID_EVO1.HA2Open[1])
			{
				bullbearHA2 = 2;
			}
			else if (iGRID_EVO1.HA2Close[1] > iGRID_EVO1.HA2Open[1])
			{
				bullbearHA2 = 1;
			}
			
			if (Position.MarketPosition == MarketPosition.Long && bullbearHA2 == 2)
			{
				ExitLong("GoLong");
			}
			else if (Position.MarketPosition == MarketPosition.Short && bullbearHA2 == 1)
			{
				ExitShort("GoShort");
			}
			
			
			if ((ToTime(Time[0]) >= ToTime(startTime) && ToTime(Time[0]) <= ToTime(endTime)) || TimeModeSelect == CustomEnumNamespaceGridMoney.TimeMode.Unrestricted && Position.MarketPosition == MarketPosition.Flat && (currentPnL <= maxDailyProfitAmount || maxDailyProfit == false) || (currentPnL >= -maxDailyLossAmount || maxDailyLoss == false))
			{
				
				/*
				// Set 1
				if (CrossAbove(Moneyball1.VBar, mb_uThreshold, 1) && grid1Flip == 1 && bullbearHA2 == 1)
				{
					EnterLong(DefaultQuantity, "GoLong");
					//SetProfitTarget("GoLong", CalculationMode.Currency, TP);
					//SetStopLoss("GoLong",CalculationMode.Currency, SL, false);
				}
				
				if (CrossBelow(Moneyball1.VBar, mb_lThreshold, 1) && grid1Flip == 2 && bullbearHA2 == 2)
				{
					EnterShort(DefaultQuantity, "GoShort");
					//SetProfitTarget("GoShort", CalculationMode.Currency, TP);
					//SetStopLoss("GoShort",CalculationMode.Currency, SL, false);
				}
				*/
				
				// Set 1
				if (Moneyball1.VBar[1] > mb_uThreshold && grid1Flip == 1 && bullbearHA2 == 1)
				{
					EnterLong(DefaultQuantity, "GoLong");
					//SetProfitTarget("GoLong", CalculationMode.Currency, TP);
					//SetStopLoss("GoLong",CalculationMode.Currency, SL, false);
				}
				
				if (Moneyball1.VBar[1] < mb_lThreshold && grid1Flip == 2 && bullbearHA2 == 2)
				{
					EnterShort(DefaultQuantity, "GoShort");
					//SetProfitTarget("GoShort", CalculationMode.Currency, TP);
					//SetStopLoss("GoShort",CalculationMode.Currency, SL, false);
				}
				
			}
			
			if ((Position.MarketPosition == MarketPosition.Long) && ((((currentPnL + Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0])) <= -maxDailyLossAmount) && maxDailyLoss == true) || (((currentPnL + Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0])) >= maxDailyProfitAmount) && maxDailyProfit == true))) ///If unrealized goes under maxDailyLossAmount 'OR' Above maxDailyProfitAmount
			{
				//Print((currentPnL+Position.GetProfitLoss(Close[0], PerformanceUnit.Currency)) + " - " + -maxDailyLossAmount);
				// print to the output window if the daily limit is hit in the middle of a trade
				Print("daily limit hit, exiting order " + Time[0].ToString());
				ExitLong("Daily Limit Exit", "GoLong");
			}
			
			if ((Position.MarketPosition == MarketPosition.Short) && ((((currentPnL + Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0])) <= -maxDailyLossAmount) && maxDailyLoss == true) || (((currentPnL + Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0])) >= maxDailyProfitAmount) && maxDailyProfit == true))) ///If unrealized goes under maxDailyLossAmount 'OR' Above maxDailyProfitAmount    
				
			{
				//Print((currentPnL+Position.GetProfitLoss(Close[0], PerformanceUnit.Currency)) + " - " + -maxDailyLossAmount);
				// print to the output window if the daily limit is hit in the middle of a trade
				Print("daily limit hit, exiting order " + Time[0].ToString());
				ExitShort("Daily Limit Exit", "GoShort");
			}
			
		}
		
		protected override void OnPositionUpdate(Position position, double averagePrice, int quantity, MarketPosition marketPosition)
		{
			if (Position.MarketPosition == MarketPosition.Flat && SystemPerformance.AllTrades.Count > 0)
			{
				// when a position is closed, add the last trade's Profit to the currentPnL
				currentPnL += SystemPerformance.AllTrades[SystemPerformance.AllTrades.Count - 1].ProfitCurrency;

				// print to output window if the daily limit is hit
				if (currentPnL <= -maxDailyLossAmount)
				{
					Print("daily limit hit, no new orders" + Time[0].ToString());
				}
				
				if (currentPnL >= maxDailyProfitAmount)
				{
					Print("daily Profit limit hit, no new orders" + Time[0].ToString()); ///Prints message to output
				}
				
				if (currentPnL >= -maxDailyLossAmount && currentPnL <= maxDailyProfitAmount)
				{
					Print(string.Format("Daily Profit = {0}", currentPnL)); ///Prints message to output
				}
			}
		}
		
		#region Properties
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trading Hour Restriction", GroupName = "1. Time Parameters", Order = 0)]
		public CustomEnumNamespaceGridMoney.TimeMode TIMEMODESelect
		{
			get { return TimeModeSelect; }
			set { TimeModeSelect = value; }
		}
				
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [NinjaScriptProperty]
        [Display(Name = "Opening Range-Start", GroupName = "1. Time Parameters", Order = 1)]
        public DateTime StartTime 
		{
			get { return startTime; }
			set { startTime = value; }
		}
		
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
       	[NinjaScriptProperty]
       	[Display(Name = "Opening Range-End", GroupName = "1. Time Parameters", Order = 2)]
        public DateTime EndTime
		{
			get { return endTime; }
			set { endTime = value; }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Max Daily Profit", Order=3, GroupName="2. PnL Parameters")]
		public bool maxDailyProfit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Max Daily Profit (Currency)", Order=4, GroupName="2. PnL Parameters")]
		public int maxDailyProfitAmount
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Max Daily Loss", Order=5, GroupName="2. PnL Parameters")]
		public bool maxDailyLoss
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Max Daily Loss (Currency)", Order=6, GroupName="2. PnL Parameters")]
		public int maxDailyLossAmount
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Number of Contracts", Order=0, GroupName="3. Trade Parameters")]
		public int DQ
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Number of bars between signals", Order=0, GroupName="4. Moneyball Parameters")]
		public int mb_Nb_bars
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="4. Moneyball Parameters")]
		public int mb_period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="All Zero", Order=2, GroupName="4. Moneyball Parameters")]
		public bool mb_zero
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(.001, 1.0)]
		[Display(Name="Upper Threshold", Order=4, GroupName="4. Moneyball Parameters")]
		public double mb_uThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-1.0, -.001)]
		[Display(Name="Lower Threshold", Order=5, GroupName="4. Moneyball Parameters")]
		public double mb_lThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.001, double.MaxValue)]
		[Display(Name="Sensitivity", Order=6, GroupName="4. Moneyball Parameters")]
		public double mb_Sensitivity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="HA Smooth Period 1", Order=1, GroupName="5. Qgrid Parameters")]
		public int grid1Period1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="OMA Length", Order=2, GroupName="5. Qgrid Parameters")]
		public int grid1omaL
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="OMA Speed", Order=3, GroupName="5. Qgrid Parameters")]
		public double grid1omaS
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Adaptive OMA", Order=4, GroupName="5. Qgrid Parameters")]
		public bool grid1omaA
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Sensitivity", Order=5, GroupName="5. Qgrid Parameters")]
		public double grid1Sensitivity
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Step Size", Order=6, GroupName="5. Qgrid Parameters")]
		public double grid1StepSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="HA Smooth Period 2", Order=7, GroupName="5. Qgrid Parameters")]
		public int grid1Period2
		{ get; set; }
		
		#endregion
		
	}
}

namespace CustomEnumNamespaceGridMoney
{
	public enum TimeMode
	{
		Restricted,
		Unrestricted
	}
}
