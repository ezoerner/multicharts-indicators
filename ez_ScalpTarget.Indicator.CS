using System;
using System.Drawing;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator
{
    public class ez_ScalpTarget : IndicatorObject
    {
        private VariableSeries<Int16> m_scalp;
		
        private IPlotObject Plot1;
		
		private Int16 prevValue = 0;

        public ez_ScalpTarget(object ctx) : base(ctx){
            atrlength = 14;
        }

        [Input]
        public int atrlength { get; set; }

        protected override void Create(){
            m_scalp = new VariableSeries<Int16>(this);
            Plot1 = AddPlot(new PlotAttributes("Scalp", 0, Color.Cyan, Color.Empty, 0, 0, true));
        }

        protected override void CalcBar(){
            m_scalp.Value = Math.Max((Int16)((AvgTrueRange.AverageTrueRange(this, atrlength) * 4 + 2) / 2), (Int16)4);
			bool alert = m_scalp.Value != prevValue;
			Plot1.Set(0, m_scalp.Value, alert ? Color.Red : Color.Cyan);			
			if (alert) {
                Alerts.Alert("SCALP TARGET: " + m_scalp.Value);
            }
			
			prevValue = m_scalp.Value;
        }
    }
}