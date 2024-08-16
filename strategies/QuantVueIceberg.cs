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
	public class QuantVueIceberg : Strategy
	{
		
		private Moneyball Moneyball1;
		private Qcloud Qcloud1;
		private Qwave Qwave1;
		private MACD MACD1;
		private bool isBreakevenSet = false;
		private	CustomEnumNamespaceIce.TimeMode			TimeModeSelect		= CustomEnumNamespaceIce.TimeMode.Restricted;
		private DateTime 								startTime 			= DateTime.Parse("11:00:00", System.Globalization.CultureInfo.InvariantCulture);
		private DateTime		 						endTime 			= DateTime.Parse("13:00:00", System.Globalization.CultureInfo.InvariantCulture);
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "QuantVueIceberg";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.ImmediatelySubmit;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 30;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				TP = 300;
				SL = 100;
				DQ = 10;
				BreakevenProfit = 5;
				FastPeriod = 12;  
                SlowPeriod  = 26;  
                SignalPeriod  = 9; 
			}
			else if (State == State.Configure)
			{
				DefaultQuantity = DQ;
			}
			else if (State == State.DataLoaded)
			{				
				Moneyball1 = Moneyball(Close, Brushes.RoyalBlue, Brushes.Blue, 15, 10, true, 0.35, -0.35, 0.1, MoneyballMode.M);
				Qcloud1 = Qcloud(Close, Brushes.Red, Brushes.Green, 19, 29, 49, 59, 69, 99);
				Qwave1 = Qwave(Close, 55, 256, 1.5, 0.1, 9, false, false, Brushes.Transparent);
				MACD1 = MACD(Close, FastPeriod, SlowPeriod, SignalPeriod);
				AddChartIndicator(Moneyball(Close, Brushes.RoyalBlue, Brushes.Blue, 15, 10, true, 0.35, -0.35, 0.1, MoneyballMode.M));
				AddChartIndicator(Qcloud(Close, Brushes.Red, Brushes.Green, 19, 29, 49, 59, 69, 99));
				AddChartIndicator(Qwave(Close, 55, 256, 1.5, 0.1, 9, false, false, Brushes.Transparent));
				AddChartIndicator(MACD(Close, FastPeriod, SlowPeriod, SignalPeriod));
				
			}
		}

		protected override void OnBarUpdate()
		{			
			
			if (BarsInProgress != 0 || CurrentBar < BarsRequiredToTrade)
				return;
			
			if ((ToTime(Time[0]) >= ToTime(startTime) && ToTime(Time[0]) <= ToTime(endTime)) || TimeModeSelect == CustomEnumNamespaceIce.TimeMode.Unrestricted)
			{

				if (CrossAbove(Moneyball1.VBar, 0.35, 5) && CrossAbove(Qcloud1.V1, Qcloud1.V6,51) && CrossAbove(Qwave1.K1, Qwave1.VHigh, 5) && CrossAbove(MACD1.Default, MACD1.Avg, 5)  && Close[0] > SMA(Close, 21)[0])
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), "GoLong");
					SetStopLoss("GoLong", CalculationMode.Currency, SL, false);
					SetProfitTarget("GoLong", CalculationMode.Currency, TP);
					isBreakevenSet = false;
				}
			
				if (CrossBelow(Moneyball1.VBar, -0.35, 5) && CrossBelow(Qcloud1.V1, Qcloud1.V6, 5) && CrossBelow(Qwave1.VLow, Qwave1.K1, 5) && CrossBelow(MACD1.Default, MACD1.Avg, 5) && Close[0] < SMA(Close, 21)[0])
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), "GoShort");
					SetStopLoss("GoShort", CalculationMode.Currency, SL, false);
					SetProfitTarget("GoShort", CalculationMode.Currency, TP);
					isBreakevenSet = false;
				}
			}
				
			if (BreakevenProfit > 0)
			{
				if (Position.MarketPosition != MarketPosition.Flat && (Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]) >= BreakevenProfit) && !isBreakevenSet)
				{
					SetStopLoss("GoLong", CalculationMode.Price, Position.AveragePrice + (BreakevenProfit / 2), true);
					SetStopLoss("GoShort", CalculationMode.Price, Position.AveragePrice + (BreakevenProfit / 2), true);
					isBreakevenSet = true;
				}
			}
			
		}
		
		#region Properties
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trading Hour Restriction", GroupName = "Time Parameters", Order = 0)]
		public CustomEnumNamespaceIce.TimeMode TIMEMODESelect
		{
			get { return TimeModeSelect; }
			set { TimeModeSelect = value; }
		}
				
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [NinjaScriptProperty]
        [Display(Name = "Opening Range-Start", GroupName = "Time Parameters", Order = 1)]
        public DateTime StartTime 
		{
			get { return startTime; }
			set { startTime = value; }
		}
		
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
       	[NinjaScriptProperty]
       	[Display(Name = "Opening Range-End", GroupName = "Time Parameters", Order = 2)]
        public DateTime EndTime
		{
			get { return endTime; }
			set { endTime = value; }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TP", Order=1, GroupName="Parameters")]
		public int TP
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SL", Order=2, GroupName="Parameters")]
		public int SL
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="DQ", Order=3, GroupName="Parameters")]
		public int DQ
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Breakeven Profit", Order=4, GroupName="Parameters")]
		public int BreakevenProfit
		{ get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Fast Period", Order=5, GroupName="Parameters")]
        public int FastPeriod 
		{ get; set; }

		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Slow Period", Order=6, GroupName="Parameters")]
        public int SlowPeriod 
		{ get; set; }

		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Signal Period", Order=7, GroupName="Parameters")]
        public int SignalPeriod 
		{ get; set; }
        #endregion
	}
}

namespace CustomEnumNamespaceIce
{
	public enum TimeMode
	{
		Restricted,
		Unrestricted
	}
}
