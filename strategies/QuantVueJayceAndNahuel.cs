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
	public class QuantVueJayceAndNahuel : Strategy
	{
		private iGRID_EVO iGRID_EVO1;
		private Qwave Qwave1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "QuantVueJayceAndNahuel";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 3;
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
				
				TP = 400;
				SL = 100;
				DQ = 2;
			}
			else if (State == State.Configure)
			{
				DefaultQuantity = DQ;
			}
			else if (State == State.DataLoaded)
			{
				iGRID_EVO1 = iGRID_EVO(Close, 19, 19, 2.5, true, 2, 50, 7); // Qgrid
				Qwave1 = Qwave(Close, 55, 256, 1.5, 0.1, 9, false, false, Brushes.Transparent, false);
				AddChartIndicator( iGRID_EVO(Close, 19, 19, 2.5, true, 2, 50, 7));
				AddChartIndicator(Qwave(Close, 55, 256, 1.5, 0.1, 9, false, false, Brushes.Transparent, false));
			}
			
		}

		protected override void OnBarUpdate()
		{
			if (CrossAbove(Close, Qwave1.VHigh, 1) && iGRID_EVO1.StepMA[0] < Close[0])
			{
				EnterLong(Convert.ToInt32(DefaultQuantity), "GoLong");
				SetStopLoss("GoLong", CalculationMode.Currency, SL, false);
				SetProfitTarget("GoLong", CalculationMode.Currency, TP);
			}
			
			if (CrossBelow(Close, Qwave1.VLow, 1) && iGRID_EVO1.StepMA[0] > Close[0])
			{
				EnterShort(Convert.ToInt32(DefaultQuantity), "GoShort");
				SetStopLoss("GoShort", CalculationMode.Currency, SL, false);
				SetProfitTarget("GoShort", CalculationMode.Currency, TP);
			}
			
			if (Position.MarketPosition == MarketPosition.Long)
			{
				if (CrossBelow(Close, Qwave1.VHigh, 1) || Close[0] < Open[0])	
				{
					ExitLong();
				}
			}
			
			if (Position.MarketPosition == MarketPosition.Short)
			{
				if (CrossAbove(Close, Qwave1.VLow, 1) || Close[0] > Open[0])
				{
					ExitShort();
				}
			}
		}
		
		#region Properties
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
		#endregion
	}
}
