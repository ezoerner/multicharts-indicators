using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator{
	public class ez_TrendBars : IndicatorObject {
		
		private IPlotObject plotO;
		private IPlotObject plotC;
		
		[Input]
        public int atrlength { get; set; }
		
		[Input]
		public Color wideUpBarColor { get; set; }
		
		[Input]
		public Color wideDownBarColor { get; set; }

		[Input]
		public Color normalBullTrendColor { get; set; }
		[Input]
		public Color normalBearTrendColor { get; set; }
		[Input]
		public Color wideBullTrendColor { get; set; }
		[Input]
		public Color wideBearTrendColor { get; set; }


		public ez_TrendBars(object _ctx):base(_ctx){
		  atrlength = 21;
		  wideUpBarColor = Color.FromArgb(42, 161, 152);	// solarized cyan
		  wideDownBarColor = Color.FromArgb(211, 54,130);  // solarized magenta
		  normalBullTrendColor = Color.Black;
		  normalBearTrendColor = Color.Black; //Color.FromArgb(181, 137, 0); // solarized yellow
		  wideBullTrendColor = Color.Black;
		  wideBearTrendColor = Color.Black;
		}

		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
			plotO = AddPlot(new PlotAttributes("", EPlotShapes.LeftTick, Color.Red));
			plotC = AddPlot(new PlotAttributes("", EPlotShapes.RightTick, Color.Red));
		}
		protected override void CalcBar(){
			// indicator logic 
			double barRange = Bars.High[0] - Bars.Low[0];
			Boolean isTrendBar = Math.Abs(Bars.Close[0] - Bars.Open[0]) > barRange / 2;
			
			double atr = AvgTrueRange.AverageTrueRange(this, atrlength);
			Boolean isWideBar = barRange > atr;
			
			Boolean isBullBar = Bars.Close[0] > Bars.Open[0];
			Boolean isBearBar = Bars.Close[0] < Bars.Open[0];
			
			if (isBullBar || isBearBar) {
				ChartPoint start = new ChartPoint(Bars.TimeValue, Bars.Close[0]);
				ChartPoint end = new ChartPoint(Bars.TimeValue, Bars.Open[0]);
				Color trendColor = isBullBar ? (isWideBar ? wideBullTrendColor : normalBullTrendColor)
										: (isWideBar ? wideBearTrendColor : normalBearTrendColor);
				
				Color wideColor = isBullBar ? wideUpBarColor : wideDownBarColor;
				
				if (isTrendBar) {				
				  ITrendLineObject line = DrwTrendLine.Create(start,end);
				  line.Color = trendColor;
				  line.Size = 2;
				}
				
				if (isWideBar) {
				  plotO.Set(Bars.Open[0], wideColor);
				  plotC.Set(Bars.Close[0], wideColor);
				}
			}
		}
	}
}