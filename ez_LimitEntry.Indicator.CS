using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using System.Collections.Generic;
		
namespace PowerLanguage.Indicator{
	
	[RecoverDrawings(true)]
	public class ez_LimitEntry : IndicatorObject {
		
        [Input]
        public int atrlength { get; set; }
		
		[Input]
		public Int16 maxScalpTarget { get; set; }
		
		public ez_LimitEntry(object ctx) : base(ctx) {
		  atrlength = 14;
		  maxScalpTarget = 0; // 0 means no max
		}
				
		protected override void Create() {
		}
		
		protected override void StartCalc() {
			ExecInfo.MaxBarsBack = atrlength;
		}
		
		protected override void CalcBar(){
			Int16 tentativeScalpTarget = Math.Max((Int16)((AvgTrueRange.AverageTrueRange(this, atrlength) * 4 + 2) / 2), (Int16)4);
			Int16 scalpTarget = maxScalpTarget <= 0 ? tentativeScalpTarget : Math.Min(maxScalpTarget, tentativeScalpTarget);
			Int16 barTicks = (Int16)((Bars.HighValue - Bars.LowValue) / 0.25);
			
			int maxStopTicks = scalpTarget * 2;
			int maxSignalBarTicks = maxStopTicks - 2;
			bool goodSignal = barTicks <= maxSignalBarTicks;
			
			List<ITrendLineObject> linesToDelete = Bars.Status == EBarState.Close ?
				DrwTrendLine.GetTrendLineObjects(EDrawingSource.CurrentTech).ToList() :
				new List<ITrendLineObject>();
			List<IArrowObject> arrowsToDelete = Bars.Status == EBarState.Close ?
				DrwArrow.GetArrowObjects(EDrawingSource.CurrentTech).ToList() :
				new List<IArrowObject>();

			if (!goodSignal && Bars.Status == EBarState.Close && Bars.LastBarOnChart && Bars.CurrentBar > atrlength + 1) {
			    double lineSPrice = Bars.HighValue - (maxSignalBarTicks + 1) * 0.25;
				double lineLPrice = Bars.LowValue + (maxSignalBarTicks + 1) * 0.25;
				ITrendLineObject lineS = DrwTrendLine.Create(new ChartPoint(Bars.CurrentBar - 1, lineSPrice), new ChartPoint(Bars.CurrentBar, lineSPrice));
				ITrendLineObject lineL = DrwTrendLine.Create(new ChartPoint(Bars.CurrentBar - 1, lineLPrice), new ChartPoint(Bars.CurrentBar, lineLPrice));
				
				IArrowObject arrowS = DrwArrow.Create(new ChartPoint(Bars.CurrentBar, lineSPrice), true);
				IArrowObject arrowL = DrwArrow.Create(new ChartPoint(Bars.CurrentBar, lineLPrice), false);
				
				lineS.Color = Color.LightSalmon;
				lineS.Size = 2;
			    lineS.Locked = true;
				lineL.Color = Color.LightGreen;
				lineL.Size = 2;
				lineL.Locked = true;
				
				arrowS.Color = Color.LightSalmon;
				arrowS.Size = 3;
			    arrowS.Locked = true;
				arrowL.Color = Color.LightGreen;
				arrowL.Size = 3;
				arrowL.Locked = true;
				arrowL.Style = EArrowForms.ArrowForm0;
				arrowS.Style = EArrowForms.ArrowForm0;
			}
			
			foreach (ITrendLineObject ln in linesToDelete) {
				ln.Delete();
			}
			linesToDelete = null;
			
			foreach (IArrowObject ar in arrowsToDelete) {
				ar.Delete();
			}
		}
	}
}
