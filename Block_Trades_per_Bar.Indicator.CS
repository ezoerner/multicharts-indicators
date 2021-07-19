using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator{
	public class Block_Trades_per_Bar : IndicatorObject {

		public Block_Trades_per_Bar(object _ctx):base(_ctx){
            datanum = 1;
            blocksize = 5000;
		}

		private int m_blocktrades;
        private int m_barnum;
		private IPlotObject plot1;
	
        [Input]
        public double blocksize { get; set; }

        [Input]
        public int datanum { get; set; }

        protected override void Create(){
			plot1 = AddPlot(new PlotAttributes("BlockTrds", EPlotShapes.Histogram,
                    	Color.Blue, Color.Empty, 1, 0, true));
        }

        protected override void StartCalc(){
            m_blocktrades = 0;
            m_barnum = 0;
		}

        protected override void CalcBar(){
            if ((Bars.LastBarOnChart && (BarsOfData(datanum).Status != EBarState.Close))){
                if ((m_barnum != Bars.CurrentBar)){
                    m_blocktrades = 0;
                    m_barnum = Bars.CurrentBar;
                }
                if (PublicFunctions.DoubleGreaterEquals(Bars.StatusLine.LastVolume, blocksize)){
                    m_blocktrades++;
                }
                plot1.Set(0, m_blocktrades);
            }
        }
	}
}