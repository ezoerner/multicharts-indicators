using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator{
	public class ez_SwingLines : IndicatorObject {
		
		enum BarType {
			UpBar, DownBar, InsideBar, OutsideBar
		};
		
		[Input]
		public Color swingColor { get; set; }

		
		// currBullLeg
		// currBearLeg
		Boolean inDownLeg = false;
		Boolean inUpLeg = false;
		
		double lastSwingHigh;
		double thisSwingHigh;
		double lastSwingLow;
		double thisSwingLow;
		int lastPivotX;
		int thisPivotX;
		double lastPivotY;
		
		ITrendLineObject lastLine = null;
		// lastBullDiamond
		// lastBearDiamond
		
		public ez_SwingLines(object _ctx):base(_ctx){ }
		
		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
			swingColor = Color.FromArgb(203, 75, 22);  // solarized orange
		}
		
		protected override void StartCalc() {
			// assign inputs 
			lastSwingHigh = Bars.HighValue;
			thisSwingHigh = Bars.HighValue;
			lastSwingLow = Bars.LowValue;
			thisSwingLow = Bars.LowValue;
			lastPivotX = Bars.CurrentBar;
			thisPivotX = Bars.CurrentBar;
			lastPivotY = Bars.CloseValue;		
		}
		
		protected override void CalcBar(){
			if (Bars.Status != EBarState.Close) {
				return;
			}
			
			// indicator logic
			BarType barType;
			if (Bars.HighValue > Bars.High[1] && Bars.LowValue > Bars.Low[1]) {
				barType = BarType.UpBar;
			}
			else if (Bars.HighValue < Bars.High[1] && Bars.LowValue < Bars.Low[1]) {
				barType = BarType.DownBar;
			}
			else if (Bars.HighValue <= Bars.High[1] && Bars.LowValue >= Bars.Low[1]) {
				barType = BarType.InsideBar;
			}
			else {
				barType = BarType.OutsideBar;
			}
			
			double thisY = Bars.CloseValue;
			Boolean plotLine = true;
			//Boolean potentialSecondEntryLong = false;
			//Boolean potentialSecondEntryShort = false;
			
			// Possibilities: Break Up, ContinueDownAndBreakUp, ContinueUp, Break Down, ContinueUpAndBreakDown, Continue Down,
			// or Not Yet In a Swing
			
			Boolean breakUp = !inUpLeg && (barType == BarType.UpBar ||
				(inDownLeg && barType == BarType.OutsideBar && Bars.HighValue > lastSwingHigh));
			Boolean continueUp = inUpLeg && (barType == BarType.UpBar || barType == BarType.InsideBar ||
				(barType == BarType.OutsideBar && Bars.LowValue  >= lastSwingLow));
			Boolean breakDown = !inDownLeg && (barType == BarType.DownBar ||
				(inUpLeg && barType == BarType.OutsideBar && Bars.LowValue < lastSwingLow));
			Boolean continueDown = inDownLeg &&(barType == BarType.DownBar || barType == BarType.InsideBar ||
				(barType == BarType.OutsideBar && Bars.HighValue <= lastSwingHigh));
			Boolean continueUpAndBreakDown = breakDown && Bars.HighValue > thisSwingHigh;
			Boolean continueDownAndBreakUp = breakUp && Bars.LowValue < thisSwingLow;
						
			if (breakUp) {
				#if DEBUG
				Output.WriteLine("CurrentBar = {0}; breakUp", Bars.CurrentBar);
				#endif
				
				inUpLeg = true;
				inDownLeg = false;
				
				if (continueDownAndBreakUp) {
					#if DEBUG
					Output.WriteLine("CurrentBar = {0}; continueDownAndBreakUp", Bars.CurrentBar);
					#endif
					
					thisY = Bars.LowValue;
					thisSwingLow = Bars.LowValue;
					thisPivotX = Bars.CurrentBar;

					lastLine.Delete();
					ChartPoint start0 = new ChartPoint(lastPivotX, lastPivotY);
               		ChartPoint end0 = new ChartPoint(Bars.CurrentBar, thisY);
					DrwTrendLine.Create(start0, end0);
					ChartPoint start = new ChartPoint(Bars.CurrentBar, Bars.LowValue);
               		ChartPoint end = new ChartPoint(Bars.CurrentBar, Bars.HighValue);
					lastLine = DrwTrendLine.Create(start, end);						
					plotLine = false;

				}
				else {
					thisY = Bars.HighValue;
				}
			
				// currBullLeg = thisSwingLow < lastSwingLow ? 1 : currBullLeg + 1;
				// new label
				// array.push(labelIds, newLabel);
				// delete label
				lastPivotX = thisPivotX;
				lastPivotY = thisSwingLow;
				lastSwingLow = thisSwingLow;
				thisSwingHigh = Bars.HighValue;
				thisPivotX = Bars.CurrentBar;
			}
			else if (continueUp) {
				#if DEBUG
				Output.WriteLine("CurrentBar = {0}; continueUp", Bars.CurrentBar);
				#endif
				
				if (Bars.HighValue >= thisSwingHigh) {  // new high in swing
					thisY = Bars.HighValue;
					thisSwingHigh = Bars.HighValue;
					thisPivotX = Bars.CurrentBar;
					if (Bars.Status == EBarState.Close) {
						lastLine.Delete();
					}
				}
				else { // not new high in swing
					plotLine = false;
				}
			}
			else if (breakDown) {
				#if DEBUG
				Output.WriteLine("CurrentBar = {0}; breakDown", Bars.CurrentBar);
				#endif

				inDownLeg = true;
				inUpLeg = false;
				
				if (continueUpAndBreakDown) {
					#if DEBUG
					Output.WriteLine("CurrentBar = {0}; continueUpAndBreakDown", Bars.CurrentBar);
					#endif

					thisY = Bars.HighValue;
					thisSwingHigh = Bars.HighValue;
					thisPivotX = Bars.CurrentBar;

					lastLine.Delete();
					ChartPoint start0 = new ChartPoint(lastPivotX, lastPivotY);
           			ChartPoint end0 = new ChartPoint(Bars.CurrentBar, thisY);
					DrwTrendLine.Create(start0, end0);
					ChartPoint start = new ChartPoint(Bars.CurrentBar, Bars.HighValue);
           			ChartPoint end = new ChartPoint(Bars.CurrentBar, Bars.LowValue);
					lastLine = DrwTrendLine.Create(start, end);
					plotLine = false;

				}
				else {
					thisY = Bars.LowValue;
				}
				
				// currBearLeg..
				// new label
				// array push label
				// label delet
				lastPivotX = thisPivotX;
				lastPivotY = thisSwingHigh;
				lastSwingHigh = thisSwingHigh;
				thisSwingLow = Bars.LowValue;
				thisPivotX = Bars.CurrentBar;
			}
			else if (continueDown) {
				#if DEBUG
				Output.WriteLine("CurrentBar = {0}; continueDown", Bars.CurrentBar);
				#endif
				
				if (Bars.LowValue <= thisSwingLow) {
					thisY = Bars.LowValue;
					thisSwingLow = Bars.LowValue;
					thisPivotX = Bars.CurrentBar;
					lastLine.Delete();
				}
				else {
					plotLine = false;
				}
			}
			else {
				#if DEBUG
				Output.WriteLine("CurrentBar = {0}; (no effect)", Bars.CurrentBar);
				#endif
				
				plotLine = false;
			}
				
			if (plotLine) {
				ChartPoint start = new ChartPoint(lastPivotX, lastPivotY);
       			ChartPoint end = new ChartPoint(Bars.CurrentBar, thisY);
				lastLine = DrwTrendLine.Create(start, end);
			}
		}
	}
}