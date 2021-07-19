using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator{
	public class ez_PATI : IndicatorObject {
		
		[Input]
        public int period { get; set; }
		
		public ez_PATI(object _ctx):base(_ctx){
			period = 10;
		}
		
		private IPlotObject plot1;
		
		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
			plot1 = AddPlot(new PlotAttributes("", EPlotShapes.Line, Color.Blue));
		}
		
		protected override void StartCalc() {
			// assign inputs 
		}
		
		protected override void CalcBar(){
			// indicator logic 
            int numDojisOrMarubozus = 0;
            for (int off = 0; off < period; off++)
            {
                if (isDoji(off) || isMarubozu(off))
                {
                    numDojisOrMarubozus++;
                }
            }

            double pati = (double)numDojisOrMarubozus / period;
			
			if (pati > 0.5) {
				Alerts.Alert("PATI over 0.5");
			}
			plot1.Set(0, pati, pati <= 0.5 ? Color.Blue : Color.Red);
		}
		
		private Boolean isDoji(int offset) {
            return Bars.Open[offset] == Bars.Close[offset];
        }

        private Boolean isMarubozu(int offset)
        {
            double hi = Bars.High[offset];
            double lo = Bars.Low[offset];
            double clz = Bars.Close[offset];
            double opn = Bars.Open[offset];
            return hi == clz && lo == opn || lo == clz && hi == opn;
        }
	}
}
