using System;
using System.Drawing;

namespace PowerLanguage.Indicator
{
    [SameAsSymbol(true)]
    public class ez_OHLC : IndicatorObject
    {
		public TimeSpan sessionStart {get; set;}
		
		public TimeSpan sessionEnd {get; set;}
		
		[Input]
		public int windDownTimeHours { get; set; }
		
		[Input]
		public int windDownTimeMinutes { get; set; }
		
		private VariableObject<DateTime> m_openTimestamp;
		private VariableObject<DateTime> m_closeTimestamp;
		
		private VariableObject<Double> m_overnightHigh;
		private VariableObject<Double> m_overnightLow;
		
        private VariableObject<Int32> m_counter;

        private VariableObject<Double> m_yestopen;

        private VariableObject<Double> m_todaysopen;

        private VariableObject<Double> m_yesthigh;

        private VariableObject<Double> m_todayshigh;

        private VariableObject<Double> m_yestlow;

        private VariableObject<Double> m_todayslow;

        private VariableObject<Double> m_yestclose;

        private IPlotObject Plot1;

        private IPlotObject Plot2;

        private IPlotObject Plot3;

        private IPlotObject Plot4;
		
		private IPlotObject Plot5;

		private IPlotObject Plot6;


        public ez_OHLC(object ctx) :
            base(ctx) {}

        protected override void Create(){
			sessionStart = new TimeSpan(6, 30, 0);
			sessionEnd = new TimeSpan(13, 15, 0);
			
			windDownTimeHours = 1;
			windDownTimeMinutes = 15;
			
			m_openTimestamp = new VariableObject<DateTime>(this);
			m_closeTimestamp = new VariableObject<DateTime>(this);
			
			m_overnightHigh = new VariableObject<Double>(this);
			m_overnightLow = new VariableObject<Double>(this);
			
            m_counter = new VariableObject<Int32>(this);
            m_yestopen = new VariableObject<Double>(this);
            m_todaysopen = new VariableObject<Double>(this);
            m_yesthigh = new VariableObject<Double>(this);
            m_todayshigh = new VariableObject<Double>(this);
            m_yestlow = new VariableObject<Double>(this);
            m_todayslow = new VariableObject<Double>(this);
            m_yestclose = new VariableObject<Double>(this);
            Plot1 =
                AddPlot(new PlotAttributes("Open", EPlotShapes.LeftTick,
                                           Color.Magenta, Color.Empty, 2,
                                           0,
                                           false));
            Plot2 =
                AddPlot(new PlotAttributes("YestHigh", EPlotShapes.LeftTick,
                                           Color.Blue, Color.Empty, 1, 0,
                                           false));
            Plot3 =
                AddPlot(new PlotAttributes("YestLow", EPlotShapes.LeftTick,
                                           Color.Yellow, Color.Empty, 1,
                                           0,
                                           false));
            Plot4 =
                AddPlot(new PlotAttributes("YestClose", EPlotShapes.LeftTick,
                                           Color.Cyan, Color.Empty, 1, 0,
                                           false));
			Plot5 =
				AddPlot(new PlotAttributes("OvernightHigh", EPlotShapes.LeftTick,
											Color.LightBlue, Color.Empty, 2 ,0,
										   false));
			Plot6 =
				AddPlot(new PlotAttributes("OvernightLow", EPlotShapes.LeftTick,
											Color.Pink, Color.Empty, 2 ,0,
										   false));
			
        }

		protected override void StartCalc() {
			m_closeTimestamp.Value = Bars.TimeValue;
			m_openTimestamp.Value = Bars.TimeValue;
		}
		
        protected override void CalcBar()
        {
            EResolution resolution = Bars.Info.Resolution.Type;
            if (resolution == EResolution.Quarter ||
                EResolution.Week <= resolution && resolution <= EResolution.Year) return;

			if (m_openTimestamp.Value <= Bars.Time[1] &&
					Bars.TimeValue.TimeOfDay.CompareTo(sessionStart) >= 0 && Bars.Time[1].TimeOfDay < sessionStart) {
				m_yestopen.Value = m_todaysopen.Value;
				m_todaysopen.Value = Bars.CloseValue;
				m_openTimestamp.Value = Bars.TimeValue;
						
				m_yesthigh.Value = m_todayshigh.Value;
                m_yestlow.Value = m_todayslow.Value;
                m_todayshigh.Value = Bars.High[0];
                m_todayslow.Value = Bars.Low[0];
						
				DateTime nextClose = Bars.TimeValue.Date.Add(sessionEnd).Subtract(new TimeSpan(0, windDownTimeHours, windDownTimeMinutes, 0, 0));
				ITrendLineObject line = DrwTrendLine.Create(new ChartPoint(nextClose ,0), new ChartPoint(nextClose, Bars.HighValue));
				line.ExtLeft = true;
				line.ExtRight = true;
				line.Style = ETLStyle.ToolDotted;
				line.Color = Color.Magenta;
						
			}
					
			if (m_closeTimestamp.Value <= Bars.Time[1] &&
					Bars.TimeValue.TimeOfDay.CompareTo(sessionEnd) >= 0 && Bars.Time[1].TimeOfDay < sessionEnd) {
				 m_yestclose.Value = Bars.CloseValue;
				 m_closeTimestamp.Value = Bars.TimeValue;
						
				 m_overnightHigh.Value = Bars.HighValue;
				 m_overnightLow.Value = Bars.LowValue;
						
				int daysToAdd = Bars.TimeValue.Date.DayOfWeek == DayOfWeek.Friday ? 3 : 1;
				DateTime nextOpen = Bars.TimeValue.Date.AddDays(daysToAdd).Add(sessionStart);
				ITrendLineObject line = DrwTrendLine.Create(new ChartPoint(nextOpen ,0), new ChartPoint(nextOpen, Bars.HighValue));
				line.ExtLeft = true;
				line.ExtRight = true;
				line.Style = ETLStyle.ToolDotted;
				line.Color = Color.Cyan;
			}
			
            if (Bars.Time[0].Date != Bars.Time[1].Date)
            {
                m_counter.Value = (m_counter.Value + 1);
            }
            else if (Bars.TimeValue.TimeOfDay <= sessionEnd && Bars.TimeValue.TimeOfDay >= sessionStart) {
                if (PublicFunctions.DoubleGreater(Bars.High[0], m_todayshigh.Value)){
                    m_todayshigh.Value = Bars.High[0];
                }
                if (PublicFunctions.DoubleLess(Bars.Low[0], m_todayslow.Value)){
                    m_todayslow.Value = Bars.Low[0];
                }
            }
			else {
				 if (PublicFunctions.DoubleGreater(Bars.High[0], m_overnightHigh.Value)){
                    m_overnightHigh.Value = Bars.High[0];
                }
                if (PublicFunctions.DoubleLess(Bars.Low[0], m_overnightLow.Value)){
                    m_overnightLow.Value = Bars.Low[0];
                }
			}

            if (m_counter.Value >= 2)
            {
                Plot1.Set(0, m_todaysopen.Value);
                Plot2.Set(0, m_yesthigh.Value);
                Plot3.Set(0, m_yestlow.Value);
                Plot4.Set(0, m_yestclose.Value);
				Plot5.Set(0, m_overnightHigh.Value);
				Plot6.Set(0, m_overnightLow.Value);
            }
        }
    }
}