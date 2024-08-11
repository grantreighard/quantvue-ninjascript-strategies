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
	public class QuantVueMoneyball : Strategy
	{
		private Moneyball Moneyball1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "QuantVueMoneyball";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.ImmediatelySubmitSynchronizeAccount;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				TP = 30;
				SL = 20;
				DQ = 1;
			}
			else if (State == State.Configure)
			{
				DefaultQuantity = DQ;
			}
			else if (State == State.DataLoaded)
			{				
				Moneyball1 = Moneyball(Close, Brushes.RoyalBlue, Brushes.Blue, 15, 10, true, 0.35, -0.35, 0.1, MoneyballMode.M);
				SetProfitTarget(CalculationMode.Currency, TP);
				SetStopLoss(CalculationMode.Currency, SL);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 5)
				return;

			 // Set 1
			if (CrossAbove(Moneyball1.VBar, 0.35, 1))
			{
				EnterLong(DefaultQuantity);
			}
			
			if (CrossBelow(Moneyball1.VBar, -0.35, 1))
			{
				EnterShort(DefaultQuantity);
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
