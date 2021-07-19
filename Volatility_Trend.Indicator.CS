using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator{

	public class Volatility_Trend : IndicatorObject
	{
		public Volatility_Trend(object _ctx) : base(_ctx) {
			ATRLength = 14;
			ATRMultiplier = 2.0;
            DnColor = Color.Red;
            UpColor = Color.Green;
			MaxDynamicPeriod = 10;
			//MovAvgValidTypesInfo = "0_Simple_1_eXponential_2_Weighted_3_Triangular";
			ATRMovAvgType = MovAverageATRTypes.TypeMA_Weighted;
		}

		private IPlotObject plot1;

		private VariableSeries<bool> m_isUpDirection;
		private VariableSeries<double> m_atr;
        private VariableSeries<int> m_period1, m_period2, m_period3;
        private VariableSeries<double> m_VTI;

		private XAverage m_xaverage;
		private TriAverage m_triaverage;
		private VariableSeries<Double> m_customATRSeria;

		[Input]
		public int ATRLength { get; set; }

		[Input]
		public double ATRMultiplier { get; set; }

		[Input]
		public int MaxDynamicPeriod { get; set; }

		private ISeries<double> InputData { get; set; }

        [Input]
        public Color UpColor { get; set; }

        [Input]
        public Color DnColor { get; set; }

		[Input]
		public MovAverageATRTypes ATRMovAvgType { get; set; }

		protected override void Create() {
			plot1 = AddPlot(new PlotAttributes("VTI", EPlotShapes.Point, Color.Red, Color.Empty, 5, 0, true));
			m_isUpDirection = new VariableSeries<bool>(this, true);
			m_atr = new VariableSeries<double>(this, .0);
			m_VTI = new VariableSeries<double>(this, .0);
			m_period1 = new VariableSeries<int>(this, 0);
			m_period2 = new VariableSeries<int>(this, 0);
			m_period3 = new VariableSeries<int>(this, 0);

			m_xaverage = new XAverage(this);
			m_triaverage = new TriAverage(this);
			m_customATRSeria = new VariableSeries<double>(this, .0);
		}

		protected override void StartCalc()
		{
			InputData = Bars.Close;

			m_xaverage.Price = m_customATRSeria;
			m_xaverage.Length = ATRLength;

			m_triaverage.price = m_customATRSeria;
			m_triaverage.length = ATRLength;
		}

		protected override void CalcBar()
		{
			m_isUpDirection.Value = PublicFunctions.Iff(InputData[0] > m_VTI[0], true, false);

			m_period1.Value = m_period3[1];
			m_period2.Value = PublicFunctions.Iff(m_isUpDirection.Value != m_isUpDirection[1], 0, m_period1.Value);
			m_period3.Value = PublicFunctions.Iff(m_period2.Value < MaxDynamicPeriod, m_period2.Value + 1, m_period2.Value);

			m_customATRSeria.Value = this.TrueRangeSeries(0).Value;
			switch (ATRMovAvgType)
			{
				case MovAverageATRTypes.TypeMA_Simple:
					m_atr.Value = m_customATRSeria.Average(ATRLength);
					break;
				case MovAverageATRTypes.TypeMA_eXponential:
					m_atr.Value = m_xaverage.Call();
					break;
				case MovAverageATRTypes.TypeMA_Weighted:
					m_atr.Value = m_customATRSeria.WAverageCustom(ATRLength);
					break;
				case MovAverageATRTypes.TypeMA_Triangular:
					m_atr.Value = m_triaverage.Call();
					break;
				default:
					m_atr.Value = m_customATRSeria.Average(ATRLength);
					break;
			}

			m_VTI.Value = PublicFunctions.Iff(m_isUpDirection.Value,
					PublicFunctions.Highest(InputData, m_period3.Value) - ATRMultiplier * m_atr.Value,
					PublicFunctions.Lowest(InputData, m_period3.Value) + ATRMultiplier * m_atr.Value
				);

			plot1.Set(0, m_VTI.Value, PublicFunctions.Iff(m_isUpDirection.Value, UpColor, DnColor));
		}
	}
	
	public enum MovAverageATRTypes {
		TypeMA_Simple,
		TypeMA_eXponential,
		TypeMA_Weighted,
		TypeMA_Triangular
	}
}