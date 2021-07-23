using System;
using System.Drawing;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator {
	
    [SameAsSymbol(true)]
	public class ez_Congestion : IndicatorObject/*, IChartCustomDrawer */ {
		
/*		public struct ChartRect {
			
			public int leftIndex, rightIndex;
			public double topPrice, bottomPrice;
						
			public ChartRect(int leftIndex, int rightIndex, double topPrice, double bottomPrice)
		    {
		        this.leftIndex = leftIndex;
		        this.rightIndex = rightIndex;
				this.topPrice = topPrice;
				this.bottomPrice = bottomPrice;
		    }
		}
*/
		public class ChartRect {
			private readonly ITrendLineObject line1;
			private readonly ITrendLineObject line2;
			private readonly ITrendLineObject line3;
			private readonly ITrendLineObject line4;
			
			public ChartRect(int leftIndex, int rightIndex, double topPrice, double bottomPrice, CStudyControl studyControl) {
				this.line1 = studyControl.DrwTrendLine.Create(new ChartPoint(leftIndex, topPrice), new ChartPoint(rightIndex, topPrice));;
				this.line2 = studyControl.DrwTrendLine.Create(new ChartPoint(rightIndex, topPrice), new ChartPoint(rightIndex, bottomPrice));
				this.line3 = studyControl.DrwTrendLine.Create(new ChartPoint(rightIndex, bottomPrice), new ChartPoint(leftIndex, bottomPrice));
				this.line4 = studyControl.DrwTrendLine.Create(new ChartPoint(leftIndex, bottomPrice), new ChartPoint(leftIndex, topPrice));
			}
			
			public void setColor(Color color) {
				line1.Color = color;
				line2.Color = color;
				line3.Color = color;
				line4.Color = color;
			}
			
			public void delete() {
				line1.Delete();
				line2.Delete();
				line3.Delete();
				line4.Delete();
			}
		}
		
		private VariableSeries<ChartRect> congestRect;
		
		private VariableSeries<int> congestCount;
		private VariableSeries<Double> congestHigh;
		private VariableSeries<Double> congestLow;
		
		//private ConcurrentQueue<ChartRect> cq = new ConcurrentQueue<ChartRect>();
		
		public ez_Congestion(object _ctx):base(_ctx){}

		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
            congestCount = new VariableSeries<int>(this);
            congestHigh = new VariableSeries<Double>(this);
            congestLow = new VariableSeries<Double>(this);
			
			congestRect = new VariableSeries<ChartRect>(this);
			
 			//ChartCustomDraw.Register(this);
		}
		
		protected override void StartCalc() {
			congestCount.Value = 0;
			congestHigh.Value = Double.NaN;
			congestLow.Value = Double.NaN;
		}
		
		protected override void CalcBar(){
			congestRect.Value = null;
			
			if (Bars.CurrentBar > 0 && Bars.Status == EBarState.Close) {				
				congestCount.Value = 0;
				congestHigh.Value = Double.NaN;
				congestLow.Value = Double.NaN;
				
				if (Bars.CloseValue >= Bars.Low[1] && Bars.CloseValue <= Bars.High[1]) {
					congestCount.Value = congestCount[1] + 1;
					
					if (Double.IsNaN(congestHigh[1]) || Bars.CloseValue > congestHigh[1]) {
						congestHigh.Value = Bars.CloseValue;
					}
					else {
						congestHigh.Value = congestHigh[1];
					}
					
					if (Double.IsNaN(congestLow[1]) || Bars.CloseValue < congestLow[1]) {
						congestLow.Value = Bars.CloseValue;
					}
					else {
						congestLow.Value = congestLow[1];
					}
				}
				#if DEBUG
				Output.WriteLine("congestCount={0}; congestHigh={1}; congestLow={2}; congestHigh[1]={3}; congestLow[1]={4}", congestCount.Value, congestHigh.Value, congestLow.Value, congestHigh[1], congestLow[1]);
				#endif
			}
			
			if (congestCount.Value >= 3 && Bars.Status == EBarState.Close) {
				int leftIndex = Bars.CurrentBar - (congestCount.Value - 1);
				double top  = congestHigh.Value;
				int rightIndex = Bars.CurrentBar;
				double bottom = congestLow.Value;
				
				#if DEBUG
				Output.WriteLine("CongestRect: leftIndex={0}; top={1}; rightIndex={2}; bottom={3}", leftIndex, top, rightIndex, bottom);
				#endif
				
				//cq.Enqueue(rect);				
				ChartRect rect = new ChartRect(leftIndex, rightIndex, top, bottom, this);
				rect.setColor(Color.BlanchedAlmond);
				congestRect.Value = rect;
				if (congestRect[1] != null) {
					congestRect[1].delete();
				}
			}
		}
		/*
		protected override void Destroy(){
            ChartCustomDraw.Unregister(this);
        }

		void IChartCustomDrawer.Draw(DrawContext context, EDrawPhases phase){
			if (phase != EDrawPhases.Final) return;
			
			Brush brush = new SolidBrush(Color.LightSlateGray);
			
			ChartRect rect;
			while(cq.TryDequeue(out rect)) {

				PointF location = context.Environment.ChartPoint2Point(new ChartPoint(rect.leftIndex, rect.topPrice));
				float leftX = context.Environment.BarIndex2PointX((uint)rect.leftIndex);
				float rightX = context.Environment.BarIndex2PointX((uint)rect.rightIndex);
				PointF bottomRight = context.Environment.ChartPoint2Point(new ChartPoint(rect.rightIndex, rect.bottomPrice));
				SizeF size = new SizeF(bottomRight.X - location.X, bottomRight.Y - location.Y);
				
				
				RectangleF drawRect = new RectangleF(leftX, location.Y, rightX - leftX, bottomRight.Y - location.Y);
				
				#if DEBUG
				Output.WriteLine("leftX, rightX = {0}, {1}", leftX, rightX);
				#endif

				
				#if DEBUG
				Output.WriteLine("Dequeued: leftIndex={0}; top={1}; rightIndex={2}; bottom={3} ; location={4}, bottomRight={5}; drawRect= {6}",
					rect.leftIndex, rect.topPrice, rect.rightIndex, rect.bottomPrice, location, bottomRight, drawRect);
				#endif
				
				context.graphics.FillRectangle(brush, drawRect);			
			}
		}
		*/
	}
}