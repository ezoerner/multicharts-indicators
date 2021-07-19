using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using System.Collections.Generic;
		
namespace PowerLanguage.Indicator{
	
	[RecoverDrawings(true)]
	public class ez_LimitEntry2 : IndicatorObject {
		
		public ez_LimitEntry2(object ctx) : base(ctx) {
		}
				
		protected override void Create() {
		}
		
		protected override void StartCalc() {
			ExecInfo.MaxBarsBack = 1;
		}
		
		protected override void CalcBar(){
			Int16 barTicks = (Int16)((Bars.HighValue - Bars.LowValue) * 4);
			Int16 scalpTarget = (Int16)Math.Max(barTicks + 2, 4);
			
			int maxStopTicks = scalpTarget;
			int maxSignalBarTicks = maxStopTicks - 2;
			bool goodSignal = barTicks <= maxSignalBarTicks;
			
			List<ITrendLineObject> linesToDelete = Bars.Status == EBarState.Close ?
				DrwTrendLine.GetTrendLineObjects(EDrawingSource.CurrentTech).ToList() :
				new List<ITrendLineObject>();
			List<IArrowObject> arrowsToDelete = Bars.Status == EBarState.Close ?
				DrwArrow.GetArrowObjects(EDrawingSource.CurrentTech).ToList() :
				new List<IArrowObject>();

			if (!goodSignal && Bars.Status == EBarState.Close && Bars.LastBarOnChart && Bars.CurrentBar > 2) {
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
