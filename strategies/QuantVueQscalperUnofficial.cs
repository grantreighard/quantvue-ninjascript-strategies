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
	public class QuantVueQscalperUnofficial : Strategy
	{
		private Qwave Qwave1;
		private Qcloud Qcloud1;
		private double currentPnL;
		
		private CustomEnumNamespaceScalper.TimeMode TimeModeSelect = CustomEnumNamespaceScalper.TimeMode.Restricted;
		private DateTime startTime = DateTime.Parse("09:35:00", System.Globalization.CultureInfo.InvariantCulture);
		private DateTime endTime = DateTime.Parse("16:00:00", System.Globalization.CultureInfo.InvariantCulture);
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "QuantVueQscalperUnofficial";
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
				
				TPTicks = 90;
				SLTicks = 30;
				DQ = 2;
				maxDailyProfit = true;
				maxDailyProfitAmount = 500;
				maxDailyLoss = true;
				maxDailyLossAmount = 250;
			}
			else if (State == State.Configure)
			{
				DefaultQuantity = DQ;
			}
			else if (State == State.DataLoaded)
			{				
				Qwave1 = Qwave(Close, 55, 256, 1.5, 0.1, 9, false, false, Brushes.Transparent, false);
				Qcloud1 = Qcloud(Close, Brushes.Red, Brushes.Green, 19, 29, 49, 59, 69, 99, false);
				AddChartIndicator(Qwave(Close, 55, 256, 1.5, 0.1, 9, false, false, Brushes.Transparent, false));
				AddChartIndicator(Qcloud(Close, Brushes.Red, Brushes.Green, 19, 29, 49, 59, 69, 99, false));
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
			if (CurrentBar < BarsRequiredToTrade)
				return;
			
			if (Bars.IsFirstBarOfSession)
			{
				currentPnL = 0;
			}

			if ((ToTime(Time[0]) >= ToTime(startTime) && ToTime(Time[0]) <= ToTime(endTime)) || TimeModeSelect == CustomEnumNamespaceScalper.TimeMode.Unrestricted && Position.MarketPosition == MarketPosition.Flat)
			{
				if ((currentPnL <= maxDailyProfitAmount || maxDailyProfit == false) || (currentPnL >= -maxDailyLossAmount || maxDailyLoss == false))
				{
					// long scalp
					if (Open[4] > Close[4] && Close[2] < Close[4] && Close[2] < Close[0] && Open[0] < Close[0]) // reversal pattern
					{
						if (Close[2] <= Qwave1.VLow[0] + 10 * TickSize && Close[2] >= Qwave1.VLow[0] - 10 * TickSize ||
							Close[2] <= Qwave1.V1[0] + 10 * TickSize && Close[2] >= Qwave1.V1[0] - 10 * TickSize) // within proximity zone
						{
							
							if (Volume[1] < Volume[0] && ADX(21)[0] < 20) // increasing volume, in chop
							{
								EnterLong(Convert.ToInt32(DefaultQuantity), "GoLong");
								SetStopLoss("GoLong", CalculationMode.Ticks, SLTicks, true);
								SetProfitTarget("GoLong", CalculationMode.Ticks, TPTicks);
							}
						}
					}
					
					// short scalp
					if (Open[4] < Close[4] && Close[2] > Close[4] && Close[2] > Close[0] && Open[0] > Close[0]) // reversal pattern
					{
						if (Close[2] <= Qwave1.VHigh[0] + 10 * TickSize && Close[2] >= Qwave1.VHigh[0] - 10 * TickSize ||
							Close[2] <= Qwave1.V1[0] + 10 * TickSize && Close[2] >= Qwave1.V1[0] - 10 * TickSize) // within proximity zone
						{
							
							if (Volume[1] < Volume[0] && ADX(21)[0] < 20) // increasing volume, in chop
							{
								EnterShort(Convert.ToInt32(DefaultQuantity), "GoShort");
								SetStopLoss("GoShort", CalculationMode.Ticks, SLTicks, true);
								SetProfitTarget("GoShort", CalculationMode.Ticks, TPTicks);
							}
						}
					}
				}
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trading Hour Restriction", GroupName = "Time Parameters", Order = 0)]
		public CustomEnumNamespaceScalper.TimeMode TIMEMODESelect
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
		[Display(Name = "TP (Ticks)", Order = 1, GroupName = "Trade Parameters")]
		public int TPTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "SL (Ticks)", Order = 2, GroupName = "Trade Parameters")]
		public int SLTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Default Quantity (Contracts)", Order = 3, GroupName = "Trade Parameters")]
		public int DQ
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Max Daily Profit", Order = 5, GroupName = "Trade Parameters")]
		public bool maxDailyProfit
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Max Daily Profit (Currency)", Order = 6, GroupName = "Trade Parameters")]
		public int maxDailyProfitAmount
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Max Daily Loss", Order = 7, GroupName = "Trade Parameters")]
		public bool maxDailyLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Max Daily Loss (Currency)", Order = 7, GroupName = "Trade Parameters")]
		public int maxDailyLossAmount
		{ get; set; }
		#endregion
	}
}


namespace CustomEnumNamespaceScalper
{
	public enum TimeMode
	{
		Restricted,
		Unrestricted
	}

	public enum StopMode
	{
		BEOnly,
		StepSL,
		//IceburgATM
	}
}
