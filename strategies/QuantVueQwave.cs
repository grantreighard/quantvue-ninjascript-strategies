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
        public class QuantVueQwave : Strategy
        {
        	private Qwave Qwave1;
       		private double entryPrice = 0.0;
        	private bool isBreakevenSet = false; // Flag to check if breakeven is already set
        	private Order stopLossOrderLong;
        	private Order stopLossOrderShort;
			private	CustomEnumNamespaceWave.TimeMode		TimeModeSelect			= CustomEnumNamespaceWave.TimeMode.Restricted;
			private DateTime 								startTime 				= DateTime.Parse("11:00:00", System.Globalization.CultureInfo.InvariantCulture);
			private DateTime		 						endTime 				= DateTime.Parse("13:00:00", System.Globalization.CultureInfo.InvariantCulture);
			private	CustomEnumNamespaceWave.StopMode		StopModeSelect			= CustomEnumNamespaceWave.StopMode.BEOnly;
			private int										tickCount				= 1;
			private int 									priorTradesCount 		= 0;
			private double 									priorTradesCumProfit	= 0;
			private double 									currentPnL;


                protected override void OnStateChange()
                {
                        if (State == State.SetDefaults)
                        {
                                Description									= @"Enter the description for your new custom Strategy here.";
                                Name										= "QuantVueQwave";
                                Calculate									= Calculate.OnEachTick;
                                EntriesPerDirection							= 1;
                                EntryHandling								= EntryHandling.AllEntries;
                                IsExitOnSessionCloseStrategy				= true;
                                ExitOnSessionCloseSeconds					= 30;
                                IsFillLimitOnTouch							= false;
                                MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
                                OrderFillResolution							= OrderFillResolution.Standard;
                                Slippage									= 0;
                                StartBehavior								= StartBehavior.WaitUntilFlatSynchronizeAccount;
                                TimeInForce									= TimeInForce.Gtc;
                                TraceOrders									= false;
                                RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
                                StopTargetHandling							= StopTargetHandling.PerEntryExecution;
                                BarsRequiredToTrade							= 20;
                                RealtimeErrorHandling						= RealtimeErrorHandling.IgnoreAllErrors;
                                // Disable this property for performance gains in Strategy Analyzer optimizations
                                // See the Help Guide for additional information
                                IsInstantiatedOnEachOptimizationIteration	= true;
                                TP = 30;
                                SL = 20;
                                DQ = 1;
								BreakevenProfit = 0;
								maxDailyProfit = false;
								maxDailyProfitAmount = 500;
								maxDailyLoss = false;
								maxDailyLossAmount = 500;
								slFrequency = 1;
								slStepSize = 1;
                        }
                        else if (State == State.Configure)
                        {
                                DefaultQuantity = DQ;
                        }
                        else if (State == State.DataLoaded)
                        {            	                    
                            Qwave1 = Qwave(Close, 55, 256, 1.5, 0.1, 9, false, false, Brushes.Transparent, false);
							AddChartIndicator(Qwave(Close, 55, 256, 1.5, 0.1, 9, false, false, Brushes.Transparent, false));
                        }
                }
                
                protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
                                    OrderState orderState, DateTime time, ErrorCode error, string nativeError)
                {
                  if (error != ErrorCode.NoError) 
                  {
                        ExitLong();
                        ExitShort();
                  }
                }
                
                protected override void OnBarUpdate()
                {
                        if (BarsInProgress != 0) 
                                return;


                        if (CurrentBars[0] < 1)
                                return;
						
						if (Bars.IsFirstBarOfSession)
						{
							currentPnL = 0;
						}
                        
                        if ((ToTime(Time[0]) >= ToTime(startTime) && ToTime(Time[0]) <= ToTime(endTime)) || TimeModeSelect == CustomEnumNamespaceWave.TimeMode.Unrestricted && Position.MarketPosition == MarketPosition.Flat)
						{
							if ((currentPnL <= maxDailyProfitAmount || maxDailyProfit == false) || (currentPnL >= -maxDailyLossAmount || maxDailyLoss == false))
							{
                                if (CrossAbove(Qwave1.K1, Qwave1.VHigh, 1) || CrossAbove(Qwave1.K1, Qwave1.V1, 1))
                                {
                                    EnterLong(Convert.ToInt32(DefaultQuantity), "GoLong");
                                    SetStopLoss("GoLong", CalculationMode.Currency, SL, false);
									SetProfitTarget("GoLong", CalculationMode.Currency, TP, false);
									isBreakevenSet = false;
                                }
                                
                                 // Set 2
                                if (CrossBelow(Qwave1.VLow, Qwave1.K1, 1) || CrossBelow(Qwave1.K1, Qwave1.V1, 1))
                                {
                                    EnterShort(Convert.ToInt32(DefaultQuantity), "GoShort");
                                    SetStopLoss("GoShort", CalculationMode.Currency, SL, false);
									SetProfitTarget("GoShort", CalculationMode.Currency, TP, false);
									isBreakevenSet = false;
                                }
							}
                        }
						
						// BE Only Stop Loss Mode //
			
						if (BreakevenProfit > 0 && StopModeSelect == CustomEnumNamespaceWave.StopMode.BEOnly || StopModeSelect == CustomEnumNamespaceWave.StopMode.StepSL)
						{
							if (Position.MarketPosition != MarketPosition.Flat && (Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]) >= BreakevenProfit) && !isBreakevenSet)
							{
								if (Position.MarketPosition == MarketPosition.Long)
								{
									SetStopLoss("GoLong", CalculationMode.Price, Position.AveragePrice, false);
									isBreakevenSet = true;
									tickCount = 1;
								}
								else if (Position.MarketPosition == MarketPosition.Short)
								{
									SetStopLoss("GoShort", CalculationMode.Price, Position.AveragePrice, false);
									isBreakevenSet = true;
									tickCount = 1;
								}
							}
						}
			
						// Step Stop Loss Mode //
			
						if (Position.MarketPosition != MarketPosition.Flat && StopModeSelect == CustomEnumNamespaceWave.StopMode.StepSL && isBreakevenSet == true)
						{
									
							if (Position.MarketPosition == MarketPosition.Long)
							{
								if (Close[0] > Position.AveragePrice + ((slStepSize + (slFrequency * tickCount)) * TickSize)) // adjust higher each time by tickCount
								{
									SetStopLoss("GoLong", CalculationMode.Price, Position.AveragePrice + (((slFrequency * tickCount) - slStepSize) * TickSize), false);
									tickCount ++; // increment to next tick
								}
							}

							if (Position.MarketPosition == MarketPosition.Short)
							{
								if (Close[0] < Position.AveragePrice - ((slStepSize + (slFrequency * tickCount)) * TickSize)) // adjust higher each time by tickCount
								{
									SetStopLoss("GoShort", CalculationMode.Price, Position.AveragePrice - (((slFrequency * tickCount) - slStepSize) * TickSize), false);
									tickCount ++; // increment to next tick
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
				[Display(ResourceType = typeof(Custom.Resource), Name = "Trading Hour Restriction", GroupName = "Time Parameters", Order = 0)]
				public CustomEnumNamespaceWave.TimeMode TIMEMODESelect
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
				[Range(0, int.MaxValue)]
				[Display(Name="Breakeven Profit (Currency)", Order=4, GroupName="Trade Parameters")]
				public int BreakevenProfit
				{ get; set; }
		
				[NinjaScriptProperty]
				[Display(Name="Max Daily Profit", Order=5, GroupName="Trade Parameters")]
				public bool maxDailyProfit
				{ get; set; }
		
				[NinjaScriptProperty]
				[Range(0, int.MaxValue)]
				[Display(Name="Max Daily Profit (Currency)", Order=6, GroupName="Trade Parameters")]
				public int maxDailyProfitAmount
				{ get; set; }
		
				[NinjaScriptProperty]
				[Display(Name="Max Daily Loss", Order=7, GroupName="Trade Parameters")]
				public bool maxDailyLoss
				{ get; set; }
		
				[NinjaScriptProperty]
				[Range(0, int.MaxValue)]
				[Display(Name="Max Daily Loss (Currency)", Order=7, GroupName="Trade Parameters")]
				public int maxDailyLossAmount
				{ get; set; }
                #endregion
				
				[NinjaScriptProperty]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Stoploss Mode Select", GroupName = "SL Parameters", Order = 0)]
				public CustomEnumNamespaceWave.StopMode STOPMODESelect
				{
					get { return StopModeSelect; }
					set { StopModeSelect = value; }
				}
		
				[NinjaScriptProperty]
				[Range(1, int.MaxValue)]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Stoploss Step Distance (Ticks)", GroupName = "SL Parameters", Order = 1)]
				public int slStepSize
				{ get; set; }
		
				[NinjaScriptProperty]
				[Range(1, int.MaxValue)]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Stoploss Step Frequency (Ticks)", GroupName = "SL Parameters", Order = 2)]
				public int slFrequency
				{ get; set; }


        }
}

namespace CustomEnumNamespaceWave
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
