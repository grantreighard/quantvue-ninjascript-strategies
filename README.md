# NinjaScript strategies for QuantVue indicators

These are strategies in C# files to be added to NinjaTrader in order to automate your trading. 

They require an active license from [QuantVue](https://www.quantvue.io) in order to use the indicators.

I am actively forward testing these strategies, the results of which you can see in my [spreadsheet](https://docs.google.com/spreadsheets/d/13GEn5-fEMmgHHa7D3HUARmFuh4muIJzuGl-1CxAjZJw/edit?usp=sharing).

These strategies are public and free to use, but all I ask is that you join the [Discord community](https://discord.gg/tC7u7magU3) and help pay it forward by sharing your results or improvements to the code.

## How to install

There are two ways to go about installing the strategies into your local NinjaTrader program.

1. Download the files and place in `Documents\NinjaTrader 8\bin\Custom\Strategies`, then open the NinjaScript Editor and Compile (right-click > Compile or F5)
2. Copy the code, go to NinjaScript Editor, click + button at bottom, click New Strategy, select all, paste, save, and Compile

## How to use

The strategies can be used for backtesting in the Strategy Analyzer or adding in the Strategies tab after being added and compiled.

We have had good success by back- and forward-testing using the Qrenko data series using 7 shift, 11 offset, 33 range.